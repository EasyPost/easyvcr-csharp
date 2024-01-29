using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EasyVCR.Interfaces;
using EasyVCR.InternalUtilities;
using EasyVCR.RequestElements;

namespace EasyVCR.Handlers
{
    /// <summary>
    ///     A handler that records and replays HTTP requests and responses.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class VCRHandler : DelegatingHandler
    {
        private readonly Cassette _cassette;

        private readonly Censors _censors;
        private readonly TimeSpan? _delay;
        private readonly IInteractionConverter _interactionConverter;
        private readonly ConsoleFallbackLogger _logger;
        private readonly MatchRules _matchRules;
        private readonly Mode _mode;
        private readonly bool _useOriginalDelay;
        private readonly TimeFrame _validTimeFrame;
        private readonly ExpirationActions _whenExpired;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VCRHandler" /> class.
        /// </summary>
        /// <param name="cassette">Cassette to use to record/replay requests.</param>
        /// <param name="mode">Mode to operate in.</param>
        /// <param name="advancedSettings">Advanced settings to use during recording/replaying, optional</param>
        /// <param name="innerHandler">Inner handler to also execute on requests.</param>
        private VCRHandler(Cassette cassette, Mode mode, AdvancedSettings? advancedSettings = null, HttpMessageHandler? innerHandler = null)
        {
            if (innerHandler != null)
                InnerHandler = innerHandler; // despite not being nullable, InnerHandler does not need to be set (is optional): https://learn.microsoft.com/en-us/dotnet/api/System.Net.Http.DelegatingHandler.InnerHandler

            _cassette = cassette;
            _mode = mode;
            _censors = advancedSettings?.Censors ?? new Censors();
            _interactionConverter = advancedSettings?.InteractionConverter ?? new DefaultInteractionConverter();
            _matchRules = advancedSettings?.MatchRules ?? MatchRules.Default;
            _useOriginalDelay = advancedSettings?.SimulateDelay ?? false;
            _delay = advancedSettings?.ManualDelayTimeSpan ?? TimeSpan.Zero;
            _validTimeFrame = advancedSettings?.ValidTimeFrame ?? TimeFrame.Forever;
            _whenExpired = advancedSettings?.WhenExpired ?? ExpirationActions.Warn;
            _logger = new ConsoleFallbackLogger(advancedSettings?.Logger, "EasyVCR");

            // Will throw an exception if an invalid settings combination is provided.
            ExpirationActionExtensions.CheckCompatibleSettings(_whenExpired, _mode);
        }

        // ReSharper disable once InconsistentNaming
        /// <summary>
        ///     Creates a new VCRHandler.
        ///     Available if you want to construct your own HTTP client rather than using the built-in <see cref="EasyVCRHttpClient"/>.
        /// </summary>
        /// <param name="cassette">Cassette object to use.</param>
        /// <param name="mode">Mode to operate in (i.e. Record, Replay, Auto, Bypass).</param>
        /// <param name="advancedSettings">AdvancedSettings object to use.</param>
        /// <param name="innerHandler">Custom inner handler to execute as part of HTTP requests.</param>
        /// <returns>A VCRHandler instance.</returns>
        public static VCRHandler NewVCRHandler(Cassette cassette, Mode mode, AdvancedSettings? advancedSettings = null, HttpMessageHandler? innerHandler = null)
        {
            // This could be done by simply making the constructor public, but this falls in line with the other helper methods in HttpClients.cs
            return new VCRHandler(cassette, mode, advancedSettings, innerHandler);
        }

        /// <summary>
        ///     Override to alter the request-response behavior.
        ///     Record the request and response to the cassette.
        /// </summary>
        /// <param name="request">HttpRequestMessage object.</param>
        /// <param name="cancellationToken">CancellationToken object.</param>
        /// <returns>HttpResponseMessage object.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = new System.Diagnostics.Stopwatch();
            switch (_mode)
            {
                case Mode.Record:
                    // make real request, record response
                    stopwatch.Start();
                    var recordResponse = await base.SendAsync(request, cancellationToken);
                    stopwatch.Stop();
                    await RecordRequestAndResponse(request, recordResponse, stopwatch.Elapsed);
                    return recordResponse;
                case Mode.Replay:
                    // try to get recorded request
                    var replayInteraction = await FindMatchingInteraction(request);
                    if (replayInteraction == null) throw new VCRException($"No interaction found for request {request.Method} {request.RequestUri}");
                    if (_validTimeFrame.HasLapsed(replayInteraction.RecordedAt))
                    {
                        // matching interaction is expired
                        switch (_whenExpired)
                        {
                            case ExpirationActions.Warn:
                                // just throw a warning
                                // will still simulate delay below
                                _logger.Warning("Matching interaction is expired.");
                                break;
                            case ExpirationActions.ThrowException:
                                // throw an exception and exit this function
                                throw new VCRException($"Matching interaction is expired.");
                            case ExpirationActions.RecordAgain:
                                // we should never get here, the settings check should catch this during construction
                                throw new VCRException("Cannot re-record an expired interaction in Replay mode.");
                            default:
                                // we should never get here
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    // simulate delay if configured
                    await SimulateDelay(replayInteraction, cancellationToken);
                    // return matching interaction's response
                    return replayInteraction.Response.ToHttpResponseMessage(request);
                case Mode.Auto:
                    // try to get recorded request
                    var autoInteraction = await FindMatchingInteraction(request);
                    if (autoInteraction != null)
                    {
                        // found a matching interaction
                        if (_validTimeFrame.HasLapsed(autoInteraction.RecordedAt))
                        {
                            // matching interaction is expired
                            switch (_whenExpired)
                            {
                                case ExpirationActions.Warn:
                                    // just throw a warning
                                    // will still simulate delay below
                                    _logger.Warning("Matching interaction is expired.");
                                    break;
                                case ExpirationActions.ThrowException:
                                    // throw an exception and exit this function
                                    throw new VCRException($"Matching interaction is expired.");
                                case ExpirationActions.RecordAgain:
                                    //  re-record over expired interaction
                                    // this will not execute the simulated delay, but since it's making a live request, a real delay will happen.
                                    stopwatch.Start();
                                    var newResponse = await base.SendAsync(request, cancellationToken);
                                    stopwatch.Stop();
                                    await RecordRequestAndResponse(request, newResponse, stopwatch.Elapsed);
                                    // return the new response immediately
                                    return newResponse;
                                default:
                                    // we should never get here
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        // simulate delay if configured
                        await SimulateDelay(autoInteraction, cancellationToken);
                        // return matching interaction's response
                        return autoInteraction.Response.ToHttpResponseMessage(request);
                    }

                    //  no matching interaction, make real request, record response
                    stopwatch.Start();
                    var autoResponse = await base.SendAsync(request, cancellationToken);
                    stopwatch.Stop();
                    await RecordRequestAndResponse(request, autoResponse, stopwatch.Elapsed);
                    // return new interaction's response
                    return autoResponse;
                case Mode.Bypass:
                default:
                    var bypassResponse = await base.SendAsync(request, cancellationToken);
                    return bypassResponse;
            }
        }

        /// <summary>
        ///     Search for an existing interaction that matches the request.
        /// </summary>
        /// <param name="request">HttpRequestMessage request.</param>
        /// <returns>Matching HttpInteraction or null if not found.</returns>
        private async Task<HttpInteraction?> FindMatchingInteraction(HttpRequestMessage request)
        {
            var interactions = _cassette.Read();
            var receivedRequest = await _interactionConverter.ToRequestAsync(request, _censors);

            try
            {
                return interactions.First(i => _matchRules.RequestsMatch(receivedRequest, i.Request));
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Record the request and response to the cassette.
        /// </summary>
        /// <param name="request">Request to record to cassette.</param>
        /// <param name="response">Response to record to cassette.</param>
        /// <param name="requestDuration">TimeSpan of original real request</param>
        /// <param name="bypassSearch">Bypass search for existing interaction. Useful if already known that one does not exist.</param>
        private async Task RecordRequestAndResponse(HttpRequestMessage request, HttpResponseMessage response, TimeSpan requestDuration, bool bypassSearch = false)
        {
            await Task.Run(async () =>
            {
                var interactionRequest = await _interactionConverter.ToRequestAsync(request, _censors);
                var interactionResponse = await _interactionConverter.ToResponseAsync(response, _censors);
                var httpInteraction = new HttpInteraction
                {
                    Request = interactionRequest,
                    Response = interactionResponse,
                    RecordedAt = DateTimeOffset.Now,
                    Duration = requestDuration.Milliseconds
                };
                // always overrides an existing interaction
                _cassette.UpdateInteraction(httpInteraction, _matchRules, bypassSearch);
            });
        }

        private async Task SimulateDelay(HttpInteraction interaction, CancellationToken cancellationToken)
        {
            if (_useOriginalDelay)
            {
                await Task.Delay(interaction.Duration, cancellationToken);
            }
            else if (_delay.HasValue)
            {
                await Task.Delay(_delay.Value, cancellationToken);
            }
        }
    }
}

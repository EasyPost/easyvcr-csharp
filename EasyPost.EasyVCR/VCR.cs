using System;
using System.Net.Http;

namespace EasyPost.EasyVCR
{
    public class VCRSettings
    {
        /// <summary>
        ///     Rules to use when matching requests to recorded responses.
        /// </summary>
        public MatchRules? MatchRules { get; set; }

        /// <summary>
        ///    Censors to apply to the requests and responses.
        /// </summary>
        public Censors? Censors { get; set; }
    }

    public class VCR
    {
        /// <summary>
        ///     The current cassette in the VCR.
        /// </summary>
        private Cassette? _currentCassette;

        /// <summary>
        ///     The name of the current cassette in the VCR.
        /// </summary>
        public string? CassetteName => _currentCassette?.Name;

        /// <summary>
        ///     Retrieve a pre-configured HTTP client that will use the VCR.
        /// </summary>
        /// <exception cref="InvalidOperationException">The VCR has no cassette</exception>
        public HttpClient Client
        {
            get
            {
                if (_currentCassette == null) throw new InvalidOperationException("No cassette is currently loaded.");

                return HttpClients.NewHttpClient(_currentCassette, Mode, Settings?.Censors, Settings?.MatchRules);
            }
        }

        /// <summary>
        ///     The operating mode of the VCR.
        /// </summary>
        public Mode Mode { get; private set; }

        /// <summary>
        ///     Settings for the VCR.
        /// </summary>
        public VCRSettings? Settings { get; set; }

        /// <summary>
        ///     Create a new VCR.
        /// </summary>
        /// <param name="settings">VCRSettings to use.</param>
        public VCR(VCRSettings? settings = null)
        {
            Settings = settings;
        }

        /// <summary>
        ///     Remove the current cassette from the VCR.
        /// </summary>
        public void Eject()
        {
            _currentCassette = null;
        }

        /// <summary>
        ///     Erase the cassette in the VCR.
        /// </summary>
        public void Erase()
        {
            _currentCassette?.Erase();
        }

        /// <summary>
        ///     Add a cassette to the VCR (or replace the current one).
        /// </summary>
        /// <param name="cassette">Cassette to insert.</param>
        public void Insert(Cassette cassette)
        {
            _currentCassette = cassette;
        }

        /// <summary>
        ///     Enable passthrough mode on the VCR (HTTP requests will be made as normal).
        /// </summary>
        public void Pause()
        {
            Mode = Mode.Bypass;
        }

        /// <summary>
        ///     Enable recording mode on the VCR.
        /// </summary>
        public void Record()
        {
            Mode = Mode.Record;
        }

        /// <summary>
        ///     Enable playback mode on the VCR.
        /// </summary>
        public void Replay()
        {
            Mode = Mode.Replay;
        }
    }
}

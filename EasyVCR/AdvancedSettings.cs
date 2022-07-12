using System;
using EasyVCR.Interfaces;
using EasyVCR.InternalUtilities;

namespace EasyVCR
{
    /// <summary>
    ///     Advanced settings for EasyVCR.
    /// </summary>
    public class AdvancedSettings
    {
        /// <summary>
        ///     Censors to use when building requests and responses.
        /// </summary>
        public Censors? Censors = Censors.Default;

        /// <summary>
        ///     Override how HttpRequestMessage and HttpResponseMessage objects are converted to Request and Response objects.
        ///     Not recommended to use unless System.Net.Http has introduced breaking changes.
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        public IInteractionConverter? InteractionConverter = new DefaultInteractionConverter();

        /// <summary>
        ///     Simulate a delay in milliseconds for the request.
        /// </summary>
        public int ManualDelay = 0;
        /// <summary>
        ///     Rules to use when evaluating recordings.
        /// </summary>
        public MatchRules? MatchRules = MatchRules.Default;

        /// <summary>
        ///     Simulate the original request's duration. Overrides <see cref="ManualDelay"/> if set.
        /// </summary>
        public bool SimulateDelay = false;

        /// <summary>
        ///     Retrieve the manual delay as a TimeSpan.
        /// </summary>
        internal TimeSpan? ManualDelayTimeSpan => ManualDelay >= 0 ? TimeSpan.FromMilliseconds(ManualDelay) : TimeSpan.Zero;
    }
}

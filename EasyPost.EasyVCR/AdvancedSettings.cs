using System;
using EasyPost.EasyVCR.InternalUtilities;

namespace EasyPost.EasyVCR
{
    public class AdvancedSettings
    {
        /// <summary>
        ///     Rules to use when evaluating recordings.
        /// </summary>
        public MatchRules? MatchRules = MatchRules.Default;

        /// <summary>
        ///     Censors to use when building requests and responses.
        /// </summary>
        public Censors? Censors = Censors.Default;

        /// <summary>
        ///     Override how HttpRequestMessage and HttpResponseMessage objects are converted to Request and Response objects.
        ///     Not recommended to use unless System.Net.Http has introduced breaking changes.
        /// </summary>
        public IInteractionConverter? InteractionConverter = new DefaultInteractionConverter();

        /// <summary>
        ///     Simulate a delay in milliseconds on a pre-recorded request.
        /// </summary>
        public int SimulateDelay = 0;

        internal TimeSpan? Delay => SimulateDelay > 0 ? TimeSpan.FromMilliseconds(SimulateDelay) : null;
    }
}

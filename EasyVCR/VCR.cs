using System;
using System.Net.Http;

namespace EasyVCR
{
    /// <summary>
    ///     Built-in VCR tool for EasyVCR.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class VCR
    {
        /// <summary>
        ///     The current cassette in the VCR.
        /// </summary>
        private Cassette? _currentCassette;

        /// <summary>
        ///     Advanced settings for the VCR.
        /// </summary>
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public AdvancedSettings? AdvancedSettings { get; set; }

        /// <summary>
        ///     The name of the current cassette in the VCR.
        /// </summary>
        public string? CassetteName => _currentCassette?.Name;

        /// <summary>
        ///     Retrieve a pre-configured HTTP client that will use the VCR.
        /// </summary>
        /// <exception cref="InvalidOperationException">The VCR has no cassette</exception>
        public EasyVCRHttpClient Client
        {
            get
            {
                if (_currentCassette == null) throw new InvalidOperationException("No cassette is currently loaded.");

                return HttpClients.NewHttpClient(_currentCassette, Mode, AdvancedSettings);
            }
        }

        /// <summary>
        ///     The operating mode of the VCR.
        /// </summary>
        public Mode Mode
        {
            get
            {
                // bypass mode overrides all other modes
                if (_mode == Mode.Bypass) return Mode.Bypass;

                var environmentMode = GetModeFromEnvironment();
                return environmentMode ?? _mode;
            }
            private set => _mode = value;
        }

        // ReSharper disable once InconsistentNaming
        private Mode _mode { get; set; }

        /// <summary>
        ///     Create a new VCR.
        /// </summary>
        /// <param name="advancedSettings">AdvancedSettings to use.</param>
        public VCR(AdvancedSettings? advancedSettings = null)
        {
            AdvancedSettings = advancedSettings;
            Mode = Mode.Auto;
        }

        /// <summary>
        ///     Create a clone of the VCR's pre-configured HTTP client.
        /// </summary>
        /// <returns>A pre-configured HTTP client that will use the VCR.</returns>
        public EasyVCRHttpClient CloneClient()
        {
            // The Client property always generates a new client, so we can just use that.
            return Client;
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
        ///     Enable auto mode on the VCR (record if needed, replay otherwise).
        /// </summary>
        public void RecordIfNeeded()
        {
            Mode = Mode.Auto;
        }

        /// <summary>
        ///     Enable playback mode on the VCR.
        /// </summary>
        public void Replay()
        {
            Mode = Mode.Replay;
        }

        private static Mode? GetModeFromEnvironment()
        {
            const string keyName = "EASYVCR_MODE";
            var keyValue = Environment.GetEnvironmentVariable(keyName);
            if (keyValue == null) return null;
            if (Enum.TryParse(keyValue, out Mode mode)) return mode;
            return null;
        }
    }
}

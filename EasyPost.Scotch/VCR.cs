using System;
using System.Collections.Generic;
using System.Net.Http;

namespace EasyPost.Scotch
{
    public class VCRSettings
    {
        public bool HideCredentials { get; set; }
        
        public List<string>? HeadersToHide { get; set; }
    }
    
    public class VCR
    {
        public VCRSettings? Settings { get; set; }
        
        private Cassette? _currentCassette;
        
        private ScotchMode _mode;

        public HttpClient Client
        {
            get
            {
                if (_currentCassette == null)
                {
                    throw new InvalidOperationException("No cassette is currently loaded.");
                }
                return HttpClients.NewHttpClient(_currentCassette, _mode, Settings?.HideCredentials ?? false, Settings?.HeadersToHide);
            }
        }

        public VCR(VCRSettings? settings = null)
        {
            Settings = settings;
        }

        public void Insert(Cassette cassette)
        {
            _currentCassette = cassette;
        }

        public void Eject()
        {
            _currentCassette = null;
        }

        public void Record()
        {
            _mode = ScotchMode.Recording;
        }
        
        public void Replay()
        {
            _mode = ScotchMode.Replaying;
        }
        
        public void Pause()
        {
            _mode = ScotchMode.None;
        }
    }
}

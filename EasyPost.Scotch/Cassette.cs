using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyPost.Scotch.InternalUtilities;
using EasyPost.Scotch.InternalUtilities.JSON;
using EasyPost.Scotch.InternalUtilities.JSON.Orders;
using EasyPost.Scotch.RequestElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyPost.Scotch
{
    public class Cassette
    {
        private static object _fileLocker = new();

        public readonly string FilePath;
        private bool _locked;
        private readonly IOrderOption _orderOption;

        public int Count => Read().ToList().Count;
        
        public Cassette(string folderPath, string cassetteName, IOrderOption? order = null)
        {
            _orderOption = order ?? new CassetteOrder.Alphabetical();
            FilePath = Utils.GetFilePath(folderPath, $"{cassetteName}.json");
        }

        public void Erase()
        {
            File.Delete(FilePath);
        }

        public void Lock()
        {
            _locked = true;
        }

        public void Unlock()
        {
            _locked = false;
        }

        internal IEnumerable<HttpInteraction> Read()
        {
            CheckIfLocked();

            if (!File.Exists(FilePath))
            {
                return Enumerable.Empty<HttpInteraction>();
            }

            var jsonString = File.ReadAllText(FilePath);
            var cassetteParseResult = Serialization.ConvertJsonToObject<List<HttpInteraction>>(jsonString, new VersionConverter());
            if (cassetteParseResult == null)
            {
                throw new VCRException("Could not parse cassette file");
            }
            return cassetteParseResult;
        }

        internal void UpdateInteraction(HttpInteraction httpInteraction)
        {
            lock (_fileLocker)
            {
                var existingInteractions = Read().ToList();
                var matchingIndex = existingInteractions.FindIndex(i => InteractionHelpers.RequestsMatch(httpInteraction.Request, i.Request));
                List<HttpInteraction> newInteractions;
                if (matchingIndex < 0)
                {
                    newInteractions = existingInteractions.Append(httpInteraction).ToList();
                }
                else
                {
                    newInteractions = existingInteractions.ToList();
                    newInteractions[matchingIndex] = httpInteraction;
                }

                Write(newInteractions);
            }
        }

        private void CheckIfLocked()
        {
            if (_locked) throw new VCRException("Cassette is locked");
        }

        private void Write(IEnumerable<HttpInteraction> httpInteraction)
        {
            CheckIfLocked();

            var serializedInteraction = Serialization.ConvertObjectToJson(httpInteraction.ToList(), _orderOption, Formatting.Indented, new VersionConverter());
            if (serializedInteraction == null)
            {
                throw new VCRException("Could not serialize cassette");
            }

            File.WriteAllText(FilePath, serializedInteraction);
        }
    }
}

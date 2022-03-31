using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyPost.EasyVCR.InternalUtilities.JSON;
using EasyPost.EasyVCR.InternalUtilities.JSON.Orders;
using EasyPost.EasyVCR.RequestElements;
using EasyPost.EasyVCR.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyPost.EasyVCR
{
    public class Cassette
    {
        private static object _fileLocker = new();

        public readonly string Name;
        private readonly string _filePath;
        private readonly IOrderOption _orderOption;
        private bool _locked;

        /// <summary>
        ///     Get how many interactions are recorded on this cassette.
        /// </summary>
        public int Count => Read().ToList().Count;

        public Cassette(string folderPath, string cassetteName, IOrderOption? order = null)
        {
            _orderOption = order ?? new CassetteOrder.Alphabetical();
            Name = cassetteName;
            _filePath = Tools.GetFilePath(folderPath, $"{cassetteName}.json");
        }

        /// <summary>
        ///     Erase this cassette by deleting the file
        /// </summary>
        public void Erase()
        {
            File.Delete(_filePath);
        }

        /// <summary>
        ///     Lock this cassette (prevent reading or writing)
        /// </summary>
        public void Lock()
        {
            _locked = true;
        }

        /// <summary>
        ///     Unlock this cassette (allow the cassette to be used)
        /// </summary>
        public void Unlock()
        {
            _locked = false;
        }

        /// <summary>
        ///     Read all the interactions recorded on this cassette
        /// </summary>
        /// <returns>List of HttpInteraction objects.</returns>
        /// <exception cref="VCRException">Unable to parse the cassette file.</exception>
        internal IEnumerable<HttpInteraction> Read()
        {
            CheckIfLocked();

            if (!FileExists()) return Enumerable.Empty<HttpInteraction>();

            var jsonString = File.ReadAllText(_filePath);
            var cassetteParseResult = Serialization.ConvertJsonToObject<List<HttpInteraction>>(jsonString, new VersionConverter());
            if (cassetteParseResult == null) throw new VCRException("Could not parse cassette file");

            return cassetteParseResult;
        }

        /// <summary>
        ///     Overwrite an existing interaction on this cassette, or add a new one if it doesn't exist
        /// </summary>
        /// <param name="httpInteraction">HttpInteraction to write to the cassette.</param>
        /// <param name="matchRules">Set of rules to follow when evaluating if a pair of interactions match.</param>
        internal void UpdateInteraction(HttpInteraction httpInteraction, MatchRules matchRules)
        {
            lock (_fileLocker)
            {
                var existingInteractions = Read().ToList();
                var matchingIndex = existingInteractions.FindIndex(i => matchRules.RequestsMatch(httpInteraction.Request, i.Request));
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

        /// <summary>
        ///     Check if this cassette is locked
        /// </summary>
        /// <exception cref="VCRException">Cassette is locked.</exception>
        private void CheckIfLocked()
        {
            if (_locked) throw new VCRException("Cassette is locked");
        }

        /// <summary>
        ///     Check if this cassette's file exists
        /// </summary>
        /// <returns>True if cassette file exists, false otherwise</returns>
        private bool FileExists()
        {
            return File.Exists(_filePath);
        }

        /// <summary>
        ///     Write a list of interactions to this cassette
        /// </summary>
        /// <param name="httpInteractions">A list of HttpInteraction objects to write to the cassette.</param>
        /// <exception cref="VCRException">Could not write to cassette.</exception>
        private void Write(IEnumerable<HttpInteraction> httpInteractions)
        {
            CheckIfLocked();

            var serializedInteraction = Serialization.ConvertObjectToJson(httpInteractions.ToList(), _orderOption, Formatting.Indented, new VersionConverter());
            if (serializedInteraction == null) throw new VCRException("Could not serialize cassette");

            File.WriteAllText(_filePath, serializedInteraction);
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyPost.Scotch
{
    public static class Cassette
    {
        private static object locker = new();

        public static IEnumerable<HttpInteraction> ReadCassette(string cassettePath)
        {
            if (!File.Exists(cassettePath)) return new List<HttpInteraction>();

            var jsonString = File.ReadAllText(cassettePath);
            var cassetteParseResult = JsonConvert.DeserializeObject<List<HttpInteraction>>(jsonString, new VersionConverter());
            if (cassetteParseResult == null) throw new VCRException("Could not parse cassette file");
            return cassetteParseResult;
        }

        public static void UpdateInteraction(string cassettePath, HttpInteraction httpInteraction)
        {
            lock (locker)
            {
                var existingInteractions = ReadCassette(cassettePath).ToList();
                var matchingIndex = existingInteractions.FindIndex(i => Helpers.RequestsMatch(httpInteraction.Request, i.Request));
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

                WriteCassette(cassettePath, newInteractions);
            }
        }

        private static void WriteCassette(string cassettePath, IEnumerable<HttpInteraction> httpInteraction)
        {
            var serializedInteraction = JsonConvert.SerializeObject(httpInteraction.ToList(), Formatting.Indented, new VersionConverter());
            File.WriteAllText(cassettePath, serializedInteraction.ToString());
        }
    }
}

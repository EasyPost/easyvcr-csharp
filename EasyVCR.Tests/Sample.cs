using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace EasyVCR.Tests
{
    public class Sample
    {
        /// <summary>
        ///     Set advanced options for a cassette, applied to all requests made using the cassette.
        ///     Set specific order options when creating a cassette.
        /// </summary>
        public async Task AdvancedCassetteExample()
        {
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = MatchRules.DefaultStrict, // use the built-in strict match rules
                Censors = Censors.DefaultSensitive, // use the built-in sensitive censors
                SimulateDelay = true // simulate the exact delay of the original request during playback
                // InteractionConverter = new MyInteractionConverter(), // use a custom interaction converter by implementing IInteractionConverter
            };
            var order = new CassetteOrder.Alphabetical(); // elements of each request in a cassette will be ordered alphabetically

            var cassette = new Cassette("path/to/cassettes", "my_cassette", order);

            var httpClient = HttpClients.NewHttpClient(cassette, Mode.Auto, advancedSettings);

            // Use the httpClient as you would normally.
            var response = await httpClient.GetAsync("https://google.com");

            cassette.Lock();
        }

        /// <summary>
        ///     Set global advanced options for VCR, applied to all requests made by the VCR.
        ///     Set specific order options when creating a cassette.
        /// </summary>
        public async Task AdvancedVCRExample()
        {
            var bodyElementsToIgnoreDuringMatch = new List<CensorElement>
            {
                new KeyCensorElement("name", true),
                new KeyCensorElement("phone", false),
            };
            var headerCensors = new List<KeyCensorElement>
            {
                new("X-My-Header", true),
            };
            var queryParameterCensors = new List<KeyCensorElement>
            {
                new("api_key", false),
            };
            var advancedSettings = new AdvancedSettings
            {
                MatchRules = new MatchRules().ByBody(bodyElementsToIgnoreDuringMatch).ByHeader("X-My-Header"), // Match recorded requests by body and a specific header
                Censors = new Censors("redacted").CensorHeaders(headerCensors).CensorQueryParameters(queryParameterCensors), // Redact a specific header and query parameter 
                ManualDelay = 1000, // Simulate a delay of 1 second
            };
            var order = new CassetteOrder.None(); // elements of each request in a cassette will not be ordered any particular way
            var vcr = new VCR(advancedSettings);
            var cassette = new Cassette("path/to/cassettes", "my_cassette", order);
            vcr.Insert(cassette);
            vcr.Record();

            var httpClient = vcr.Client;

            // Use the httpClient as you would normally.
            var response = await httpClient.GetAsync("https://google.com");

            vcr.Eject();
        }

        /// <summary>
        ///    Create a cassette, use the cassette to get an HttpClient, and make a request.
        /// </summary>
        public async Task SimpleCassetteExample()
        {
            var cassette = new Cassette("path/to/cassettes", "my_cassette");

            var httpClient = HttpClients.NewHttpClient(cassette, Mode.Auto);

            // Use the httpClient as you would normally.
            var response = await httpClient.GetAsync("https://google.com");

            cassette.Lock();
        }

        /// <summary>
        ///     Create a VCR, create a cassette, insert cassette into VCR, get the EasyVcrHttpClient from the VCR and make a request.
        /// </summary>
        public async Task SimpleVCRExample()
        {
            var vcr = new VCR();
            var cassette = new Cassette("path/to/cassettes", "my_cassette");
            vcr.Insert(cassette);
            vcr.Record();

            var httpClient = vcr.Client;

            // Use the httpClient as you would normally.
            var response = await httpClient.GetAsync("https://google.com");

            vcr.Eject();
        }
    }
}

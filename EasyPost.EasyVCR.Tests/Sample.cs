using System.Threading.Tasks;

namespace EasyPost.EasyVCR.Tests
{
    public class Sample
    {
        public async Task SampleFunction()
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

        public async Task SampleFunction2()
        {
            var cassette = new Cassette("path/to/cassettes", "my_cassette");

            var httpClient = HttpClients.NewHttpClient(cassette, Mode.Auto);
            
            // Use the httpClient as you would normally.
            var response = await httpClient.GetAsync("https://google.com");
            
            cassette.Lock();
        }
    }
}

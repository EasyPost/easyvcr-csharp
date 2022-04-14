using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EasyPost.EasyVCR.Tests
{
    public class Post
    {
        [JsonProperty("user_id")]
        internal int UserId { get; }
        [JsonProperty("id")]
        internal int Id { get; }
        [JsonProperty("title")]
        internal string Title { get; }
        [JsonProperty("body")]
        internal string Body { get; }

        public Post(int userId, int id, string title, string body)
        {
            UserId = userId;
            Id = id;
            Title = title;
            Body = body;
        }
    }

    public class FakeDataService
    {
        private readonly HttpClient? _client;
        private readonly VCR? _vcr;

        public HttpClient Client
        {
            get
            {
                if (_client != null) return _client;

                if (_vcr != null) return _vcr.Client;

                throw new InvalidOperationException("No VCR or HttpClient has been set.");
            }
        }

        public FakeDataService(VCR vcr)
        {
            _vcr = vcr;
        }

        public FakeDataService(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Post>?> GetPosts()
        {
            var response = await GetPostsRawResponse();
            return JsonConvert.DeserializeObject<List<Post>>(await response.Content.ReadAsStringAsync());
        }

        public async Task<Post?> GetPost(int id)
        {
            var response = await GetPostRawResponse(id);
            return JsonConvert.DeserializeObject<Post>(await response.Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> GetPostsRawResponse()
        {
            return await Client.GetAsync("https://jsonplaceholder.typicode.com/posts");
        }

        public async Task<HttpResponseMessage> GetPostRawResponse(int id)
        {
            return await Client.GetAsync($"https://jsonplaceholder.typicode.com/posts/{id}");
        }
    }
}

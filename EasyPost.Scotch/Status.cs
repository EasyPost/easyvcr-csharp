using System.Net;

namespace EasyPost.Scotch
{
    public class Status
    {
        public HttpStatusCode Code { get; set; }
        public string? Message { get; set; }
    }
}

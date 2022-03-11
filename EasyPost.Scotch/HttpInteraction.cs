using System;

namespace EasyPost.Scotch
{
    public class HttpInteraction
    {
        public DateTimeOffset RecordedAt { get; set; }
        public Request Request { get; set; }
        public Response Response { get; set; }
    }
}

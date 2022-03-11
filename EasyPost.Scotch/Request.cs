using System.Collections.Generic;

namespace EasyPost.Scotch;

public class Request
{
    public string? Body { get; set; }
    public IDictionary<string, string>? ContentHeaders { get; set; }
    public string Method { get; set; }
    public IDictionary<string, string> RequestHeaders { get; set; }
    public string Uri { get; set; }
}

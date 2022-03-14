using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace EasyPost.Scotch;

public class Response
{
    public string? Body { get; set; }
    public IDictionary<string, string>? ContentHeaders { get; set; }

    public Version HttpVersion { get; set; }
    public IDictionary<string, string>? ResponseHeaders { get; set; }
    public Status Status { get; set; }

    public HttpResponseMessage ToHttpResponseMessage(HttpRequestMessage requestMessage)
    {
        var result = new HttpResponseMessage(Status.Code);
        result.ReasonPhrase = Status.Message;
        result.Version = HttpVersion;
        foreach (var h in ResponseHeaders ?? new Dictionary<string, string>()) result.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

        var content = new ByteArrayContent(Encoding.UTF8.GetBytes(Body ?? string.Empty));
        foreach (var h in ContentHeaders ?? new Dictionary<string, string>()) content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToString());

        result.Content = content;
        result.RequestMessage = requestMessage;
        return result;
    }
}

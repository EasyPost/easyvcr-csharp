# Scotch

### Disclaimer
This is a fork of the original [Scotch by Martin Leech](https://github.com/mleech/scotch), updated to address outstanding functionality issues, as well as introduce a handful of new features. This version of Scotch is a C# rewrite of the original F# version.

## What is Scotch?
Scotch is a library for recording and replaying HTTP interactions in your test suite. 

This can be useful for speeding up your test suite, or for running your tests on a CI server which doesn't have connectivity to the HTTP endpoints you need to interact with.

Scotch is based on the [VCR Ruby gem](https://github.com/vcr/vcr).

### Step 1.
Run your test suite locally against a real HTTP endpoint in recording mode

```csharp
using EasyPost.Scotch;

// Create a HttpClient which uses a RecordingHandler
var vcrHttpClient = HttpClients.NewHttpClient(pathToCassetteFile, ScotchMode.Recording);

// Use this HttpClient in any class making HTTP calls
// For example, RestSharp v107+ supports custom HTTP clients
RestClient restClient = new RestClient(vcrHttpClient, new RestClientOptions()));
```
Real HTTP calls will be made and recorded to the cassette file.

### Step 2.
Switch to replay mode:
```csharp
using EasyPost.Scotch;

var httpClient = HttpClients.NewHttpClient(pathToCassetteFile, ScotchMode.Replaying);
```
Now when tests are run no real HTTP calls will be made. Instead, the HTTP responses will be replayed from the cassette file. 

Requests are matched by both HTTP method (i.e. `GET`, `POST`) and full URL.

### Extra Features
- Prevent sensitive headers from being recorded to cassette:
```csharp
using EasyPost.Scotch;

// Censor default sensitive headers (i.e. `Authorization`)
var vcrHttpClient = HttpClients.NewHttpClient(pathToCassetteFile, ScotchMode.Recording, hideCredentials: true);

// Indicate specific headers to censor
var vcrHttpClient = HttpClients.NewHttpClient(pathToCassetteFile, ScotchMode.Recording, hideCredentials: true, headersToHide: new string[] { "MyCustomHeader" });
```

## Why "Scotch"?
In keeping with the VCR theme, Scotch was a famous brand of VHS cassettes with [a particularly catchy ad campaign](https://www.youtube.com/watch?v=lgqTajuemp0).

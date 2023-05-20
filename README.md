# EasyVCR

[![CI](https://github.com/EasyPost/easyvcr-csharp/workflows/CI/badge.svg)](https://github.com/EasyPost/easyvcr-csharp/actions?query=workflow%3ACI)
[![Coverage Status](https://coveralls.io/repos/github/EasyPost/easyvcr-csharp/badge.svg?branch=master)](https://coveralls.io/github/EasyPost/easyvcr-csharp?branch=master)
[![NuGet](https://img.shields.io/nuget/dt/EasyVCR)](https://www.nuget.org/packages/EasyVCR)

EasyVCR is a library for recording and replaying HTTP interactions in your test suite.

This can be useful for speeding up your test suite, or for running your tests on a CI server which doesn't have
connectivity to the HTTP endpoints you need to interact with.

## How to use EasyVCR

#### Step 1.

Run your test suite locally against a real HTTP endpoint in recording mode

```csharp
using EasyVCR;

// Create a cassette to handle HTTP interactions
var cassette = new Cassette("path/to/cassettes", "my_cassette");

// create an EasyVcrHttpClient using the cassette
var recordingHttpClient = HttpClients.NewHttpClient(cassette, Mode.Record);

// Use this EasyVcrHttpClient in any class making HTTP calls
// Note: EasyVcrHttpClient implements HttpClient, so it can be used anywhere a HttpClient is expected
// For example, RestSharp v107+ supports custom HTTP clients
RestClient restClient = new RestClient(recordingHttpClient, new RestClientOptions()));

// Or make HTTP calls directly
var response = await recordingHttpClient.GetAsync("https://api.example.com/v1/users");
```

Real HTTP calls will be made and recorded to the cassette file.

#### Step 2.

Switch to replay mode:

```csharp
using EasyVCR;

// Create a cassette to handle HTTP interactions
var cassette = new Cassette("path/to/cassettes", "my_cassette");

// create an EasyVcrHttpClient using the cassette
var replayingHttpClient = HttpClients.NewHttpClient(cassette, Mode.Replay);
```

Now when tests are run, no real HTTP calls will be made. Instead, the HTTP responses will be replayed from the cassette
file.

### Available modes

- `Mode.Auto`:  Play back a request if it has been recorded before, or record a new one if not. (default mode for `VCR`)
- `Mode.Record`: Record a request, including overwriting any existing matching recording.
- `Mode.Replay`: Replay a request. Throws an exception if no matching recording is found.
- `Mode.Bypass`:  Do not record or replay any requests (client will behave like a normal HttpClient).

## Features

`EasyVCR` comes with a number of features, many of which can be customized via the `AdvancedOptions` class.

### Censoring

Censor sensitive data in the request and response, such as API keys and auth tokens.

Can censor:
- Request and response headers (via key name)
- Request and response bodies (via key name) (JSON only)
- Request query parameters (via key name)
- Request URL path elements (via regex pattern matching)

**Default**: *Disabled*

```csharp
using EasyVCR;

var cassette = new Cassette("path/to/cassettes", "my_cassette");

var headerCensors = new List<KeyCensorElement> {
    new("Authorization", false), // Hide the Authorization header
};
var bodyCensors = new List<KeyCensorElement> {
    new("table", true), // Hide the table element (case sensitive) in the request and response body
};
var pathCensors = new List<PatternCensorElement> {
    new(".*\\d{4}.*"), // Hide any path element that contains 4 digits
};
var censors = new Censors().CensorHeaders(headerCensors)
                           .CensorBodyElements(bodyCensors)
                           .CensorPathElements(pathCensors);

var advancedOptions = new AdvancedOptions()
{
    Censors = censors
};

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Record, advancedSettings);
```

### Delay

Simulate a delay when replaying a recorded request, either using a specified delay or the original request duration.

NOTE: Delays may suffer from a small margin of error on certain .NET versions. Do not rely on the delay being exact down
to the millisecond.

**Default**: *No delay*

```csharp
using EasyVCR;

var cassette = new Cassette("path/to/cassettes", "my_cassette");
var advancedOptions = new AdvancedOptions()
{
    SimulateDelay = true, // Simulate a delay of the original request duration when replaying (overrides ManualDelay)
    ManualDelay = 1000 // Simulate a delay of 1000 milliseconds when replaying
};

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
```

### Expiration

Set expiration dates for recorded requests, and decide what to do with expired recordings.

**Default**: *No expiration*

```csharp
using EasyVCR;

var cassette = new Cassette("path/to/cassettes", "my_cassette");
var advancedOptions = new AdvancedOptions()
{
    ValidTimeFrame = new TimeFrame() {  // Any matching request is considered expired if it was recorded more than 30 days ago
        Days = 30,
    },
    WhenExpired = ExpirationActions.ThrowException // Throw exception if the recording is expired
};

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
```

### Matching

Customize how a recorded request is determined to be a match to the current request.

**Default**: *Method and full URL must match*

```csharp
using EasyVCR;

var cassette = new Cassette("path/to/cassettes", "my_cassette");
var advancedOptions = new AdvancedOptions()
{
    MatchRules = new MatchRules().ByBody().ByHeader("X-My-Header"), // Match recorded requests by body and a specific header
};

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
```

### Ordering

Customize how elements of a recorded request are organized in the cassette file.
Helpful to avoid unnecessary git differences between cassette file versions.

**Default**: *Elements are stored alphabetically*

**NOTE:** This setting must be used when creating the cassette.

```csharp
using EasyVCR;

var order = new CassetteOrder.None(); // elements of each request in a cassette won't be ordered in any particular way
var cassette = new Cassette("path/to/cassettes", "my_cassette", order);

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
```

### Logging

Have EasyVCR integrate with your custom logger to log warnings and errors.

**Default**: *Logs to console*

```csharp
using EasyVCR;

var cassette = new Cassette("path/to/cassettes", "my_cassette");
var advancedOptions = new AdvancedOptions()
{
    Logger = new MyCustomLogger(), // Have EasyVCR use your custom logger when making log entries
};

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
```

### HttpClient Conversion

Override how HttpClient request and response objects are converted into `EasyVCR` request and response objects, and vice
versa.
Useful if `HttpClient` suffers breaking changes in future .NET versions.

```csharp
using EasyVCR;

var cassette = new Cassette("path/to/cassettes", "my_cassette");
var advancedOptions = new AdvancedOptions()
{
    InteractionConverter = new MyInteractionConverter(), // use a custom interaction converter by implementing IInteractionConverter
};

var httpClient = HttpClients.NewHttpClient(cassette, Mode.Replay, advancedSettings);
```

## VCR

In addition to individual recordable HttpClient instances, `EasyVCR` also offers a built-in VCR, which can be used to
easily switch between multiple cassettes and/or modes. Any advanced settings applied to the VCR will be applied on every
request made using the VCR's HttpClient.

```csharp
using EasyVCR;

var queryParameterCensors = new List<KeyCensorElement> {
    new("api_key", true), // Hide the api_key query parameter
};
var advancedSettings = new AdvancedSettings
{
    Censors = new Censors().CensorQueryParameters(queryParameterCensors)
};

// Create a VCR with the advanced settings applied
var vcr = new VCR(advancedSettings);

// Create a cassette and add it to the VCR
var cassette = new Cassette("path/to/cassettes", "my_cassette");
vcr.Insert(cassette);
       
// Set the VCR to record mode     
vcr.Record();
            
// Get an HttpClient using the VCR
var httpClient = vcr.Client;
            
// Use the HttpClient as you would normally.
var response = await httpClient.GetAsync("https://google.com");

// Remove the cassette from the VCR            
vcr.Eject();
```

#### Credit

- [Scotch by Martin Leech](https://github.com/mleech/scotch), whose core functionality on which this is based.

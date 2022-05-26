# CHANGELOG

## v0.3.1 (2022-05-26)

- Improvements to censoring
  - Null values won't be replaced during censoring, avoid interfering with de/serialization
  - Censoring runs recursively through nested dictionaries and lists

## v0.3.0 (2022-05-24)

- Performance improvements to censoring
  - Censoring process is skipped if there are no censors to apply
  - Body censors apply to nested JSON body keys, not just top-level keys
    - Lists replaced with empty lists, dictionaries replaced with empty dictionaries, primitives replaced with censor text
- Censor declaration functions expect lists of multiple keys

## v0.2.0 (2022-05-18)

- Fix match-by-body functionality by normalizing the body before comparing it.
- Account for non-JSON response bodies.
- Recorded requests now tagged with custom headers, utility to check if a response came from a recorded request or a
  live request.

## v0.1.0 (2022-04-25)

- Initial release
- Support for .NET Core 3.1, .NET 5.0, .NET 6.0 and .NET Standard 2.0
- Support for Auto mode (replay a request if it exists, otherwise record a new one)
- Introduce `VCR` as a singleton users can use to track cassettes and mode with unified settings
  - For users not wanting to use `VCR`, a recording-capable `HttpClient` can be retrieved via the `HttpClients` class
- Support for Bypass mode (skip cassette process, make a normal HTTP request)
- Support for storing mode in environment variables as `EASYVCR_MODE`
  - Bypass mode overrides environment variable
  - Only available in `VCR`
- Support for advanced settings via `AdvancedSettings`
  - Simulate a delay on each pre-recorded request via `SimulateDelay`
  - Override default rules to hide headers, query parameters and body parameters in cassettes via `Censors`
  - Override default rules when determining if a request matches an existing record via `MatchRules` (advanced)
  - Override default conversion of HttpRequestMessage and HttpResponseMessage objects via `IInteractionConverter`(advanced)
    - This will hopefully allow users to adjust in case System.Net.Http introduces breaking changes in the future, without requiring an update to this package.
- Support for custom ordering of JSON in cassette files via `CassetteOrder` and `IOrderOption` (advanced)
  - Cassette elements default to being ordered alphabetically

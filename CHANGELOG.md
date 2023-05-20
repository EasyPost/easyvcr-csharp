# CHANGELOG

## Next Release

- Add support for censoring XML, HTML and plain text bodies
- [BREAKING CHANGE] `CensorElement` now abstract base class, cannot be used directly
  - Three types of `CensorElement` options available:
    - `KeyCensorElement`: Censor the value of a specified key (will be ignored if used for plain text/HTML data)
    - `RegexCensorElement`: Censor any string that matches a specified regex pattern (will check the value of a key-value pair if used for JSON/XML data)
    - `TextCensorElement`: Censor a specified string (will check the value of a key-value pair if used for JSON/XML data; requires the whole body to match the specified string if used for plain text/HTML data)
  - Body censoring: `KeyCensorElement` (recommended for JSON/XML if key is known), `TextCensorElement` (recommended for JSON/XML if value is known), and `RegexCensorElement` (recommended for plain text/HTML)
  - Path element censoring: Use `RegexCensorElement`
  - Query parameter censoring: Use `KeyCensorElement`
  - Header censoring: Use `KeyCensorElement`
- [BREAKING CHANGE] `CensorHeadersByKeys`, `CensorBodyElementsByKeys`, `CensorQueryParametersByKeys` and `CensorPathElementsByPatterns` removed
  - Use `CensorHeaders`, `CensorBodyElements`, `CensorQueryParameters` and `CensorPathElements` instead

## v0.9.0 (2023-05-17)

- Fix a bug where URLs were not being extracted correctly, potentially causing false matches when matching by URL
- New match rules for matching by query parameters, by path or by custom user-defined rules

## v0.8.0 (2022-12-20)

- Adds ability to censor parts of a URL path using regex patterns
- Fixes a bug that would throw error if trying to "match by body" with non-JSON bodies.

## v0.7.0 (2022-11-15)

- Add support for .NET 7

## v0.6.0 (2022-10-19)

- Add support for client cloning (useful when trying to re-use EasyVCR's HTTP client as an inner client inside multiple HTTP clients)
  - This avoids an error where some clients, like RestSharp, expect the inner client to be a new instance each time
- EasyVCR now uses an `EasyVCRHTTPClient` class instead of `HttpClient` to allow for internal functionality
  - `EasyVCRHTTPClient` implements `HttpClient`, so it can be used anywhere a `HttpClient` is expected

## v0.5.1 (2022-10-05)

- Fix missing NuGet dependency for custom logger

## v0.5.0 (2022-10-04)

- New feature: Set expiration time for interactions (how long since it was recorded should an interaction be considered valid)
  - Can determine what to do if a matching interaction is considered invalid:
    - Warn the user, but proceed with the interaction
    - Throw an exception
    - Automatically re-record (cannot be used in `Replay` mode)
- New feature: Pass in a custom ILogger instance to EasyVCR to funnel log messages into your own logging setup (fallback: logs to console)
- Improvement: Clarify that Delay may not be exact

## v0.4.0 (2022-06-13)

- Improvements to censoring
  - Ability to define censored elements individually, with per-element case sensitivity
- Improvements to matching
  - Ability to ignore certain elements when matching by body

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

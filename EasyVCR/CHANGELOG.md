# Changelog

## v0.2.0 (2022-05-18)

- Fix match-by-body functionality by normalizing the body before comparing it.
- Account for non-JSON response bodies.
- Recorded requests now tagged with custom headers, utility to check if a response came from a recorded request or a
  live request.

## Initial Release - v0.1.0 (2022-04-25)

- Record and replay HTTP interactions, including support for censors, delays and custom match rules.
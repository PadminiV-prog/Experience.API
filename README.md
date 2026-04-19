# Experience API

## Solution Structure

- `ExperienceApi/` - Azure Functions isolated worker project for Experience API
- `ExperienceApi.Tests/` - xUnit test project for function unit tests

## Projects

- `ExperienceApi`: HTTP-triggered serverless API (`POST /experience/process`) with middleware for correlation/source headers and service abstractions for validation, token acquisition, and downstream HTTP calls.
- `ExperienceApi.Tests`: Unit tests for `ProcessRequestFunction` covering success, bad request, and internal server error scenarios.

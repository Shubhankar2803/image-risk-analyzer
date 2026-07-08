Backend tests (xUnit)

This folder contains a starter xUnit test project for the API.

Run tests:

1. From the repo root (recommended):

```powershell
cd backend/tests
dotnet test
```

Notes:
- The test project references the API project at `../src/RiskAnalyzer.Api/RiskAnalyzer.Api.csproj`.
- `ApiStartupTests` demonstrates creating an in-memory `WebApplicationFactory<Program>` and a client. Add unit tests for services, and integration tests that call specific endpoints.
- For unit tests that target individual services, add new test classes under this folder and reference/construct the service classes or use mocks (Moq).

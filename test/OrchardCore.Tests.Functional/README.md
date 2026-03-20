# Functional Tests (`OrchardCore.Tests.Functional`)

End-to-end browser tests for OrchardCore using [Playwright](https://playwright.dev/dotnet/) and xUnit. The tests launch OrchardCore in-process on a dynamic port, then drive a headless Chromium browser to verify setup, theming, login, and multi-tenancy.

## Prerequisites

- .NET SDK (current version)
- Playwright Chromium browser (installed automatically on first build, or manually via the command below)

## Running the tests

Build the project and install the Playwright browser:

```bash
dotnet build -c Release test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj
pwsh test/OrchardCore.Tests.Functional/bin/Release/net10.0/playwright.ps1 install chromium
```

Run all tests:

```bash
dotnet test --project test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj -c Release --no-build
```

Run only CMS or MVC tests:

```bash
dotnet test --project test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj -c Release --no-build --filter-class "*Cms*"
dotnet test --project test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj -c Release --no-build --filter-class "*Mvc*"
```

## Architecture

### In-process hosting

Tests use `OrchardTestServer`, which builds and starts OrchardCore in-process using `WebApplication.CreateBuilder` with the same configuration as the actual `Program.cs` entry points. Each test class fixture gets its own server instance on a dynamic port, with an isolated `App_Data_Tests_{FixtureName}` directory.

### Parallel execution

Each CMS recipe test class has its own `IClassFixture<T>` (e.g., `AgencyFixture`, `BlogFixture`) that starts an independent OrchardCore instance with an isolated `App_Data_Tests_{FixtureName}` directory. Test classes run serially (`maxParallelThreads: 1` in `xunit.runner.json`) to avoid database conflicts when using shared database servers (MySQL, PostgreSQL, SQL Server) in CI.

### Test structure

```
test/OrchardCore.Tests.Functional/
  Helpers/
    OrchardTestServer.cs       # Builds and runs OrchardCore in-process
    OrchardTestFixture.cs      # Manages Playwright browser + server lifecycle
    InMemoryLoggerProvider.cs  # Captures log entries for assertion
    AuthHelper.cs              # IPage.LoginAsync() extension
    TenantHelper.cs            # IPage.SiteSetupAsync(), CreateTenantAsync() extensions
    ButtonHelper.cs            # IPage.ClickSaveAsync() etc. extensions
    ...
  Tests/
    Cms/
      CmsRecipeFixture.cs      # Base fixture: starts app, runs setup with a recipe
      CmsTestBase.cs           # Base test class with log assertion in teardown
      AgencyTests.cs           # Tests for the Agency recipe
      BlogTests.cs             # Tests for the Blog recipe
      SaasFixture.cs           # Fixture for SaaS multi-tenancy tests
      SaasTests.cs             # Tests tenant creation and isolation
      ...
    Mvc/
      MvcSetupFixture.cs       # Fixture for the MVC HelloWorld app
      MvcTests.cs              # Smoke test for MVC module
  Fixtures/
    migrations.recipe.json     # Custom recipe for testing database migrations
```

### Helper extensions

All helpers are extension methods on Playwright's `IPage`, enabling fluent test code:

```csharp
var page = await Fixture.CreatePageAsync();
await page.LoginAsync();
await page.GotoAsync("/Admin");
await Assertions.Expect(page.Locator(".menu-admin")).ToHaveAttributeAsync("id", "adminMenu");
await page.CloseAsync();
```

### Log assertions

An `InMemoryLoggerProvider` captures all Warning+ log entries during the test. After each test, `AssertNoLoggedErrors()` verifies no Error or Critical messages were logged, catching server-side issues that may be invisible in the browser.

## Environment variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | App environment (`Development` or `Production`) | `Production` |
| `PLAYWRIGHT_TRACING` | Enable Playwright tracing (set to any value) | Disabled |
| `ORCHARD_EXTERNAL` | Skip in-process hosting, use an externally running app | Not set |
| `ORCHARD_URL` | Base URL when using `ORCHARD_EXTERNAL` | `http://localhost:5000` |
| `OrchardCore__DatabaseProvider` | Database provider (`Postgres`, `MySql`, `SqlConnection`) | SQLite |
| `OrchardCore__ConnectionString` | Connection string for the database provider | Not set |

## Playwright tracing

When `PLAYWRIGHT_TRACING` is set, traces are saved to `test/OrchardCore.Tests.Functional/traces/`. View them at <https://trace.playwright.dev>.

## CI

The `functional_all_db.yml` workflow runs these tests against SQLite, PostgreSQL, MySQL, and SQL Server. Tracing is enabled by default in CI, and trace files are uploaded as artifacts on failure.

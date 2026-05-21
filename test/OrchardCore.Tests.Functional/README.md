# Functional Tests (`OrchardCore.Tests.Functional`)

End-to-end browser tests for OrchardCore using [Playwright](https://playwright.dev/dotnet/) and xUnit. The tests launch OrchardCore in-process on a dynamic port, then drive a headless Chromium browser to verify setup, theming, login, and multi-tenancy.

## Prerequisites

- .NET SDK (current version)
- Playwright Chromium browser (installed automatically on first build, or manually via the command below)

## Running the tests

Build the project and install the Playwright browser:

```bash
dotnet build -c Release test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj
dotnet exec test/OrchardCore.Tests.Functional/bin/Release/net10.0/Microsoft.Playwright.dll install chromium
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

Tests use `OrchardTestServer`, which builds and starts OrchardCore in-process using `WebApplication.CreateBuilder` with the same configuration as the actual `Program.cs` entry points. Each test class fixture gets its own server instance on a dynamic port, with an isolated `App_Data_Tests_{FixtureName}` directory and (when using shared databases) a fixture-specific database.

### Parallel execution

Test fixtures run in parallel. Each fixture is fully isolated:

- **App data**: Each fixture gets its own `App_Data_Tests_{FixtureName}` directory.
- **Database**: When using shared database servers (MySQL, PostgreSQL, SQL Server), each fixture drops and recreates its own database (e.g., `app_AgencyFixture`) to ensure a clean state.
- **Configuration**: Database env vars (`OrchardCore__ConnectionString`, `OrchardCore__DatabaseProvider`) are captured once at process start and immediately cleared. Each host receives its per-fixture connection string via `tenants.json` and `builder.Configuration`.
- **Recipes**: Test-only recipes are served from embedded assembly resources via `EmbeddedRecipeHarvester`, eliminating shared filesystem state.

Parallelism is controlled by `maxParallelThreads` in `xunit.runner.json` (defaults to CPU core count when omitted).

### Test structure

```
test/OrchardCore.Tests.Functional/
  Helpers/
    OrchardTestServer.cs         # Builds and runs OrchardCore in-process
    OrchardTestFixture.cs        # Manages Playwright browser + server lifecycle
    EmbeddedRecipeHarvester.cs   # Serves test recipes from embedded resources
    AuthHelper.cs                # IPage.LoginAsync() extension
    TenantHelper.cs              # IPage.SiteSetupAsync(), CreateTenantAsync() extensions
    ButtonHelper.cs              # IPage.ClickSaveAsync() etc. extensions
    ConfigurationHelper.cs       # IPage.SetPageSizeAsync() extension
    FeatureHelper.cs             # IPage.EnableFeatureAsync() / DisableFeatureAsync()
    SelectorHelper.cs            # IPage.GetByCy() / FindByCy() for data-cy attributes
    TestUtils.cs                 # OrchardConfig, TenantInfo, GenerateTenantInfo()
  Tests/
    Cms/
      CmsRecipeFixture.cs        # Base fixture: starts app, runs setup with a recipe
      CmsTestBase.cs             # Base test class with log assertion in teardown
      AgencyTests.cs             # Tests for the Agency recipe
      BlogTests.cs               # Tests for the Blog recipe
      ComingSoonTests.cs         # Tests for the ComingSoon recipe
      HeadlessTests.cs           # Tests for the Headless recipe
      MigrationsTests.cs         # Tests for database migrations via custom recipe
      SaasFixture.cs             # Fixture for SaaS multi-tenancy tests
      SaasTests.cs               # Tests tenant creation and isolation
    Mvc/
      MvcSetupFixture.cs         # Fixture for the MVC HelloWorld app
      MvcTests.cs                # Smoke test for MVC module
  Fixtures/
    migrations.recipe.json       # Embedded recipe for testing database migrations
```

### Adding test recipes

Place any `.recipe.json` file in the `Fixtures/` folder. The csproj embeds `Fixtures/**/*.json` as assembly resources, and `EmbeddedRecipeHarvester` automatically discovers and serves them — no manual registration needed.

### Fixture hierarchy

Tests follow a layered fixture pattern:

1. **`OrchardTestServer`** - Builds and starts the host, manages database isolation, captures logs via `FakeLoggerProvider`.
2. **`OrchardTestFixture`** - Wraps `OrchardTestServer` with Playwright browser/context lifecycle, tracing, and app data cleanup.
3. **`CmsRecipeFixture`** - Abstract base that runs site setup with a specific recipe during initialization. Concrete fixtures: `AgencyFixture`, `BlogFixture`, `ComingSoonFixture`, `HeadlessFixture`, `MigrationsFixture`.
4. **`CmsTestBase<TFixture>`** - Generic base test class that calls `AssertNoLoggedIssues()` in teardown.

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

A `FakeLoggerProvider` (from `Microsoft.Extensions.Diagnostics.Testing`) captures all Warning+ log entries during the test. After each test, `AssertNoLoggedIssues()` fails if any Warning or higher message was logged, catching server-side issues that may be invisible in the browser. A specific known benign DataProtection warning is filtered out.

## Environment variables

| Variable | Description | Default |
|----------|-------------|---------|
| `PLAYWRIGHT_TRACING` | Enable Playwright tracing (set to any value) | Disabled |
| `ORCHARD_EXTERNAL` | Skip in-process hosting, use an externally running app | Not set |
| `ORCHARD_URL` | Base URL when using `ORCHARD_EXTERNAL` | `http://localhost:5000` |
| `OrchardCore__DatabaseProvider` | Database provider (`Postgres`, `MySql`, `SqlConnection`) | SQLite |
| `OrchardCore__ConnectionString` | Connection string for the database provider | Not set |

## Testing with external databases

Helper scripts spin up a Docker container and run the CMS tests with the same configuration as CI.

**Linux / macOS:**

```bash
cd test/OrchardCore.Tests.Functional

./run-db-tests.sh postgres   # PostgreSQL
./run-db-tests.sh mysql      # MySQL
./run-db-tests.sh mssql      # SQL Server
./run-db-tests.sh sqlite     # SQLite (no container)
./run-db-tests.sh all        # All databases sequentially
./run-db-tests.sh cleanup    # Remove test containers
```

**Windows (PowerShell):**

```powershell
cd test/OrchardCore.Tests.Functional

.\run-db-tests.ps1 postgres   # PostgreSQL
.\run-db-tests.ps1 mysql      # MySQL
.\run-db-tests.ps1 mssql      # SQL Server
.\run-db-tests.ps1 sqlite     # SQLite (no container)
.\run-db-tests.ps1 all        # All databases sequentially
.\run-db-tests.ps1 cleanup    # Remove test containers
```

`run-db-tests.ps1` can also be used on Linux/macOS via [PowerShell](https://github.com/PowerShell/PowerShell). If `pwsh` is not available, install it from the [PowerShell GitHub releases page](https://github.com/PowerShell/PowerShell/releases/latest).

Requires Docker.

## Playwright tracing

When `PLAYWRIGHT_TRACING` is set, traces are saved to `test/OrchardCore.Tests.Functional/traces/`. View them at <https://trace.playwright.dev>.

## CI

The `functional_all_db.yml` workflow runs these tests against SQLite, PostgreSQL, MySQL, and SQL Server. Tracing is enabled by default in CI, and trace files are uploaded as artifacts on failure.

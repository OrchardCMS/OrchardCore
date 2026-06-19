# Testing reference

## Project layout

```
test/
├── OrchardCore.Tests/               # unit + in-process integration (main)
│   ├── Apis/Context/                # SiteContext + test harness
│   ├── Apis/ContentManagement/      # content API tests
│   ├── Apis/GraphQL/                # GraphQL tests
│   ├── Modules/<Module>/            # per-module tests
│   ├── DisplayManagement/, Localization/, ...
│   └── xunit.runner.json
├── OrchardCore.Abstractions.Tests/  # pure unit tests
├── OrchardCore.Tests.Integration/   # external services (S3, ...)
├── OrchardCore.Tests.Functional/    # Playwright
└── OrchardCore.Benchmarks/          # BenchmarkDotNet
```

Framework: **xUnit v3** via the `xunit.v3.mtp-v2` package (versions in `Directory.Packages.props`). Projects set `<OutputType>Exe</OutputType>` for the Microsoft Testing Platform runner. `xunit.runner.json` here sets `"shadowCopy": false`.

## Naming conventions

- Class: `{Subject}Tests` — `Base64Tests`, `BlogPostApiControllerTests`.
- Method: `{Action}Should{Expected}` or `{Action}_{Condition}_{Expected}` — `ShouldCreateDraftOfExistingContentItem`, `InvokeAsync_InitializedShell_SkipsSetup`.
- Arrange–Act–Assert.

## Parameterized tests

```csharp
public static IEnumerable<object[]> MergeArrayEntries =>
[
    ["[1,2]", "[3]", null, "[1,2,3]"],
];

[Theory]
[MemberData(nameof(MergeArrayEntries))]
public void Merge(string a, string b, JsonMergeSettings s, string expected) { ... }
```

## SiteContext (integration harness)

`test/OrchardCore.Tests/Apis/Context/SiteContext.cs`. Key surface:

```csharp
public class SiteContext : IDisposable
{
    public static IShellHost ShellHost { get; }
    public static HttpClient DefaultTenantClient { get; }

    public string RecipeName { get; set; } = "Blog";
    public string DatabaseProvider { get; set; } = "Sqlite";
    public string ConnectionString { get; set; }
    public HttpClient Client { get; private set; }
    public OrchardGraphQLClient GraphQLClient { get; private set; }

    public virtual async Task InitializeAsync();
    public async Task UsingTenantScopeAsync(Func<ShellScope, Task> execute, bool activateShell = true);
}
```

`InitializeAsync` creates a tenant with a GUID name + generated table prefix by calling `api/tenants/create` on the shared `DefaultTenantClient`, runs the recipe, and exposes a tenant-scoped `Client`.

`UsingTenantScopeAsync` gets a `ShellScope` for the tenant, sets `HttpContextAccessor.HttpContext`, runs your delegate, then clears it — the way to touch `ISession`, `IContentManager`, etc.

### Customizing

```csharp
public class AgencyContext : SiteContext
{
    public AgencyContext() => this.WithRecipe("Agency");
}
```

Or set `RecipeName`/`DatabaseProvider`/`ConnectionString` before `InitializeAsync`. Provider-specific contexts (e.g. `LuceneContext`) subclass `SiteContext`.

## Mocking (Moq)

```csharp
// property stub
var clock = Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow);

// full mock
var host = new Mock<IShellHost>();
host.Setup(h => h.GetSettings("Default")).Returns(settings);
host.Verify(h => h.GetSettings(It.IsAny<string>()), Times.Once);

// inject built services
var sp = new ServiceCollection().AddSingleton(host.Object).BuildServiceProvider();
```

Fakes are often `SiteContext` subclasses rather than hand-written doubles.

## Functional tests (Playwright)

`test/OrchardCore.Tests.Functional/Helpers/OrchardTestFixture.cs`:

```csharp
public sealed class OrchardTestFixture : IAsyncDisposable
{
    public string BaseUrl { get; private set; }
    public IBrowser Browser { get; }

    public async Task InitializeAsync()
    {
        _server = await OrchardTestServer.StartCmsAsync(AppDir, AppDataPath, _instanceId);
        BaseUrl = _server.ServerAddress;
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
    }

    public async Task<IPage> CreatePageAsync(string traceName = null) { ... }
}
```

- Cleans `App_Data` between runs; starts an in-process CMS server.
- Tracing on `PLAYWRIGHT_TRACING` env var (screenshots/snapshots/sources → `traces/`).
- Package: `Microsoft.Playwright`.

## Running

```bash
dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj
dotnet test <project> --filter "FullyQualifiedName~Namespace.Class"
```

All tests must pass for CI. Per the contributing guide: run `yarn build` when changing CSS/JS, and guard refactors with new tests.

## Docs

- `src/docs/contributing/contributing-code.md` — testing expectations (no standalone test guide); follows ASP.NET Core engineering guidelines.
- `src/docs/getting-started/test-drive-orchard-core.md` — user-facing, not for writing tests.

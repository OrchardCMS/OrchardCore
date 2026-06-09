---
name: orchardcore-unit-test
description: Writes and runs OrchardCore tests — xUnit unit tests, SiteContext-based integration tests, Moq mocking, and Playwright functional tests. Use when the user needs to add a test, set up a test harness/tenant for tests, mock OrchardCore services, or run the test suite.
---

# OrchardCore Unit & Integration Testing

This skill guides you through writing and running OrchardCore tests following project conventions.

OrchardCore uses **xUnit v3** (Microsoft Testing Platform — test projects are `Exe`). Tests live under `test/`:

| Project | Kind |
|---------|------|
| `OrchardCore.Tests` | unit + in-process integration (main) |
| `OrchardCore.Abstractions.Tests` | pure unit tests for core abstractions |
| `OrchardCore.Tests.Integration` | external-service integration (S3, etc.) |
| `OrchardCore.Tests.Functional` | Playwright browser automation |
| `OrchardCore.Benchmarks` | BenchmarkDotNet (not xUnit) |

## Decide the test type

| Testing… | Type | Harness |
|----------|------|---------|
| Pure logic, a single class | unit | plain xUnit + Moq |
| A driver/service needing DI | unit | build a small `ServiceCollection` |
| Content APIs, recipes, tenant behavior | integration | `SiteContext` |
| Admin/front-end through a browser | functional | `OrchardTestFixture` + Playwright |

## Workflow A: unit test

### Step 1: Add a test class

Naming: `{Subject}Tests`; methods `{Action}Should{Result}` or `{Action}_{Condition}_{Result}`.

```csharp
namespace OrchardCore.Json.Nodes.Test;

public class Base64Tests
{
    [Theory]
    [InlineData("YTw+OmE/", "a<>:a?")]
    [InlineData("SGVsbA==", "Hell")]
    public void DecodeToStringTest(string source, string expected)
    {
        Assert.Equal(expected, Base64.FromUTF8Base64String(source));
    }
}
```

Use `[Fact]` for single cases, `[Theory]` + `[InlineData]`/`[MemberData]` for parameterized. Assertions are xUnit `Assert.*` (no Shouldly in this repo).

### Step 2: Mock dependencies with Moq

```csharp
using Moq;

// Quick stub:
var clock = Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow);

// With setup/verify:
var shellHost = new Mock<IShellHost>();
shellHost.Setup(h => h.GetScopeAsync(It.IsAny<string>())).ReturnsAsync(scope);
// ...
shellHost.Verify(h => h.GetScopeAsync("Default"), Times.Once);
```

Build a service provider when the unit needs DI:

```csharp
var httpContext = new DefaultHttpContext
{
    RequestServices = new ServiceCollection()
        .AddSingleton(myService.Object)
        .BuildServiceProvider(),
};
```

## Workflow B: integration test with SiteContext

`SiteContext` spins up a real tenant (SQLite by default) from a recipe and gives you an `HttpClient` + tenant scope.

```csharp
public class BlogPostApiControllerTests
{
    [Fact]
    public async Task ShouldCreateDraftOfExistingContentItem()
    {
        using var context = new SiteContext();
        await context.InitializeAsync();

        var response = await context.Client.PostAsJsonAsync("api/content?draft=true", contentItem);
        var draft = await response.Content.ReadAsAsync<ContentItem>();

        Assert.True(draft.Latest);
        Assert.False(draft.Published);
    }
}
```

Resolve tenant services inside a scope:

```csharp
await context.UsingTenantScopeAsync(async scope =>
{
    var session = scope.ServiceProvider.GetRequiredService<ISession>();
    var posts = await session.Query<ContentItem, ContentItemIndex>(x => x.ContentType == "BlogPost").ListAsync();
    Assert.Equal(2, posts.Count());
});
```

Customize the recipe by subclassing or `WithRecipe`:

```csharp
public class AgencyContext : SiteContext
{
    public AgencyContext() => this.WithRecipe("Agency");
}
```

Defaults: `RecipeName = "Blog"`, `DatabaseProvider = "Sqlite"`, a random tenant name + table prefix per test.

## Workflow C: functional test (Playwright)

`OrchardTestFixture` starts a CMS server and a headless Chromium browser.

```csharp
var page = await fixture.CreatePageAsync();
await page.GotoAsync("/");
await Expect(page.Locator("h1")).ToBeVisibleAsync();
```

Set `PLAYWRIGHT_TRACING` to capture screenshots/snapshots/sources into `traces/`.

## Running tests

```bash
# All tests in a project (from repo root)
dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj

# Filter by name (xUnit / MTP)
dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj --filter "FullyQualifiedName~BlogPost"
```

CI requires all tests green. If you change CSS/JS, run `yarn build` first (asset tests).

## Quick Reference

### xUnit attributes

| Attribute | Use |
|-----------|-----|
| `[Fact]` | one test case |
| `[Theory]` + `[InlineData]` | inline parameter sets |
| `[Theory]` + `[MemberData(nameof(X))]` | computed parameter sets |

### Common assertions

`Assert.Equal`, `Assert.True/False`, `Assert.Null/NotNull`, `Assert.Contains`, `Assert.Throws<T>`, `await Assert.ThrowsAsync<T>(...)`.

### SiteContext members

| Member | Purpose |
|--------|---------|
| `InitializeAsync()` | create + set up the tenant |
| `Client` | `HttpClient` bound to the tenant |
| `UsingTenantScopeAsync(fn)` | run code in the tenant's DI scope |
| `GraphQLClient` | GraphQL API client |
| `RecipeName` / `DatabaseProvider` | override before `InitializeAsync` |

### Moq cheatsheet

| Need | Code |
|------|------|
| Stub a property | `Mock.Of<I>(x => x.P == v)` |
| Setup a method | `m.Setup(x => x.F(It.IsAny<T>())).ReturnsAsync(r)` |
| Verify a call | `m.Verify(x => x.F(arg), Times.Once)` |
| Pass the object | `m.Object` |

## Gotchas

- Test projects are `Exe` (MTP) — keep that `OutputType` when adding one; don't switch to library.
- `SiteContext` is `IDisposable` — always `using var context = ...`.
- Resolve tenant services only inside `UsingTenantScopeAsync`; the outer scope isn't the tenant.
- Integration tests use SQLite + a fresh per-test table prefix; tests must not assume shared state.
- Guard refactors with tests — the contributing guide requires new tests for refactoring.

## References

- `references/testing.md` — SiteContext internals, fixtures, Playwright, project layout
- `src/docs/contributing/contributing-code.md` (repo) — test expectations
- `test/OrchardCore.Tests/` (repo) — real examples
- `AGENTS.md` (repo root) — build commands

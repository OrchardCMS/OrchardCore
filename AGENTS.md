# Orchard Core - Agent Guidelines

This document provides instructions for LLM agents working with the Orchard Core codebase. It covers building, testing, and creating new features.

## Project Overview

Orchard Core is an open-source, modular, multi-tenant application framework and CMS for ASP.NET Core. It consists of:

- **Orchard Core Framework**: An application framework for building modular, multi-tenant applications
- **Orchard Core CMS**: A Web Content Management System built on top of the framework

**Repository**: <https://github.com/OrchardCMS/OrchardCore>  
**Documentation**: <https://docs.orchardcore.net/>

## Prerequisites

- **.NET SDK**: Version 10.0+ (see `global.json` for exact version requirements)
- **Node.js**: Version 22.x (for asset compilation)
- **Yarn**: Version 4.x (package manager for frontend assets)

## Building the Project

### Command Line

```bash
# Navigate to the web project
cd src/OrchardCore.Cms.Web

# Run the application
dotnet run

# Or build with specific configuration
dotnet build -c Debug -f net10.0
```

### Full Solution Build

```bash
# From repository root
dotnet build OrchardCore.sln

# Build with Release configuration
dotnet build OrchardCore.sln -c Release
```

### Target Framework

The default target framework is `net10.0` as defined in `src/OrchardCore.Build/TargetFrameworks.props`.

## Running the Application

```bash
cd src/OrchardCore.Cms.Web
dotnet run -f net10.0
```

The application will be available at `http://localhost:5000` (and `https://localhost:5001`).

## Running Tests

### Unit Tests

Unit tests are located in the `test/` directory and use xUnit v3.

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test test/OrchardCore.Tests/OrchardCore.Tests.csproj

# Run tests with a filter
dotnet test --filter-method "*.YourTest"
```

Arguments for tests:

```bash
--filter-class
```

Run all methods in a given test class. Pass one or more fully qualified type names (i.e.,
'MyNamespace.MyClass' or 'MyNamespace.MyClass+InnerClass'). Wildcard '*' is supported at
the beginning and/or end of each filter.
    Note: Specifying more than one is an OR operation.
        This is categorized as a simple filter. You cannot use both simple filters and query filters.

```bash        
--filter-method
```

Run a given test method. Pass one or more fully qualified method names (i.e.,
'MyNamespace.MyClass.MyTestMethod'). Wildcard '*' is supported at the beginning and/or end
of each filter.
    Note: Specifying more than one is an OR operation.
        This is categorized as a simple filter. You cannot use both simple filters and query filters.


### Functional Tests (Cypress)

End-to-end tests are located in `test/OrchardCore.Tests.Functional/`.

```bash
cd test/OrchardCore.Tests.Functional
npm install
npm run cms:test
```

### Automated Browser Testing (Playwright MCP)

For AI agents, the Playwright MCP (Model Context Protocol) provides automated browser testing capabilities:

- **Setup**: Ensure the application is running at `http://localhost:5000`
- **Navigation**: Use `mcp_playwright_browser_navigate` to navigate to pages
- **Interactions**: Use tools like `mcp_playwright_browser_click`, `mcp_playwright_browser_type` for user actions
- **Verification**: Use `mcp_playwright_browser_snapshot` to capture page state
- **Console**: Use `mcp_playwright_browser_console_messages` to check for JavaScript errors

**Example workflow:**
1. Navigate to the application
2. Complete setup wizard if needed
3. Navigate to specific features (e.g., `/Admin/Media`)
4. Interact with UI elements
5. Verify console has no errors
6. Capture screenshots or snapshots for validation

### Test Organization

- `test/OrchardCore.Tests/` - Main unit test project
- `test/OrchardCore.Abstractions.Tests/` - Tests for abstractions
- `test/OrchardCore.Tests.Functional/` - Cypress E2E tests
- `test/OrchardCore.Tests.Modules/` - Test modules used by tests

## Project Structure

```
OrchardCore/
├── src/
│   ├── OrchardCore/                    # Core framework libraries
│   │   ├── OrchardCore/                # Main framework
│   │   ├── OrchardCore.Abstractions/   # Core interfaces
│   │   └── ...                         # Other abstractions
│   ├── OrchardCore.Modules/            # Built-in modules
│   ├── OrchardCore.Themes/             # Built-in themes
│   ├── OrchardCore.Cms.Web/            # Main CMS web application
│   └── docs/                           # Documentation source
├── test/                               # Test projects
└── .scripts/                           # Build and asset scripts
```

## Available Skills

The following skills are available in `.skills/` for guided workflows:

| Skill | Description | Use When |
|-------|-------------|----------|
| `orchardcore-module-creator` | Create new modules | Adding modules, content parts, fields, handlers |
| `orchardcore-theme-creator` | Create new themes | Adding themes, layouts, frontend assets |
| `orchardcore-tester` | Browser-based testing | Testing features via Playwright automation |

These skills provide step-by-step guidance, code templates, and references for common tasks.

## Frontend Assets

### Asset Management

Orchard Core uses an asset manager for compiling SCSS, TypeScript, and JavaScript.

```bash
# Install Yarn

# Install dependencies (from repository root)
corepack enable
yarn

# Build all assets including gulp
yarn build
```

### Assets.json Configuration

Each module with frontend assets needs an `Assets.json` file:

```json
[
  {
    "action": "vite",
    "name": "your-module",
    "source": "Assets/",
    "tags": ["js", "css"]
  }
]
```

### Asset Dependencies (package.json)

```json
{
  "name": "@orchardcore/your-module",
  "version": "1.0.0",
  "dependencies": {
    "vue": "3.5.13",
    "bootstrap": "5.3.8"
  }
}
```

## Content Management Patterns

For detailed patterns including Content Parts, Content Part Drivers, Content Fields, and more, see the `orchardcore-module-creator` skill in `.skills/`.

## Coding Conventions

### General Guidelines

- Follow [ASP.NET Core Engineering guidelines](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines)
- Use `sealed` for classes that should not be inherited
- Use file-scoped namespaces
- Prefer collection expressions (`[]`) over `new List<T>()`
- Avoid use of primary constructors

### Naming Conventions

- Classes: `PascalCase`
- Interfaces: `IPascalCase`
- Methods: `PascalCase`
- Properties: `PascalCase`
- Private fields: `_camelCase`
- Local variables: `camelCase`
- Constants: `PascalCase`

### Async Conventions

- Suffix async methods with `Async`
- Use `Task` or `ValueTask` return types

### Code Analysis

The project uses:
- StyleCop.Analyzers for style enforcement
- `AnalysisLevel` set to `latest-Recommended`
- Specific CA rules are suppressed (see `Directory.Build.props`)

## Database Patterns

### YesSql Usage

Orchard Core uses YesSql as its document database abstraction:

```csharp
public class YourService
{
    private readonly ISession _session;

    public YourService(ISession session)
    {
        _session = session;
    }

    public async Task<YourDocument> GetAsync(string id)
    {
        return await _session.Query<YourDocument, YourIndex>()
            .Where(x => x.DocumentId == id)
            .FirstOrDefaultAsync();
    }

    public async Task SaveAsync(YourDocument document)
    {
        await _session.SaveAsync(document);
    }
}
```

### Index Definition

```csharp
public class YourIndex : MapIndex
{
    public string DocumentId { get; set; }
    public string Name { get; set; }
}

public class YourIndexProvider : IndexProvider<YourDocument>
{
    public override void Describe(DescribeContext<YourDocument> context)
    {
        context.For<YourIndex>()
            .Map(doc => new YourIndex
            {
                DocumentId = doc.Id,
                Name = doc.Name,
            });
    }
}
```

## Testing Patterns

### Unit Test Structure

```csharp
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.YourModule;

public class YourServiceTests
{
    [Fact]
    public async Task YourMethod_ShouldDoSomething_WhenCondition()
    {
        // Arrange
        var service = new YourService();

        // Act
        var result = await service.YourMethodAsync();

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("input1", "expected1")]
    [InlineData("input2", "expected2")]
    public void YourMethod_ShouldReturnExpected(string input, string expected)
    {
        // Test implementation
    }
}
```

### Integration Test with Host

```csharp
public class YourIntegrationTests : IClassFixture<OrchardTestFixture>
{
    private readonly OrchardTestFixture _fixture;

    public YourIntegrationTests(OrchardTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Feature_ShouldWork()
    {
        // Use _fixture to create test scenarios
    }
}
```

### Manual Testing

For browser-based manual testing using Playwright, see the `orchardcore-tester` skill in `.skills/`.

**Quick start:**
```powershell
# Build
dotnet build src/OrchardCore.Cms.Web -c Debug -f net10.0

# Generate/get port and start in background
$port = if (Test-Path .orchardcore-port) { Get-Content .orchardcore-port } else { $p = Get-Random -Min 5000 -Max 6000; $p | Out-File .orchardcore-port -NoNewline; $p }
$proc = Start-Process dotnet -ArgumentList "run","-f","net10.0","--no-build","--urls","http://localhost:$port" -WorkingDirectory "src/OrchardCore.Cms.Web" -PassThru -NoNewWindow
$proc.Id | Out-File .orchardcore-pid -NoNewline

# URL: http://localhost:$port
# Test credentials: admin / admin@test.com / Password1!

# Stop when done
Stop-Process -Id (Get-Content .orchardcore-pid) -Force; Remove-Item .orchardcore-pid

# Reset state: Remove-Item -Recurse -Force src/OrchardCore.Cms.Web/App_Data
```

**Debugging**: Check `src/OrchardCore.Cms.Web/App_Data/logs/orchard-log-{date}.log`

### Functional Testing with Cypress

Create new functional tests under `test/OrchardCore.Tests.Functional/cypress/` following the existing spec patterns.

Run the Cypress functional tests:

```bash
cd test/OrchardCore.Tests.Functional

# Required before first usage
npm install

# Run all functional tests
npm run cms:test
```

## Common Extension Points

### Registering Services

```csharp
// In Startup.cs
services.AddScoped<IYourService, YourService>();
services.AddSingleton<IYourSingleton, YourSingleton>();
services.AddTransient<IYourTransient, YourTransient>();
```

### Event Handlers

```csharp
public class YourContentHandler : ContentHandlerBase
{
    public override Task PublishedAsync(PublishContentContext context)
    {
        // Handle content published event
        return Task.CompletedTask;
    }
}
```

### Background Tasks

```csharp
public class YourBackgroundTask : IBackgroundTask
{
    public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Background work implementation
        return Task.CompletedTask;
    }
}
```

### Navigation/Admin Menu

```csharp
public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Your Menu"], menu => menu
                .Add(S["Your Item"], S["Your Item"], item => item
                    .Action("Index", "Admin", "OrchardCore.YourModule")
                    .Permission(YourPermissions.ManageYourFeature)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
```

## Debugging Tips

1. **Enable detailed errors** in development by setting `ASPNETCORE_ENVIRONMENT=Development`
2. **Check tenant logs** in `App_Data/Sites/{TenantName}/logs/`
3. **Use MiniProfiler** module for performance analysis
4. **Enable SQL logging** by configuring YesSql logging

## Pull Request Guidelines

1. Follow existing code style and conventions
2. Include unit tests for new functionality
3. Update documentation if adding new features
4. Run asset build if modifying CSS/JS: `yarn build`
5. Ensure all tests pass: `dotnet test`
6. Link related GitHub issues using `Fixes #IssueId`
7. Add release notes for significant changes in `src/docs/releases/`

## Useful Commands

```bash
# Restore packages
dotnet restore

# Clean build artifacts
dotnet clean

# Run
dotnet run --project src/OrchardCore.Cms.Web

# Build assets
yarn build

# Lint JavaScript/TypeScript
yarn lint

# Type check Vue/TypeScript
yarn check
```

## Resources

- [Documentation](https://docs.orchardcore.net/)
- [Contributing Guide](https://docs.orchardcore.net/en/latest/contributing/)
- [Discord Community](https://orchardcore.net/discord)
- [Issue Tracker](https://github.com/OrchardCMS/OrchardCore/issues)
- [API Reference](https://docs.orchardcore.net/en/latest/reference/)

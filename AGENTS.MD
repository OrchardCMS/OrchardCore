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

## Creating a New Module

### Module Structure

A module typically contains:

```
OrchardCore.Modules/OrchardCore.YourModule/
├── Manifest.cs                 # Module metadata and features
├── Startup.cs                  # Service registration
├── Migrations.cs               # Database migrations
├── PermissionProvider.cs       # Permission definitions
├── AdminMenu.cs                # Admin navigation
├── Controllers/                # MVC controllers
├── Drivers/                    # Content part/field drivers
├── Fields/                     # Custom content fields
├── Handlers/                   # Event handlers
├── Models/                     # Data models
├── Services/                   # Business logic services
├── Settings/                   # Part/field settings
├── ViewModels/                 # View models
├── Views/                      # Razor views
├── Assets/                     # Frontend assets (JS, CSS, SCSS)
│   ├── js/
│   ├── scss/
│   └── package.json
├── wwwroot/                    # Static files output
└── OrchardCore.YourModule.csproj
```

### Manifest.cs

Define module metadata and features:

```csharp
using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Your Module",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.YourModule",
    Name = "Your Module",
    Description = "Description of your module.",
    Dependencies = ["OrchardCore.ContentTypes"],
    Category = "Content Management"
)]
```

### Startup.cs

Register services using dependency injection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.YourModule;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IYourService, YourService>();
        services.AddDataMigration<Migrations>();
        // Register navigation, permissions, etc.
    }
}
```

### Migrations.cs

Database migrations using YesSql:

```csharp
using OrchardCore.Data.Migration;

namespace OrchardCore.YourModule;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        // Initial migration logic
        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        // Migration from version 1 to 2
        return 2;
    }
}
```

### PermissionProvider.cs

Define and register permissions:

```csharp
using OrchardCore.Security.Permissions;

namespace OrchardCore.YourModule;

public sealed class PermissionProvider : IPermissionProvider
{
    public static readonly Permission ManageYourFeature = 
        new("ManageYourFeature", "Manage your feature");

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
        => Task.FromResult<IEnumerable<Permission>>([ManageYourFeature]);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = [ManageYourFeature],
        },
    ];
}
```

### Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <Title>OrchardCore Your Module</Title>
    <Description>Your module description.</Description>
    <PackageTags>$(PackageTags) OrchardCoreCMS</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />
    <!-- Add other dependencies -->
  </ItemGroup>
</Project>
```

## Creating a New Theme

### Theme Structure

```
OrchardCore.Themes/YourTheme/
├── Manifest.cs
├── Views/
│   └── Layout.cshtml
├── Assets/
│   └── scss/
├── wwwroot/
│   ├── css/
│   └── js/
└── YourTheme.csproj
```

### Theme Manifest

```csharp
using OrchardCore.DisplayManagement.Manifest;

[assembly: Theme(
    Name = "Your Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Your theme description."
)]
```

## Frontend Assets

### Asset Management

Orchard Core uses an asset manager for compiling SCSS, TypeScript, and JavaScript.

```bash
# Install Yarn

# Install dependencies (from repository root)
corepack enable
yarn

# Build all assets including gulp
yarn build -gr
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

### Content Part

```csharp
public class YourPart : ContentPart
{
    public string YourField { get; set; }
}
```

### Content Part Driver

```csharp
public sealed class YourPartDisplayDriver : ContentPartDisplayDriver<YourPart>
{
    public override IDisplayResult Display(YourPart part, BuildPartDisplayContext context)
    {
        return Initialize<YourPartViewModel>("YourPart", model =>
        {
            model.YourField = part.YourField;
        }).Location("Detail", "Content:5");
    }

    public override IDisplayResult Edit(YourPart part, BuildPartEditorContext context)
    {
        return Initialize<YourPartViewModel>("YourPart_Edit", model =>
        {
            model.YourField = part.YourField;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(
        YourPart part, 
        UpdatePartEditorContext context)
    {
        var viewModel = new YourPartViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);
        part.YourField = viewModel.YourField;
        return Edit(part, context);
    }
}
```

### Content Field

```csharp
public class YourField : ContentField
{
    public string Value { get; set; }
}
```

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

Use the Playwright MCP to do manual testing that involve browsing the web application.

#### Clearing Application State

To start with a fresh installation:

```bash
# Stop the application if running (Ctrl+C)

# Delete the App_Data folder (contains database and tenant data)
rm -rf src/OrchardCore.Cms.Web/App_Data

# The application will prompt for setup on next run
```

The application is ready when the message `Application started.` is writing on the console.

#### Starting the Application

```bash
# Navigate to the web project
cd src/OrchardCore.Cms.Web

# Run
dotnet run -f net10.0

# Or Run without building if there were no binary code changes
dotnet run -f net10.0 --no-build

# Application will be available at http://localhost:5000
```

#### Setting Up a Test Site

1. Navigate to `http://localhost:5000` in your browser
2. Complete the setup wizard:
   - **Site Name**: Enter any name (e.g., "Test Site")
   - **Recipe**: Select **"Blog"** instead of the default "Software as a Service"
     - Blog recipe includes common features like Media, Content Management, and Admin enabled
     - Provides a better starting point for testing various modules
   - **Database**: Keep default "Sqlite" for local testing
   - **Super User**: Create admin credentials
     - Username: `admin`
     - Email: `admin@test.com`
     - Password: `Password1!`
3. Click **"Finish Setup"**
4. Log in with your admin credentials

#### Enabling Specific Features

After setup, you can enable additional features:

1. Navigate to **Admin** → **Configuration** → **Features** (`/Admin/Features`)
2. Find the feature you want to enable (e.g., "Media", "Media Library", "Deployment")
3. Click the **"Enable"** button next to the feature
4. Wait for the feature to be enabled (page will refresh)

**Common Features for Testing:**
- **OrchardCore.Media**: Core media management
- **OrchardCore.Contents**: Content item management
- **OrchardCore.ContentTypes**: Content type editor

**Pro Tip**: You can enable multiple features at once by checking their boxes and clicking "Enable" at the bottom.

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
4. Run asset build if modifying CSS/JS: `yarn build -gr`
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
yarn build -gr

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

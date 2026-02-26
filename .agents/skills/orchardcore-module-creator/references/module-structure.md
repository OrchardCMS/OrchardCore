# Module Structure Reference

## Directory Layout

```
src/OrchardCore.Modules/OrchardCore.YourModule/
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

## Required Files

### Manifest.cs

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
    <!-- Add other dependencies as needed -->
  </ItemGroup>
</Project>
```

## Optional Files

| File | Purpose | When Needed |
|------|---------|-------------|
| `Migrations.cs` | Database schema | When storing data |
| `PermissionProvider.cs` | Define permissions | When securing features |
| `AdminMenu.cs` | Admin navigation | When adding admin UI |
| `Controllers/` | HTTP endpoints | When handling requests |
| `Drivers/` | Content display/edit | When creating content parts |
| `Handlers/` | Event handling | When responding to events |
| `Services/` | Business logic | When encapsulating logic |
| `Views/` | Razor templates | When rendering HTML |

## Feature Dependencies

Common dependencies to consider:

| Dependency | Use Case |
|------------|----------|
| `OrchardCore.ContentTypes` | Working with content types |
| `OrchardCore.Contents` | Content item CRUD |
| `OrchardCore.Media` | Media/file handling |
| `OrchardCore.Users` | User management |
| `OrchardCore.Workflows` | Workflow integration |
| `OrchardCore.Liquid` | Liquid template support |

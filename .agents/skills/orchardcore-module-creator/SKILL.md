---
name: orchardcore-module-creator
description: Creates new OrchardCore modules with proper structure, manifest, startup, and patterns. Use when the user needs to create a new module, add content parts, fields, drivers, handlers, or admin functionality.
---

# OrchardCore Module Creator

This skill guides you through creating new OrchardCore modules following project conventions.

## Prerequisites

- OrchardCore repository at `D:\orchardcore`
- .NET SDK 10.0+ installed

## Module Creation Workflow

### Step 1: Determine Module Type

**What kind of module are you creating?**

| Type | Description | Key Components |
|------|-------------|----------------|
| **Content Part** | Adds data/behavior to content items | Part, Driver, Views |
| **Content Field** | Custom field type | Field, Driver, Views |
| **Settings** | Site-wide configuration | SiteSettings, Driver |
| **Admin Feature** | Admin pages/tools | Controller, Views, Menu |
| **API** | REST endpoints | ApiController |
| **Background Task** | Scheduled jobs | IBackgroundTask |

### Step 2: Create Module Directory

```bash
# Create module folder
mkdir src/OrchardCore.Modules/OrchardCore.YourModule
cd src/OrchardCore.Modules/OrchardCore.YourModule
```

### Step 3: Create Required Files

Every module needs these three files:

1. **Manifest.cs** - Module metadata
2. **Startup.cs** - Service registration  
3. **OrchardCore.YourModule.csproj** - Project file

See `references/module-structure.md` for templates.

### Step 4: Add Components Based on Type

**For Content Part modules:**
```
Models/YourPart.cs
ViewModels/YourPartViewModel.cs
Drivers/YourPartDisplayDriver.cs
Views/YourPart.cshtml
Views/YourPart_Edit.cshtml
```

**For Admin modules:**
```
Controllers/AdminController.cs
Views/Admin/Index.cshtml
AdminMenu.cs
PermissionProvider.cs
```

**For Data-storing modules:**
```
Migrations.cs
Indexes/YourIndex.cs
```

See `references/patterns.md` for code templates.

### Step 5: Register in Startup.cs

```csharp
public override void ConfigureServices(IServiceCollection services)
{
    // Content part
    services.AddContentPart<YourPart>()
        .UseDisplayDriver<YourPartDisplayDriver>();
    
    // Services
    services.AddScoped<IYourService, YourService>();
    
    // Migrations (if storing data)
    services.AddDataMigration<Migrations>();
    
    // Permissions (if securing features)
    services.AddPermissionProvider<PermissionProvider>();
    
    // Navigation (if adding admin menu)
    services.AddNavigationProvider<AdminMenu>();
}
```

### Step 6: Build and Test

```bash
# Build the module
cd D:\orchardcore
dotnet build src/OrchardCore.Modules/OrchardCore.YourModule

# Run the application
cd src/OrchardCore.Cms.Web
dotnet run -f net10.0

# Enable the feature in Admin → Features
```

## Quick Reference

### Naming Conventions

| Item | Convention | Example |
|------|------------|---------|
| Module folder | `OrchardCore.ModuleName` | `OrchardCore.Rating` |
| Namespace | `OrchardCore.ModuleName` | `OrchardCore.Rating` |
| Feature ID | `OrchardCore.ModuleName` | `OrchardCore.Rating` |
| Content Part | `NamePart` | `RatingPart` |
| Driver | `NamePartDisplayDriver` | `RatingPartDisplayDriver` |
| View | `PartName.cshtml` | `RatingPart.cshtml` |
| Edit View | `PartName_Edit.cshtml` | `RatingPart_Edit.cshtml` |

### Common Dependencies

Add to `.csproj` as needed:

```xml
<!-- Core module support -->
<ProjectReference Include="..\..\OrchardCore\OrchardCore.Module.Targets\OrchardCore.Module.Targets.csproj" />

<!-- Content management -->
<ProjectReference Include="..\..\OrchardCore\OrchardCore.ContentManagement\OrchardCore.ContentManagement.csproj" />

<!-- Admin UI -->
<ProjectReference Include="..\..\OrchardCore\OrchardCore.Admin\OrchardCore.Admin.csproj" />
```

### Feature Categories

Use in `Manifest.cs`:
- `Content Management`
- `Content`
- `Navigation`
- `Security`
- `Infrastructure`
- `Theming`
- `Developer`

## References

- `references/module-structure.md` - Directory layout and file templates
- `references/patterns.md` - Code patterns (parts, drivers, handlers, etc.)
- `references/examples.md` - Complete module examples
- `AGENTS.md` (repo root) - Coding conventions and build commands

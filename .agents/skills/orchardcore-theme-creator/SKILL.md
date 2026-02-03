---
name: orchardcore-theme-creator
description: Creates new OrchardCore themes with proper structure, manifest, layouts, and assets. Use when the user needs to create a new theme, customize layouts, or set up frontend assets.
---

# OrchardCore Theme Creator

This skill guides you through creating new OrchardCore themes following project conventions.

## Prerequisites

- OrchardCore repository at `D:\orchardcore`
- .NET SDK 10.0+ installed
- Node.js 22.x and Yarn 4.x (for asset compilation)

## Theme Creation Workflow

### Step 1: Create Theme Directory

```bash
mkdir src/OrchardCore.Themes/YourTheme
cd src/OrchardCore.Themes/YourTheme
```

### Step 2: Create Required Files

Every theme needs these files:

1. **Manifest.cs** - Theme metadata
2. **YourTheme.csproj** - Project file
3. **Views/Layout.cshtml** - Main layout
4. **Views/_ViewImports.cshtml** - Razor imports

### Step 3: Create Manifest.cs

```csharp
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "Your Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "Your theme description."
)]
```

**Extending a base theme:**
```csharp
[assembly: Theme(
    Name = "Your Theme",
    BaseTheme = "TheBlogTheme",  // Inherit from base
    // ... other properties
)]
```

### Step 4: Create Project File

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <Title>Your Theme</Title>
    <Description>Your theme description.</Description>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Theme.Targets\OrchardCore.Theme.Targets.csproj" />
  </ItemGroup>
</Project>
```

### Step 5: Create Layout

Create `Views/Layout.cshtml`:

```html
<!DOCTYPE html>
<html lang="@Orchard.CultureName()">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@RenderTitleSegments(Site.SiteName, "before")</title>
    <resources type="StyleSheet" />
</head>
<body>
    <zone Name="Header" />
    <main>
        <zone Name="Messages" />
        <zone Name="Content" />
    </main>
    <zone Name="Footer" />
    <resources type="FootScript" />
</body>
</html>
```

### Step 6: Add Assets (Optional)

Create asset structure:
```
Assets/
├── scss/site.scss
├── js/site.js
└── package.json
Assets.json
```

See `references/assets.md` for configuration details.

### Step 7: Build and Activate

```bash
# Build theme
cd D:\orchardcore
dotnet build src/OrchardCore.Themes/YourTheme

# Build assets (if added)
yarn && yarn build

# Run application
cd src/OrchardCore.Cms.Web
dotnet run -f net10.0

# Activate theme in Admin → Design → Themes
```

## Quick Reference

### Available Zones

| Zone | Purpose |
|------|---------|
| `Header` | Site header |
| `Navigation` | Main menu |
| `Messages` | Alerts/notifications |
| `Content` | Main content |
| `Sidebar` | Side widgets |
| `Footer` | Site footer |

### Base Themes

| Theme | Description |
|-------|-------------|
| `TheBlogTheme` | Blog template |
| `TheAdmin` | Admin UI |
| `TheAgencyTheme` | Business template |

### Naming Conventions

| Item | Convention |
|------|------------|
| Theme folder | `YourThemeName` (PascalCase) |
| Namespace | `YourThemeName` |
| CSS file | `site.css` |
| JS file | `site.js` |

## References

- `references/theme-structure.md` - Directory layout and templates
- `references/assets.md` - SCSS, JS, and asset pipeline
- `AGENTS.md` (repo root) - Build commands

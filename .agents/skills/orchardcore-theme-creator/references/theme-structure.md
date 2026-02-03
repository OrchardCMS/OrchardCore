# Theme Structure Reference

## Directory Layout

```
src/OrchardCore.Themes/YourTheme/
├── Manifest.cs                 # Theme metadata
├── YourTheme.csproj            # Project file
├── Views/
│   ├── Layout.cshtml           # Main layout
│   ├── _ViewImports.cshtml     # Razor imports
│   └── Shared/                 # Shared partial views
├── Assets/
│   ├── scss/
│   │   └── site.scss           # Main stylesheet
│   ├── js/
│   │   └── site.js             # Main JavaScript
│   └── package.json            # Frontend dependencies
├── wwwroot/
│   ├── css/                    # Compiled CSS output
│   ├── js/                     # Compiled JS output
│   └── images/                 # Static images
└── Assets.json                 # Asset build configuration
```

## Required Files

### Manifest.cs

```csharp
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Modules.Manifest;

[assembly: Theme(
    Name = "Your Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "A custom theme for OrchardCore.",
    Tags = ["Bootstrap"]
)]
```

### Theme with Base Theme

```csharp
[assembly: Theme(
    Name = "Your Theme",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion,
    Description = "A custom theme extending TheAdmin.",
    BaseTheme = "TheAdmin",  // Inherit from another theme
    Tags = ["Bootstrap", "Admin"]
)]
```

### Project File (.csproj)

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
    <Title>Your Theme</Title>
    <Description>A custom theme for OrchardCore.</Description>
    <PackageTags>$(PackageTags) OrchardCoreCMS</PackageTags>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\OrchardCore\OrchardCore.Theme.Targets\OrchardCore.Theme.Targets.csproj" />
    <!-- Optional: Reference base theme -->
    <!-- <ProjectReference Include="..\TheAdmin\TheAdmin.csproj" /> -->
  </ItemGroup>
</Project>
```

## Views

### Views/Layout.cshtml

```html
@using OrchardCore.DisplayManagement.Shapes
@using OrchardCore.DisplayManagement.Zones

<!DOCTYPE html>
<html lang="@Orchard.CultureName()">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>@RenderTitleSegments(Site.SiteName, "before")</title>
    
    <resources type="Meta" />
    <link asp-src="~/YourTheme/css/site.css" />
    <resources type="HeadLink" />
    <resources type="HeadScript" />
    <resources type="StyleSheet" />
</head>
<body>
    <zone Name="Header" />
    
    <main class="container">
        <zone Name="Messages" />
        <zone Name="Content" />
    </main>
    
    <zone Name="Footer" />
    
    <resources type="FootScript" />
    <script asp-src="~/YourTheme/js/site.js"></script>
</body>
</html>
```

### Views/_ViewImports.cshtml

```razor
@inherits OrchardCore.DisplayManagement.Razor.RazorPage<TModel>
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@addTagHelper *, OrchardCore.DisplayManagement
@addTagHelper *, OrchardCore.ResourceManagement
@addTagHelper *, OrchardCore.Menu

@using OrchardCore.DisplayManagement
@using OrchardCore.DisplayManagement.Shapes
@using OrchardCore.DisplayManagement.Zones
@using OrchardCore.Mvc.Utilities
@using Microsoft.AspNetCore.Mvc.Localization
```

## Available Zones

Common zones used in OrchardCore themes:

| Zone | Purpose |
|------|---------|
| `Header` | Site header, navigation |
| `Navigation` | Main menu |
| `Messages` | Alerts, notifications |
| `BeforeContent` | Before main content |
| `Content` | Main page content |
| `AfterContent` | After main content |
| `Sidebar` | Side navigation or widgets |
| `Footer` | Site footer |
| `HeadMeta` | Meta tags in head |

## Base Themes

Available base themes to extend:

| Theme | Description |
|-------|-------------|
| `TheBlogTheme` | Simple blog theme |
| `TheAdmin` | Admin dashboard theme |
| `TheAgencyTheme` | Agency/business theme |
| `SafeMode` | Minimal safe mode theme |

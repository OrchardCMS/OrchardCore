# Theme Schemes (`OrchardCore.Themes.Schemes`)

A theme that supports schemes offers several looks that admins manage and users pick from. A scheme holds four colors (a primary, a deep color, an ink and an accent), optional fine tuning (the hue and saturation of the surfaces, a light menu, the color of text on the primary, the page and toolbar colors, the page tint), and the typography and shape of the theme: the body and heading typefaces, the base font size, the density of surfaces and the roundness of corners. Each theme turns a scheme into its own CSS custom properties, so the same scheme record fits any theme.

**TheAdmin** supports schemes. Any other theme, admin or front end, can support them too.

## Enabling the feature

The Theme Schemes feature can't be enabled from **Configuration > Features**: it's enabled by the themes that depend on it (`EnabledByDependencyOnly`). Enabling a theme that supports schemes enables the feature.

## Managing the schemes of a theme

On **Design > Themes**, the card of a theme that supports schemes has a **Schemes** button. It's shown only for those themes, and only to users with the `ApplyTheme` permission. On small screens the button shows only its palette icon.

The button opens the schemes of that theme, at `/Admin/Themes/Schemes/{themeId}` (the theme id is matched without regard to case, so `/Admin/Themes/Schemes/theadmin` opens the schemes of `TheAdmin`). From there, admins:

- add, edit and delete schemes. The editor shows a live preview of the scheme, drawn by the theme;
- pick the default scheme of the theme with the **Set as default** action. The default scheme is marked with a **Default** badge and can't be deleted. Users who haven't picked a scheme of their own see the default one.

The schemes of each theme are kept apart: the schemes of one theme are never listed for another.

The first time the schemes of a theme are read, the theme seeds its own defaults (TheAdmin seeds Blue, its default, Gray, Ocean, Forest, Plum, Sunset, Graphite and Gold). They're ordinary records from then on, and a theme is seeded only once: deleting all of its schemes doesn't bring them back.

Each user can pick another scheme, when the theme offers a picker. TheAdmin shows one in the theme menu of its top bar, under the light and dark modes. The choice is kept per tenant and per theme, in the `{tenant}-{themeId}-scheme` cookie.

## Recipes and deployment

The `ThemeSchemes` deployment step exports the schemes of every theme that supports them, and the `ThemeSchemes` recipe step imports them. Each entry names its theme (`ThemeId`) and its id (`ItemId`); an entry whose `ItemId` matches a scheme of that theme replaces it. The optional `Defaults` object maps a theme id to the id of its default scheme. Entries of a theme that doesn't support schemes, or isn't enabled, are skipped with an error.

```json
{
  "steps": [
    {
      "name": "ThemeSchemes",
      "Schemes": [
        {
          "ThemeId": "TheAdmin",
          "ItemId": "harbor",
          "Name": "Harbor",
          "Primary": "#1f6f8b",
          "Deep": "#123c4c",
          "Ink": "#081a21",
          "Accent": "#5bc0de",
          "FontFamily": "inter",
          "FontScale": 1,
          "Density": 1,
          "RadiusScale": 1
        }
      ],
      "Defaults": {
        "TheAdmin": "harbor"
      }
    }
  ]
}
```

| Property | Values |
|----------|--------|
| `Primary`, `Deep`, `Ink`, `Accent` | Required, `#rrggbb`. |
| `OnPrimary`, `Page`, `Toolbar`, `Tint`, `TintDark` | Optional, `#rrggbb`. |
| `Hue`, `Saturation` | Optional, `0` to `360` and `0` to `1`. |
| `LightMenu` | Optional, `true` or `false`. |
| `FontFamily`, `HeadingFontFamily` | Optional, `lato`, `inter`, `source-sans-3` or `system`. An empty `HeadingFontFamily` uses the body typeface. |
| `FontScale` | Optional, `0.875` to `1.125` (14 px to 18 px base size). |
| `Density` | Optional, `0.75` to `1.25`. |
| `RadiusScale` | Optional, `0` to `1.5`. |
| `Position` | Optional, the place of the scheme in the pickers, lowest first. |

## Supporting schemes in your theme

A theme opts into schemes in four steps.

### 1. Depend on the feature

Add the feature to the dependencies of the theme's manifest. Enabling the theme then enables the feature.

```csharp
[assembly: Theme(
    Name = "My Theme",
    Dependencies =
    [
        "OrchardCore.Themes.Schemes",
    ]
)]
```

### 2. Implement a provider

`IThemeSchemeProvider` names the theme, its default scheme and the attribute of `<html>` that holds the current scheme. It also seeds the theme's schemes and writes the CSS of a scheme. Each theme owns its attribute and its CSS custom properties, and uses the settings of a scheme that it understands. The provider is registered as a singleton, so keep it stateless.

```csharp
using System.Text;
using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.Schemes.Models;

public sealed class MyThemeSchemeProvider : IThemeSchemeProvider
{
    public const string Id = "MyTheme";

    public string ThemeId => Id;

    public string DefaultSchemeId => "light";

    public string SchemeAttributeName => "data-my-scheme";

    public IReadOnlyList<ThemeScheme> CreateDefaults() =>
    [
        new() { Id = "light", Name = "Light", Position = 0, Primary = "#0d6efd", Deep = "#212529", Ink = "#111111", Accent = "#6ea8fe" },
        new() { Id = "forest", Name = "Forest", Position = 1, Primary = "#2f855a", Deep = "#1c4a36", Ink = "#0b2018", Accent = "#68d391" },
    ];

    public void AppendCss(StringBuilder css, string schemeId, ThemeScheme scheme)
    {
        if (!ThemeColor.TryParse(scheme.Primary, out var primary)
            || !ThemeColor.TryParse(scheme.Deep, out var deep))
        {
            return;
        }

        css.Append($"[data-my-scheme=\"{schemeId}\"]{{")
            .Append($"--my-primary:{primary.ToHex()};")
            .Append($"--my-heading:{deep.ToHex()};")
            .Append("}")
            .AppendLine();
    }
}
```

`ThemeColor` parses and writes `#rrggbb` colors, and measures the WCAG contrast between two colors, so a provider can adjust a color that would be too faint for its text. `ThemeSchemeTypography` holds the font stacks and the ranges of the scales.

### 3. Register the provider

```csharp
using OrchardCore.Themes.Schemes;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddThemeSchemes<MyThemeSchemeProvider>();
    }
}
```

The theme now gets its **Schemes** button on **Design > Themes**, and its schemes are seeded the first time they're read.

### 4. Apply the current scheme in the layout

The layout writes the current scheme on `<html>` and links to the cached stylesheet of the theme's schemes, `~/Themes/schemes/{themeId}.css`, versioned by the schemes' document. Both come from `ThemeSchemeService`:

```cshtml
@using OrchardCore.Themes.Schemes.Services
@inject ThemeSchemeService ThemeSchemes

@{
    var schemesUrl = await ThemeSchemes.GetStylesheetUrlAsync(MyThemeSchemeProvider.Id, Url);
}
<!DOCTYPE html>
<html data-my-scheme="@(await ThemeSchemes.GetSchemeAttributeAsync(MyThemeSchemeProvider.Id))">
<head>
    <resources type="Header" />
    @if (schemesUrl is not null)
    {
        <link rel="stylesheet" href="@schemesUrl" />
    }
</head>
```

`GetSchemeAttributeAsync` returns the scheme the user picked, or else the default scheme of the theme, or else the provider's default. The theme's stylesheet reads the custom properties the provider writes, for example `color: var(--my-heading);`.

### Optional: a picker and a preview

To let users pick a scheme, render the `theme-scheme-picker` script with a list of options. Clicking an option stores the choice in the cookie and applies it to the page at once; the empty value follows the theme's default scheme again.

```cshtml
<script asp-name="theme-scheme-picker" at="Foot"></script>

<div role="group" aria-label="@T["Scheme"]"
     data-theme-scheme-picker
     data-scheme-attribute="data-my-scheme"
     data-cookie-name="@ThemeSchemes.CookieName(MyThemeSchemeProvider.Id)"
     data-site-scheme="@(await ThemeSchemes.GetSiteSchemeAsync(MyThemeSchemeProvider.Id))">
    <button type="button" data-theme-scheme-value="">@T["Site default"]</button>
    @foreach (var scheme in await ThemeSchemes.ListAsync(MyThemeSchemeProvider.Id))
    {
        <button type="button" data-theme-scheme-value="@scheme.Id">@scheme.Name</button>
    }
</div>
```

The editor of a scheme renders the `ThemeSchemePreview` shape. Without a template of its own, a theme gets a simple swatch preview. A theme draws its own by adding a `ThemeSchemePreview-{ThemeId}.cshtml` template, such as `ThemeSchemePreview-MyTheme.cshtml`. An element of it that carries the scheme attribute, set to the shape's `SchemeId`, is restyled live as the fields change. The theme's templates only apply while it's the active theme of the page, so the preview of a front-end theme, edited in the admin, falls back to the swatch preview.

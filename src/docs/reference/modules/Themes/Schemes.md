# Theme Schemes (`OrchardCore.Themes.Schemes`)

A theme that supports schemes offers several looks that admins manage and users pick from. A scheme holds four colors (a primary, a deep color, an ink and an accent), optional fine tuning (the hue and saturation of the surfaces, a light menu, the color of text on the primary, the page and toolbar colors, the page tint), and the typography and shape of the theme: the body and heading typefaces, the base font size, the density of surfaces and the roundness of corners. The feature turns a scheme into CSS custom properties under the prefix of each theme, with the contrast WCAG 2.2 AA asks for, so the same scheme record fits any theme, whatever CSS framework it uses, or none.

**TheAdmin** supports schemes. Any other theme, admin or front end, can support them too.

## Enabling the feature

The Theme Schemes feature can't be enabled from **Configuration > Features**: it's enabled by the themes that depend on it (`EnabledByDependencyOnly`). Enabling a theme that supports schemes enables the feature.

## Managing the schemes of a theme

On **Design > Themes**, the card of a theme that supports schemes has a **Schemes** button. It's shown only for those themes, and only to users with the `ApplyTheme` permission. On small screens the button shows only its palette icon.

The button opens the schemes of that theme, at `/Admin/Themes/Schemes/{themeId}` (the theme id is matched without regard to case, so `/Admin/Themes/Schemes/theadmin` opens the schemes of `TheAdmin`). From there, admins:

- add, edit and delete schemes. The editor shows a live preview of the scheme, drawn by the theme;
- pick the default scheme of the theme with the **Set as default** action. The default scheme is marked with a **Default** badge and has no **Delete** action: make another scheme the default first. Users who haven't picked a scheme of their own see the default one.

A theme never breaks for want of a scheme. The `ThemeSchemes` recipe step only adds or replaces schemes, and the store refuses to delete a theme's default. If the default is removed anyway, for example straight from the database, the theme falls back to the default scheme of its options (`DefaultSchemeId`), then to its first scheme. If none is left at all, its stylesheet still writes the rules of the default scheme of its options, which is never stored; a user's choice of a scheme that no longer exists is ignored.

The schemes of each theme are kept apart: the schemes of one theme are never listed for another.

The first time the schemes of a theme are read, the theme seeds its own defaults (TheAdmin seeds Gray, its default, Blue, Ocean, Forest, Plum, Sunset, Graphite and Gold). They're ordinary records from then on, and a theme is seeded only once: deleting all of its schemes doesn't bring them back.

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

Any theme, admin or front end, can support schemes. The feature stores the schemes, derives their colors with the contrast checks, writes and caches the stylesheet, and gives the theme its **Schemes** button. The theme only registers itself and applies the scheme in its layout.

### 1. Register the theme

Add the feature to the dependencies of the theme's manifest, so that enabling the theme enables it:

```csharp
[assembly: Theme(
    Name = "My Theme",
    Dependencies =
    [
        "OrchardCore.Themes.Schemes",
    ]
)]
```

Then register the theme in its startup. One line is enough:

```csharp
using OrchardCore.Themes.Schemes;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddThemeSchemes("MyTheme");
    }
}
```

The theme now has its **Schemes** button on **Design > Themes**. With no options, it gets a neutral blue scheme named **Default**, its tokens are written as `--mytheme-*` and the current scheme is held by the `data-scheme` attribute of `<html>`.

### 2. Apply the scheme in the layout

The layout writes the current scheme on `<html>` and links to the stylesheet of the theme's schemes. Both come from `ThemeSchemeService`:

```cshtml
@using OrchardCore.Themes.Schemes.Services
@inject ThemeSchemeService ThemeSchemes

@{
    var schemesUrl = await ThemeSchemes.GetStylesheetUrlAsync("MyTheme", Url);
}
<!DOCTYPE html>
<html data-scheme="@(await ThemeSchemes.GetSchemeAttributeAsync("MyTheme"))">
<head>
    <resources type="Header" />
    @if (schemesUrl is not null)
    {
        <link rel="stylesheet" href="@schemesUrl" />
    }
</head>
```

`GetSchemeAttributeAsync` returns the scheme the user picked, or else the site's default scheme of the theme, or else the theme's own default. The theme's CSS reads the tokens:

```css
body {
    background: var(--mytheme-page);
    font-family: var(--mytheme-font-family);
}

h1, h2, h3 {
    color: var(--mytheme-heading);
    font-family: var(--mytheme-heading-font-family);
}

a {
    color: var(--mytheme-link);
}

a:hover {
    color: var(--mytheme-link-hover);
}

.button {
    background: var(--mytheme-primary);
    color: var(--mytheme-on-primary);
    border-radius: calc(0.375rem * var(--mytheme-radius-scale));
}

.button:hover {
    background: var(--mytheme-primary-hover);
}
```

Give the tokens fallbacks in the theme's own CSS (e.g. on `:root`) if pages may render without a scheme.

### 3. Options

Pass a delegate to `AddThemeSchemes` to change any option. Calling `AddThemeSchemes` again for the same theme adds to its options.

```csharp
services.AddThemeSchemes("MyTheme", options =>
{
    options.AttributeName = "data-my-scheme";
    options.TokenPrefix = "my";
    options.DefaultSchemeId = "light";
    options.Defaults =
    [
        new() { Id = "light", Name = "Light", Position = 0, Primary = "#0d6efd", Deep = "#212529", Ink = "#111111", Accent = "#6ea8fe" },
        new() { Id = "forest", Name = "Forest", Position = 1, Primary = "#2f855a", Deep = "#1c4a36", Ink = "#0b2018", Accent = "#68d391" },
    ];
});
```

| Option | Default | Purpose |
|--------|---------|---------|
| `ThemeId` | The id passed to `AddThemeSchemes` | The id of the theme's extension. |
| `AttributeName` | `data-scheme` | The attribute of `<html>` that holds the current scheme, which every rule of the stylesheet selects on. |
| `TokenPrefix` | The theme id in lowercase, every character other than a letter or a digit turned into `-` (`mytheme` for `MyTheme`) | The tokens are written as `--{prefix}-primary`, and so on. |
| `DefaultSchemeId` | The id of the first of `Defaults`, or `default` | The scheme used when the site names no default, or names one that was deleted. |
| `Defaults` | Empty: one neutral blue scheme named **Default** is seeded | The schemes seeded the first time the theme's schemes are read. |
| `DarkModeSelector` | `[data-bs-theme="dark"]` (Bootstrap 5.3's color mode attribute) | Put before the scheme's selector for its dark rule: `{DarkModeSelector}[{AttributeName}="{id}"]`, both on the same element by default. Use your own (e.g. `[data-mode="dark"]`), or `null` for a theme with no dark mode: no dark rule is written. |
| `TokenNames` | Empty | Renames generated tokens, from the generic name to the theme's (e.g. `deep` to `navy`). The `-rgb` token of a renamed color follows it. |
| `TokenContributors` | Empty | The contributors added with `AddTokens<T>()` and `UseBootstrap()`. |
| `CssWriter` | `null` | Writes the whole CSS of a scheme instead of the generated tokens. |

The scheme's id is a valid CSS identifier (lowercase letters, digits and dashes), so it is safe in the selectors.

### 4. The generated tokens

Every token is written as `--{prefix}-{name}`. The light rule, on `[{AttributeName}="{id}"]`, holds every token, including those both modes share; the dark rule, on `{DarkModeSelector}[{AttributeName}="{id}"]`, holds those that change in dark mode. Text and the boundaries of controls are raised to 4.5:1 and 3:1 contrast against every surface they sit on (WCAG 2.2 AA).

| Token | Meaning | Light | Dark |
|-------|---------|:-----:|:----:|
| `primary`, `primary-rgb` | Actions and the current item; darkened when needed so `on-primary` text reads on it | Yes | |
| `primary-hover`, `primary-active` | The primary under the pointer and while pressed | Yes | Yes |
| `primary-subtle`, `primary-border-subtle`, `primary-emphasis` | The background, border and text of highlighted items | Yes | Yes |
| `on-primary` | Text and marks on the primary | Yes | |
| `deep`, `deep-rgb` | The deep color, for headings and dark areas | Yes | |
| `ink` | The darkest color | Yes | |
| `accent`, `accent-rgb` | Links and highlights on dark surfaces | Yes | |
| `link`, `link-rgb` | Links | Yes | Yes |
| `link-hover`, `link-hover-rgb` | Links under the pointer | Yes | Yes |
| `focus` | Focus rings | Yes | Yes |
| `page` | The background of the page | Yes | Yes |
| `surface`, `surface-rgb` | Cards, panels and fields | Yes | Yes |
| `surface-muted` | A quieter surface, e.g. the footer of a card | Yes | Yes |
| `secondary-bg` | The background of secondary elements | | Yes |
| `body-color`, `body-color-rgb` | Body text (the light mode keeps the theme's own) | | Yes |
| `heading` | Headings | Yes | Yes |
| `label` | Form labels | Yes | Yes |
| `muted` | Secondary text and hints | Yes | Yes |
| `border` | Borders and dividers | Yes | Yes |
| `control-border` | The border of fields and other controls | Yes | Yes |
| `control-checked-border` | The border of a checked check box, radio button or switch | Yes | Yes |
| `row-hover` | A row of a list or table under the pointer | Yes | Yes |
| `font-family` | The body font stack | Yes | |
| `heading-font-family` | The heading font stack, or `var(--{prefix}-font-family)` | Yes | |
| `font-scale` | The scale of the base font size, `0.875` to `1.125` | Yes | |
| `density` | The scale of the spacing of controls and lists, `0.75` to `1.25` | Yes | |
| `radius-scale` | The scale of the corner radii, `0` to `1.5` | Yes | |

The `-rgb` tokens hold `r, g, b`, for `rgba(var(--mytheme-primary-rgb), 0.5)`.

### 5. Optional: more tokens

#### Bootstrap's variables

A Bootstrap 5.3 theme can have the feature write Bootstrap's root variables from the tokens:

```csharp
services.AddThemeSchemes("MyTheme", options => options.UseBootstrap());
```

It writes `--bs-primary`, `--bs-primary-rgb`, `--bs-primary-text-emphasis`, `--bs-primary-bg-subtle`, `--bs-primary-border-subtle`, `--bs-link-color`, `--bs-link-color-rgb`, `--bs-link-hover-color`, `--bs-link-hover-color-rgb`, `--bs-body-bg`, `--bs-secondary-color`, `--bs-border-color`, `--bs-heading-color` and `--bs-focus-ring-color` in both modes, `--bs-body-color`, `--bs-body-color-rgb` and `--bs-secondary-bg` in dark mode, and `--bs-body-font-family` and the `--bs-border-radius` scale (`-sm`, `-lg`, `-xl`, `-xxl`). Each one references a token, e.g. `--bs-primary: var(--mytheme-primary)`. Bootstrap's components that compile their colors in, such as `.btn-primary`, still need the theme's CSS to read the tokens. TheAdmin doesn't use it: its Sass maps its tokens onto Bootstrap's variables itself.

#### Your own tokens

A contributor adds tokens, and optionally rules, after the generated ones. It receives the scheme, its resolved `ThemeSchemePalette` (the colors of each mode, the typography and shape), the mode, and a `ThemeSchemeTokenWriter` that names tokens under the theme's prefix. It is created through dependency injection each time the stylesheet is built, so it can take services.

```csharp
using System.Text;
using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.Schemes.Models;

public sealed class MyThemeSchemeTokens : IThemeSchemeTokenContributor
{
    public void WriteTokens(ThemeScheme scheme, ThemeSchemePalette palette, ThemeSchemeMode mode, ThemeSchemeTokenWriter tokens)
    {
        var colors = palette.Get(mode);

        // --mytheme-hero-bg, in both modes.
        tokens.Add("hero-bg", colors.Primary.Mix(colors.Page, 0.15));

        if (mode == ThemeSchemeMode.Light)
        {
            // Any other custom property, by its full name.
            tokens.AddVariable("--my-widget-accent", tokens.Var("accent"));
        }
    }

    // Optional: rules of its own, after the rules of the scheme.
    public void AppendRules(StringBuilder css, ThemeScheme scheme, ThemeSchemePalette palette)
        => css.Append($".my-swatch[data-scheme=\"{scheme.Id}\"]{{background:{palette.Primary.ToHex()}}}\n");
}
```

```csharp
services.AddThemeSchemes("MyTheme", options => options.AddTokens<MyThemeSchemeTokens>());
```

Writing a token that is already written replaces its value. `ThemeColor` parses and writes `#rrggbb` colors, mixes them and measures their WCAG contrast; `ThemeSchemePalette.TextContrast` and `UiContrast` are the contrasts the feature raises text and controls to.

#### Full control

`CssWriter` writes the whole CSS of each scheme with valid colors, in place of the generated tokens. `WriteDefault()` writes what would have been written without it, to wrap it.

```csharp
services.AddThemeSchemes("MyTheme", options => options.CssWriter = context =>
{
    var colors = context.Palette.Light;

    context.Css.Append(context.Selector)
        .Append("{--brand:").Append(colors.Primary.ToHex())
        .Append(";--brand-text:").Append(context.Palette.OnPrimary.ToHex())
        .Append(";}\n");
});
```

### 6. A picker and a preview

To let users pick a scheme, render the `theme-scheme-picker` script with a list of options. Clicking an option stores the choice in the cookie and applies it to the page at once; the empty value follows the site's default scheme again. Set `data-scheme-attribute` to the theme's `AttributeName`.

```cshtml
<script asp-name="theme-scheme-picker" at="Foot"></script>

<div role="group" aria-label="@T["Scheme"]"
     data-theme-scheme-picker
     data-scheme-attribute="data-scheme"
     data-cookie-name="@ThemeSchemes.CookieName("MyTheme")"
     data-site-scheme="@(await ThemeSchemes.GetSiteSchemeAsync("MyTheme"))">
    <button type="button" data-theme-scheme-value="">@T["Site default"]</button>
    @foreach (var scheme in await ThemeSchemes.ListAsync("MyTheme"))
    {
        <button type="button" data-theme-scheme-value="@scheme.Id">@scheme.Name</button>
    }
</div>
```

The editor of a scheme renders the `ThemeSchemePreview` shape, with the `ThemeId`, `SchemeId`, `SchemeAttributeName` (the theme's `AttributeName`) and `Scheme` properties. Without a template of its own, a theme gets a simple swatch preview. A theme draws its own by adding a `ThemeSchemePreview-{ThemeId}.cshtml` template, such as `ThemeSchemePreview-MyTheme.cshtml`. An element of it that carries the scheme attribute, set to the shape's `SchemeId`, is restyled live as the fields change, with the same CSS as the saved scheme. The theme's templates only apply while it's the active theme of the page, so the preview of a front-end theme, edited in the admin, falls back to the swatch preview. The swatch of the list of schemes has an alternate per theme too, `ThemeScheme.Swatch.SummaryAdmin-{ThemeId}.cshtml`.

### 7. Caching

The stylesheet of a theme, `~/Themes/schemes/{themeId}.css`, is built once per change of its schemes and kept in memory; pages never build it. Its URL carries a version, a short hash of the stylesheet, so it changes when a scheme is saved or deleted, or when the code that writes the CSS changes (e.g. after adding a contributor), and stays the same otherwise. The current version is cached by browsers for a year (`immutable`); any other is served with `no-cache`.

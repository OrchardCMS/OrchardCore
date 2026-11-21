# Admin Theme Conventions

Orchard Core provides specific attributes and services to manage Admin Themes within your modules.

## OCAT CSS Classes for Admin Editors

The Admin Theme uses a set of CSS classes prefixed with `ocat-` (Orchard Core Admin Theme) to style form editors. These classes decouple the HTML structure from any specific CSS framework, allowing theme authors to restyle the admin without modifying Razor views.

By default, the `ocat-*` classes render a stacked (vertical) form layout equivalent to Bootstrap's default form styling. To create alternative layouts (e.g., horizontal forms), override the `ocat-*` classes in your custom admin theme's SCSS/CSS.

### Available OCAT Classes

| CSS Class | Purpose | Default Style |
|-----------|---------|---------------|
| `ocat-wrapper` | Outer wrapper for each form field group | `margin-bottom: 1rem` |
| `ocat-label` | Label element styling | Same as Bootstrap `.form-label` |
| `ocat-label-required` | Additional class for required field labels | Appends a `*` indicator in the `--bs-danger` color, separated from the text by a `0.25rem` inline-start padding so it reads well in LTR and RTL |
| `ocat-end` | Container for the input and hint content | _(no additional styles)_ |
| `ocat-end-offset` | Container for input content when no label exists (e.g., standalone checkboxes) | _(no additional styles)_ |
| `ocat-start` | Leading element container (for push/alignment) | _(no additional styles)_ |
| `ocat-limited-wrapper` | Wrapper for limited-width fields (dropdowns, numbers) | Stacked field wrapper with `margin-bottom: 1rem` |
| `ocat-limited` | Constrains input width for limited-width fields | `100%`, then `50%` at `md`, `33.333%` at `lg`, `25%` at `xxl` |

### Using OCAT Classes in Custom Views

When building custom admin editor views, use the `ocat-*` classes so your views work with any admin theme.

**Standard field pattern:**

```html
<div class="ocat-wrapper" asp-validation-class-for="Title">
    <label asp-for="Title" class="ocat-label">@T["Title"]</label>
    <div class="ocat-end">
        <input asp-for="Title" class="form-control" />
        <span asp-validation-for="Title"></span>
        <span class="hint">@T["The title of the item."]</span>
    </div>
</div>
```

**Required field pattern:**

```html
<div class="ocat-wrapper" asp-validation-class-for="Name">
    <label asp-for="Name" class="ocat-label ocat-label-required">@T["Name"]</label>
    <div class="ocat-end">
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name"></span>
    </div>
</div>
```

**Checkbox pattern (no label column):**

```html
<div class="ocat-wrapper">
    <div class="ocat-end-offset">
        <div class="form-check">
            <input type="checkbox" class="form-check-input" asp-for="IsEnabled" />
            <label class="form-check-label" asp-for="IsEnabled">@T["Enable feature"]</label>
            <span class="hint dashed">@T["Check to enable this feature."]</span>
        </div>
    </div>
</div>
```

**Limited-width field pattern (for dropdowns, number inputs):**

```html
<div class="ocat-limited-wrapper">
    <label asp-for="PageSize" class="ocat-label">@T["Page size"]</label>
    <div class="ocat-limited">
        <input asp-for="PageSize" type="number" class="form-control" />
        <span class="hint">@T["The default page size."]</span>
    </div>
</div>
```

### Customizing Content Field Hints

Content field editors render their configured hint with the `Hint_Admin` shape. The default
`Hint.Admin.cshtml` template displays the hint as text:

```cshtml
<span class="hint dashed">@Model.Hint</span>
```

To customize all content field hints, add a `Views/Hint.Admin.cshtml` template to your custom
admin theme. For example, the following template renders an accessible button with a Bootstrap
tooltip:

```cshtml
<button type="button"
        class="btn btn-link p-0 align-baseline"
        data-bs-toggle="tooltip"
        data-bs-placement="top"
        title="@Model.Hint"
        aria-label="@Model.Hint">
    <i class="fa-regular fa-circle-question" aria-hidden="true"></i>
</button>
```

The shape exposes the configured hint through `Model.Hint`. The default admin theme initializes
Bootstrap tooltips for elements with the `data-bs-toggle="tooltip"` attribute.

### Creating a Horizontal Form Layout

To achieve a horizontal form layout, override the `ocat-*` classes in your custom admin theme CSS or SCSS. Here's a pure CSS example:

```css
/* Horizontal form layout for admin editors */
.ocat-wrapper {
    --ocat-gutter-x: 1.5rem;
    display: flex;
    flex-wrap: wrap;
    margin-right: calc(-0.5 * var(--ocat-gutter-x));
    margin-left: calc(-0.5 * var(--ocat-gutter-x));
    margin-bottom: 1rem;
}

.ocat-wrapper > *,
.ocat-limited-wrapper > * {
    box-sizing: border-box;
    flex-shrink: 0;
    width: 100%;
    max-width: 100%;
    padding-right: calc(var(--ocat-gutter-x) * 0.5);
    padding-left: calc(var(--ocat-gutter-x) * 0.5);
}

.ocat-label {
    flex: 0 0 auto;
    width: 25%;
    padding-top: calc(0.375rem + var(--bs-border-width));
    padding-bottom: calc(0.375rem + var(--bs-border-width));
    text-align: end;
}

.ocat-end {
    flex: 0 0 auto;
    width: 75%;
}

.ocat-end-offset {
    flex: 0 0 auto;
    width: 75%;
    margin-inline-start: 25%;
}

.ocat-start {
    flex: 0 0 auto;
    width: 25%;
}

.ocat-limited-wrapper {
    --ocat-gutter-x: 1.5rem;
    display: flex;
    flex-wrap: wrap;
    margin-right: calc(-0.5 * var(--ocat-gutter-x));
    margin-left: calc(-0.5 * var(--ocat-gutter-x));
    margin-bottom: 1rem;
}

.ocat-limited {
    flex: 0 0 auto;
    max-width: 33.333%;
    width: 33.333%;
}
```

With these styles applied, admin editors display with a horizontal form layout:

![content-fields](https://user-images.githubusercontent.com/24724371/195202615-d61a13f4-3b8e-4b6c-ab91-720fdf6e4d2e.gif)

![content-parts](https://user-images.githubusercontent.com/24724371/195202640-1f7c7dcf-191e-4246-9690-5a7a2bf8c03f.gif)

### Content Part & Field Wrapper Classes

For content parts and fields, render the wrapper classes directly in the view instead of using helper methods:

- `ocat-wrapper content-part-wrapper content-part-wrapper-{part-name}` for parts
- `ocat-wrapper field-wrapper field-wrapper-{part-name}-{field-name}` for fields

If the part or field is a named part, also append the named-part-specific class:

- `content-part-wrapper-{named-part-name}` for parts
- `field-wrapper-{named-part-name}-{field-name}` for fields

These classes enable targeted styling of specific content type editors without relying on C# helper extensions.

## Admin Schemes

The look of **TheAdmin** comes from a scheme: its colors, typefaces, base font size, density and corners. Schemes come from the **Theme Schemes** feature of the Themes module (`OrchardCore.Themes.Schemes`), which TheAdmin depends on and any other theme can opt into (see [Theme Schemes](../modules/Themes/Schemes.md)). Admins manage the schemes of a theme from the **Schemes** button of its card on **Design > Themes** (they need the `ApplyTheme` permission). Each theme has its own default scheme, set with the **Set as default** action of the list and shown with a **Default** badge; the default scheme can't be deleted. Each user can pick another scheme of TheAdmin from the theme menu of the top bar.

A scheme is made of four colors, from which every other color of the admin is derived for the light and the dark mode, each checked against the surfaces it is drawn on so that text reaches 4.5:1 and the boundaries of controls and focus indicators 3:1 (WCAG 2.2 AA):

| Color | Used for |
|-------|----------|
| Primary | Buttons, links, the current page of the menu, checked boxes, the toolbar of lists |
| Deep | Headings, and the top of the menu |
| Ink | The bottom of the menu, the darkest surface, the backdrop of the log in page |
| Accent | Links and highlights on dark surfaces |

Optional settings fine tune what is derived: the hue and saturation of the page, borders and dark mode surfaces, a light menu (light gray with dark text), the color of text on the primary, the page color, the toolbar color and the page gradient. A second group sets the typography and shape:

| Setting | Values | CSS custom property |
|---------|--------|---------------------|
| Typeface | Lato, Inter, Source Sans 3, System font | `--ta-font-family` |
| Heading typeface | The same choices, or the typeface of the body | `--ta-heading-font-family` |
| Base font size | 14 px to 18 px, as a share of the browser's own size | `--ta-font-scale` |
| Density | Compact, Default, Comfortable | `--ta-density` |
| Corners | Square, Subtle, Default, Round | `--ta-radius-scale` |

The fonts are served by the theme; the admin never calls a font CDN.

TheAdmin seeds eight schemes (Gray, the default, which keeps the look of the previous TheAdmin, Blue, Ocean, Forest, Plum, Sunset, Graphite and Gold) the first time its schemes are read. They are ordinary records from then on, and a theme is seeded once: deleting all of its schemes doesn't bring them back.

Schemes are exported with the `ThemeSchemes` deployment step and imported with the `ThemeSchemes` recipe step. Each entry names its theme (`ThemeId`) and its id (`ItemId`); an entry whose `ItemId` matches a scheme of that theme replaces it. The optional `Defaults` object maps a theme id to the id of its default scheme. Entries of a theme that doesn't support schemes, or isn't enabled, are skipped with an error.

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

The schemes of a theme are served as one stylesheet, `~/Themes/schemes/{themeId}.css`, built once per change and versioned by a short hash of its text.

### Supporting schemes in your theme

Any theme can offer schemes the way TheAdmin does: it depends on the `OrchardCore.Themes.Schemes` feature, registers itself with `services.AddThemeSchemes("MyTheme")`, and applies the current scheme in its layout. TheAdmin registers its attribute, its `ta` prefix, its eight schemes and the tokens only its layout uses (the menu, the toolbar of lists, the top bar, the page gradient and the icons of checked controls):

```csharp
services.AddThemeSchemes(TheAdminConstants.ThemeId, options =>
{
    options.AttributeName = TheAdminSchemes.AttributeName; // data-ta-scheme
    options.TokenPrefix = TheAdminSchemes.TokenPrefix; // ta
    options.DefaultSchemeId = TheAdminSchemes.DefaultId; // gray
    options.Defaults = [.. TheAdminSchemes.CreateDefaults()];
    options.AddTokens<TheAdminSchemeTokens>();

    // Its Sass names a few generated tokens otherwise, e.g. --ta-navy for --ta-deep.
    foreach (var (token, name) in TheAdminSchemes.TokenNames)
    {
        options.TokenNames[token] = name;
    }
});
```

See [Theme Schemes](../modules/Themes/Schemes.md) for the full guide.

### Design Tokens

Every component of the theme reads the same tokens, so a theme that depends on **TheAdmin** can restyle the admin by overriding them, without rebuilding its stylesheets:

| Tokens | Purpose |
|--------|---------|
| `--ta-font-size-xs`, `-sm`, `-md`, `-lg`, `-xl`, `-page-title` | The type scale. Body text, fields and buttons are `md`; hints, column headers, breadcrumbs, metadata and the items of the menu are `sm`; badges are `xs`. |
| `--ta-surface-padding-x`, `--ta-surface-padding-y`, `--ta-row-padding-y` | The padding inside every surface: card bodies and headers, list rows and toolbars, table cells, tab panes, alerts. |
| `--ta-stack-gap`, `--ta-content-padding-x` | The space between the surfaces of a page, and around the page. |
| `--ta-radius-sm`, `--ta-radius-md`, `--ta-radius`, `--ta-radius-lg` | The corners, from fields and buttons to cards and dialogs. |
| `--ta-primary`, `--ta-link`, `--ta-heading`, `--ta-label`, `--ta-muted`, `--ta-border`, `--ta-control-border`, `--ta-page-bg`, `--ta-surface`, `--ta-toolbar-bg`, `--ta-row-hover-bg`, `--ta-menu-*` | The colors of the scheme. Bootstrap's own variables (`--bs-primary`, `--bs-link-color`, `--bs-border-color`, `--bs-secondary-color`...) read them. |

## The `[Admin]` Attribute

To ensure a controller or a specific action uses the Admin Theme, decorate it with the `[Admin]` attribute. This is essential when creating custom administration pages.

```csharp
using OrchardCore.Admin;

[Admin]
public sealed class MyCustomAdminController : Controller
{
    // All actions in this controller will render using the Admin Theme
    public IActionResult Index()
    {
        return View();
    }
}
```

You can also apply it to specific actions if the rest of the controller should use the front-end theme:

```csharp
public sealed class SettingsController : Controller
{
    [Admin]
    public IActionResult AdminSettings()
    {
        return View(); // Uses Admin Theme
    }

    public IActionResult Profile()
    {
        return View(); // Uses Front-end Theme
    }
}
```

## AdminController Base Class

For convenience, inheriting from AdminController (in the OrchardCore.Admin namespace) automatically applies the [Admin] attribute to your entire controller.

## IThemeService

The **IThemeService** allows you to programmatically manage and discover themes. It is commonly used when you need to know which theme is currently active.

| Method                 | Description                                                                      |
| :--------------------- | :------------------------------------------------------------------------------- |
| `GetAdminThemeAsync()` | Returns the `IExtensionInfo` of the currently configured Admin Theme.            |
| `GetSiteThemeAsync()`  | Returns the `IExtensionInfo` of the currently configured Site (Front-end) Theme. |

Example Usage

```csharp
public sealed class MyService
{
    private readonly IThemeService _themeService;

    public MyService(IThemeService themeService)
    {
        _themeService = themeService;
    }

    public async Task GetCurrentTheme()
    {
        var adminTheme = await _themeService.GetAdminThemeAsync();
        var themeName = adminTheme?.Id;
    }
}
```

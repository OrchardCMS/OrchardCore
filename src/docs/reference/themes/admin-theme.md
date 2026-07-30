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
| `ocat-label-required` | Additional class for required field labels | Appends red `*` indicator |
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

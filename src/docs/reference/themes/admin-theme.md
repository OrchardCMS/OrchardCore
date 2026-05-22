# Admin Theme Conventions

Orchard Core provides specific attributes and services to manage Admin Themes within your modules.

## Style Settings for `TheAdmin` Theme

The `TheAdmin` theme supports configurable style settings that allow you to make the admin editor layout responsive. By customizing these settings, you can control the alignment and width of labels, inputs, and wrappers across all admin editor views.

### Configuration

Add the following settings to your `appsettings.json` file:

```json
{
  "OrchardCore": {
    "TheAdminTheme": {
      "StyleSettings": {
        "WrapperClasses": "row mb-3",
        "LimitedWidthWrapperClasses": "row",
        "LimitedWidthClasses": "col-md-6 col-lg-5 col-xxl-4",
        "StartClasses": "col-lg-2 col-xl-3",
        "EndClasses": "col-lg-10 col-xl-9",
        "LabelClasses": "col-form-label text-lg-end col-lg-2 col-xl-3",
        "OffsetClasses": "offset-lg-2 offset-xl-3"
      }
    }
  }
}
```

With these settings applied, the admin editor adopts a horizontal form layout where labels are aligned to the left (or right-aligned on large screens) and inputs are positioned in a dedicated column to the right.

### Settings Reference

| Setting | Default | Description |
|---------|---------|-------------|
| `WrapperClasses` | `mb-3` | CSS classes applied to each field's outer wrapper element. |
| `LimitedWidthWrapperClasses` | _(empty)_ | CSS classes for the wrapper of limited-width fields (e.g., dropdowns, number inputs). |
| `LimitedWidthClasses` | _(empty)_ | CSS classes constraining the width of limited-width field inputs. |
| `StartClasses` | _(empty)_ | CSS classes for the starting section, used to push elements to the end. |
| `EndClasses` | _(empty)_ | CSS classes for the ending section where inputs and hints are placed. |
| `LabelClasses` | `form-label` | CSS classes for label elements. |
| `OffsetClasses` | _(empty)_ | CSS classes used to offset content that has no label (e.g., standalone checkboxes). |

### Available Helpers

The following Razor helpers are available for use in admin editor views:

| Helper | Description |
|--------|-------------|
| `Orchard.GetWrapperClasses(params string[] additionalClasses)` | Gets the CSS classes for a field wrapper element. |
| `Orchard.GetLabelClasses(bool inputRequired, params string[] additionalClasses)` | Gets the CSS classes for a label element. |
| `Orchard.GetEndClasses(params string[] additionalClasses)` | Gets the CSS classes for the input/content container. |
| `Orchard.GetEndClasses(bool withOffset, params string[] additionalClasses)` | Gets the CSS classes for the input container with an optional offset (for elements without a label). |
| `Orchard.GetStartClasses(params string[] additionalClasses)` | Gets the CSS classes for a starting section. |
| `Orchard.GetOffsetClasses(params string[] additionalClasses)` | Gets the offset classes to align elements without a label. |
| `Orchard.GetLimitedWidthWrapperClasses(params string[] additionalClasses)` | Gets the CSS classes for the limited-width field wrapper. |
| `Orchard.GetLimitedWidthClasses(params string[] additionalClasses)` | Gets the CSS classes for limiting the width of an input (e.g., number fields, dropdowns). |

### Using Helpers in Custom Views

When building custom admin editor views, use the style helpers so your views align with the configured theme settings.

**Standard field pattern:**

```html
<div class="@Orchard.GetWrapperClasses()" asp-validation-class-for="Title">
    <label asp-for="Title" class="@Orchard.GetLabelClasses()">@T["Title"]</label>
    <div class="@Orchard.GetEndClasses()">
        <input asp-for="Title" class="form-control" />
        <span asp-validation-for="Title"></span>
        <span class="hint">@T["The title of the item."]</span>
    </div>
</div>
```

**Checkbox pattern (no label column):**

```html
<div class="@Orchard.GetWrapperClasses()">
    <div class="@Orchard.GetEndClasses(true)">
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
<div class="@Orchard.GetLimitedWidthWrapperClasses("mb-3")">
    <label asp-for="PageSize" class="@Orchard.GetLabelClasses()">@T["Page size"]</label>
    <div class="@Orchard.GetLimitedWidthClasses()">
        <input asp-for="PageSize" type="number" class="form-control" />
        <span class="hint">@T["The default page size."]</span>
    </div>
</div>
```

### Screenshots

With the configuration above applied, admin editors display with a horizontal form layout:

![content-fields](https://user-images.githubusercontent.com/24724371/195202615-d61a13f4-3b8e-4b6c-ab91-720fdf6e4d2e.gif)

![content-parts](https://user-images.githubusercontent.com/24724371/195202640-1f7c7dcf-191e-4246-9690-5a7a2bf8c03f.gif)

### Programmatic Configuration

You can also configure the style settings in code using `PostConfigure`:

```csharp
services.PostConfigure<TheAdminThemeOptions>(options =>
{
    options.WrapperClasses = "row mb-3";
    options.LimitedWidthWrapperClasses = "row";
    options.LimitedWidthClasses = "col-md-6 col-lg-5 col-xxl-4";
    options.StartClasses = "col-lg-2 col-xl-3";
    options.EndClasses = "col-lg-10 col-xl-9";
    options.LabelClasses = "col-form-label text-lg-end col-lg-2 col-xl-3";
    options.OffsetClasses = "offset-lg-2 offset-xl-3";
});
```

## The `[Admin]` Attribute

To ensure a controller or a specific action uses the Admin Theme, decorate it with the `[Admin]` attribute. This is essential when creating custom administration pages.

```csharp
using OrchardCore.Admin;

[Admin]
public class MyCustomAdminController : Controller
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
public class SettingsController : Controller
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
public class MyService
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

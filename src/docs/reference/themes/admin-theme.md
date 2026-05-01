# Admin Theme Conventions

Orchard Core provides specific attributes and services to manage Admin Themes within your modules.

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

AdminController Base Class

For convenience, inheriting from AdminController (found in the OrchardCore.Admin namespace) will automatically apply the [Admin] attribute to your entire controller.

##IThemeService

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

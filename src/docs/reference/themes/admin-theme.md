# Admin Theme

The Admin Theme in Orchard Core provides a specialized interface designed for administrative tasks and back-end management.

## Enabling the Admin Theme

There are three primary ways to trigger the Admin Theme for your controllers or Razor Pages:

### 1. Using the `[Admin]` Attribute

The most explicit way is to decorate your controller class or a specific action method with the `[Admin]` attribute.

```csharp
[Admin]
public class MyCustomController : Controller
{
    public IActionResult Index() => View();
}
```

### 2. Controller Naming Convention

Orchard Core automatically recognizes controllers that follow a specific naming pattern. Any controller whose name ends with `AdminController` or simply `Admin` (e.g., `DashboardAdminController`) will be rendered using the Admin Theme.

### 3. Razor Pages Folder Convention

For developers using Razor Pages, any page located within a folder named `Admin` (typically under `/Pages/Admin/`) will automatically use the Admin Theme by default without requiring additional attributes.

**Example path:** `/Pages/Admin/Index.cshtml`

## Technical Insights

The seamless switching between front-end and back-end themes is handled by the following core components:

- **AdminZoneFilter**: A global resource filter that intercepts requests. It checks the `ControllerName` or the `ViewEnginePath`. If it detects an "Admin" pattern, it calls `AdminAttribute.Apply()` to mark the request.

- **AdminAttribute**: Acts as a marker in the `HttpContext.Items` to signal that the current request belongs to the administrative interface.

- **AdminThemeSelector**: A theme selector with a priority of 100. It looks for the admin marker in the request context and, if found, returns the configured Admin Theme name via `IAdminThemeService`.

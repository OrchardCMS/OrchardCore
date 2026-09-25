# Admin (`OrchardCore.Admin`)

The Admin module provides an admin dashboard for your site.

## Extend the admin dashboard

The `AdminDashboard` shape renders an `AdminDashboardContent` child shape. Features can add their own positioned content to this shape by registering a shape table provider:

```csharp
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Views;

public sealed class AdminDashboardShapeTableProvider : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("AdminDashboardContent")
            .OnDisplaying(async displaying =>
            {
                await displaying.Shape.AddAsync(new ShapeViewModel("MyAdminDashboardContent"), "10");
            });

        return ValueTask.CompletedTask;
    }
}
```

Register the provider from the feature's `Startup` class:

```csharp
services.AddShapeTableProvider<AdminDashboardShapeTableProvider>();
```

The `MyAdminDashboardContent` shape is rendered using its matching shape template. Contributions are ordered by the position passed to `AddAsync()`.

## Custom Admin prefix

If you want to specify another prefix in the URLs to access the admin section, you can change it by using this option in the `appsettings.json`:

```json
  "OrchardCore": {
    "OrchardCore_Admin": {
      "AdminUrlPrefix": "YourCustomAdminUrl"
      }
    }
```

## Customize Admin branding

By default, OrchardCore logo and site name are displayed in the top navbar.

You can change it by overriding 'AdminBranding' shape, either from a [custom admin theme](../../../guides/create-admin-theme/README.md) or using the Admin Templates feature.  
You can also use this shape to define admin favicon.

Here are samples using logo and favicon from media module.

=== "Liquid"

    ``` liquid
    {% assign favicon_url = 'favicon.ico' | asset_url %}
    {% zone "HeadMeta" %}
        {% link rel:'shortcut icon', type:'image/x-icon', src:favicon_url %}
    {% endzone %}
    {% a area: 'OrchardCore.Admin', controller: 'Admin' , action: 'Index', class: 'ta-navbar-brand' %}
        <div class="d-flex align-items-center">
            <img src="{{ 'logo.png' | asset_url }}" alt="{{ Site.SiteName }}" class="pe-2" />
            <span>{{ Site.SiteName }}</span>
        </div>
    {% enda %}
    ```

=== "Razor"

    ``` html
    <zone name="HeadMeta">
        <link asp-src="~/media/favicon.ico" type="image/x-icon" rel="shortcut icon" />
    </zone>
    <a class="ta-navbar-brand" asp-route-area="OrchardCore.Admin" asp-controller="Admin" asp-action="Index">
        <div class="d-flex align-items-center">
            <img src=@Url.Content("~/media/logo.png") alt="@Site.SiteName" class="pe-2" />
            <span>@Site.SiteName</span>
        </div>
    </a>
    ```

## Quick navigation

The `OrchardCore.Admin` module provides a quick navigation palette to jump to an admin page from the keyboard. Press <kbd>Ctrl</kbd>+<kbd>K</kbd> (<kbd>Cmd</kbd>+<kbd>K</kbd> on macOS) anywhere in the admin, or click the magnifier icon in the top navbar to open it. Typing filters navigation destinations; matching is case- and accent-insensitive and includes the parent names displayed as a breadcrumb under each result. Use the arrow keys to move the highlight, <kbd>Enter</kbd> to open the highlighted page, and <kbd>Esc</kbd> to close the palette. Quick navigation is not a full-text content search.

The built-in `AdminMenuItemNavigationSource` supplies admin menu destinations using `INavigationManager`, independently of the sidebar markup. It preserves menu ordering, breadcrumbs, link targets, authorization, localization, and tenant/admin URL prefixes. It builds its entries on demand because menu providers can depend on the current request. Only admin menu items are included by default. Enable the optional [Content Quick Navigation feature](../Contents/README.md#content-quick-navigation) to include the 50 most recently updated content items that you can edit; its count is configurable in Admin settings. The shortcut is not intercepted while the focus is inside a rich text or code editor that uses <kbd>Ctrl</kbd>+<kbd>K</kbd> for its own commands.

The palette is enabled by default and can be turned off from **Configuration → Settings → Admin** (**Enable quick navigation**) or with the `DisplayQuickNavigation` admin setting (see [Recipe Configuration](#recipe-configuration)).

### Admin theme integration

The `OrchardCore.Admin` module owns `QuickNavigationNavbarDisplayDriver`, the default `QuickNavigationNavbarItem` shape, and the palette's JavaScript and CSS. Admin themes that render the [Navbar shape](#navbar-shape) with the `DetailAdmin` display type inherit the palette without adding navigation-specific files or depending on `TheAdmin`. Pre-render the navbar before the header resources, and render footer scripts as usual.

The shape requires the `admin-quick-navigation` script and stylesheet resources only when the palette is enabled. The script initializes itself and depends on Bootstrap 5; the stylesheet requires Font Awesome 7 and the appropriate Bootstrap 5 stylesheet for the current text direction.

Breadcrumbs follow the page's text direction, with mirrored decorative separators in right-to-left layouts. Each label is directionally isolated so mixed-language paths retain their hierarchy. Sources provide localized labels; the separator itself does not require translation.

Themes can customize the `.admin-quick-navigation` styles and Bootstrap color variables without copying the JavaScript or view. Load theme overrides after the `admin-quick-navigation` stylesheet, for example:

```html
<style asp-name="my-admin-theme" asp-src="~/MyAdminTheme/Styles/theme.css" depends-on="admin-quick-navigation" at="Head"></style>
```

A theme can also override the `QuickNavigationNavbarItem` shape if it needs different markup, preserving the resource requirements and the element IDs and data attributes used by the script.

### Index document and caching

The palette loads its JSON index from the `AdminQuickNavigationIndex` named route (`~/Admin/QuickNavigation/Index` with the default admin prefix). Generate this URL with routing helpers rather than hard-coding it. The endpoint requires authentication and the `AccessAdminPanel` permission, and returns `404` when quick navigation is disabled.

Each entry contains `source`, `id`, `title`, `path` (an array of breadcrumb labels), `href`, and `target`. Titles and breadcrumbs are localized plain text. The palette filters this document locally; typing does not issue additional requests.

Opening the palette requests a fresh document with `Cache-Control: no-store`. The controller executes every registered source and renders the authorized, localized results on every request; it does not use ETags or return `304 Not Modified`. The palette's URL includes the page's current culture. No index is stored in browser HTTP caches or local storage. Loading failures are shown explicitly; closing and reopening retries the request.

Caching belongs to individual sources, not the controller. A source may reuse culture-independent identifiers or other user-independent data when it can reliably invalidate them as their dependencies change. It may use `IQuickNavigationIndex` or maintain its own cache, for example with change tokens from `ISignal`. Sources without a reliable invalidation mechanism remain request-driven. Authorization and localization still run when rendering entries; cached identifiers must never be treated as permission to display a destination.

### Adding an opt-in source

The extension points are in `OrchardCore.Admin.QuickNavigation`, in `OrchardCore.Admin.Abstractions`. Derive from `QuickNavigationSource` and register it as a scoped service in a separate feature depending on `OrchardCore.Admin`:

```csharp
[Feature("MyModule.QuickNavigation")]
public sealed class QuickNavigationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<QuickNavigationSource, MyNavigationSource>();
    }
}
```

Declare the feature in your module's manifest and leave it disabled by default. Enabling or disabling its feature controls whether the source participates, allowing administrators to choose the functionality and indexing cost they want. Sources for content types, parts, or workflow names are not enabled or supplied by the Admin module.

A source implements:

| Member | Responsibility |
|--------|----------------|
| `Name` | A unique, stable, culture-independent name. This routes indexed identifiers back to their originating source. Duplicate source names are not allowed. |
| `GetEntryIdsAsync()` | Optionally supply request-specific identifiers. The default implementation returns none. Use this for request-dependent sources such as admin menus, or for lazy population after a tenant restart. |
| `DisplayAsync(string entryId)` | Load the destination, check its permissions for the current user, and return a fresh `QuickNavigationResult` with localized `Title` and `Path`, a generated `Href`, and optional `Target`. Return `null` for missing or unauthorized entries. `Source` and `Id` are assigned by the endpoint. |

Sources do not receive an MVC `ActionContext`. Inject `IHttpContextAccessor` to obtain the current user from `HttpContext?.User`, and authorize with `IAuthorizationService`, following other Orchard Core services. Generate links with routing services such as `LinkGenerator` using the current `HttpContext` to preserve tenant and admin URL prefixes. The admin-menu source uses the existing `GetActionContextAsync()` helper internally to call `INavigationManager`; that MVC integration is not part of the source contract.

For expensive sources, inject the tenant-singleton `IQuickNavigationIndex` into a feature-owned background task or event handler. Publish a snapshot of culture-independent identifiers:

```csharp
index.Replace("MyNavigationSource", destinationIds);
```

`Replace` atomically replaces only that source's partition, removes duplicates while preserving order, and takes a defensive copy. Pass an empty collection to remove its entries. Reads return immutable snapshots, so updates can occur while a palette request is being rendered. The index is in memory, not persisted or distributed: each tenant instance must repopulate it after restart, and each server must receive updates. A source can instead maintain its own persistent/cache-backed data and expose its identifiers through `GetEntryIdsAsync`.

The endpoint combines stored and request-specific identifiers, deduplicates them within each source, and calls the registered originating source's `DisplayAsync` for each identifier. Disabled/unregistered sources are never rendered. Store identifiers, not localized labels, user-specific URLs, or pre-authorized results: the same indexed identifier can then be rendered in different languages, including data-localized labels, without reindexing it. Always authorize again at display time and generate links using the current request's routing context.

## Navbar Shape

The navigation bar shape is available in two display types: `Detail` for the frontend and `DetailAdmin` for the backend admin. The `Navbar` shape is composed and used by the `TheAdmin` and `TheTheme` themes. If you wish to compose and use the `Navbar` shape in other themes, you may create it using two steps

=== "Liquid"

    ``` liquid
    // Construct the shape at the beginning of the layout.liquid file to enable navbar items to potentially contribute to the resources output as necessary.
    {% assign navbar = Navbar() | shape_render %}

    // Subsequently in the layout.liquid file, invoke the shape at the location where you want to display it.
    {{ navbar }}
    ```

=== "Razor"

    ``` html
    @inject IDisplayManager<Navbar> DisplayManager
    @inject IUpdateModelAccessor UpdateModelAccessor
    @{
        // Construct the shape at the beginning of the layout.cshtml file to enable navbar items to potentially contribute to the resources output as necessary.
        var navbar = await DisplayAsync(await DisplayManager.BuildDisplayAsync(UpdateModelAccessor.ModelUpdater, "Detail"));
    }

    // Subsequently in the layout.cshtml file, invoke the shape at the location where you want to display it.
    @navbar
    ```

If you wish to add a menu item to the navbar, simply create a display driver for `Navbar`.

As an illustration, we inject the Visit Site link into `DetailAdmin` display type using a display driver as outlined below:

```csharp
public class VisitSiteNavbarDisplayDriver : DisplayDriver<Navbar>
{
    public override IDisplayResult Display(Navbar model, BuildDisplayContext context)
    {
        return View("VisitSiteNavbarItem", model)
            .Location(OrchardCoreConstants.DisplayType.DetailAdmin, "Content:20");
    }
}
```

You can change it by overriding 'VisitSiteNavbarItem' shape, either from a [custom admin theme](../../../guides/create-admin-theme/README.md) or using the Admin Templates feature.

=== "Liquid"

    ``` liquid
    <li class="nav-item">
        <a href="{{ '~' | absolute_url }}" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="bottom" title="{{ "Visit Site" | t }}" role="button">
            <i class="fa-solid fa-external-link" aria-hidden="true"></i>
        </a>
    </li>
    ```

=== "Razor"

    ``` html
    <li class="nav-item">
        <a href="@Url.Content("~/")" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="bottom" title="@T["Visit Site"]" role="button">
            <i class="fa-solid fa-external-link" aria-hidden="true"></i>
        </a>
    </li>
    ```

## Admin Routes

The `[Admin]` attribute has optional parameters for a custom route template and route name. It works just like the `[Route(template, name)]` attribute, except it prepends the configured admin prefix. You can apply it to the controller or the action; if both are specified then the action's template takes precedence. The route name can contain `{area}`, `{controller}`, and `{action}`, which are substituted during mapping so the names can be unique for each action. This means you don't have to define these admin routes in your module's `Startup` class anymore, but that option is still available and supported. Take a look at this example:

```csharp
[Admin("Person/{action}/{id?}", "Person{action}")]
public sealed class PersonController : Controller
{
    [Admin("Person", "Person")]
    public IActionResult Index() { ... }

    public IActionResult Create() { ... }

    public IActionResult Edit(string id) { ... }
}
```

In this example, (if the admin prefix remains the default "Admin") you can reach the Index action at `~/Admin/Person` (or by the route name `Person`), because its own action-level attribute took precedence. You can reach Create at `~/Admin/Person/Create` (route name `PersonCreate`) and Edit for the person whose identifier string is "john-doe" at `~/Admin/Person/john-doe` (route name `PersonEdit`).

## Recipe Configuration

The admin settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "AdminSettings": {
        "DisplayThemeToggler": true,
        "DisplayMenuFilter": true,
        "DisplayQuickNavigation": true,
        "DisplayNewMenu": true,
        "DisplayTitlesInTopbar": true
      }
    }
  ]
}
```

| Property                | Type    | Description                                                       |
|-------------------------|---------|-------------------------------------------------------------------|
| `DisplayThemeToggler`   | Boolean | Whether to display the light/dark theme toggler in the admin.     |
| `DisplayMenuFilter`     | Boolean | Whether to display the menu filter input in the admin navigation. |
| `DisplayQuickNavigation` | Boolean | Whether to display the quick navigation palette (Ctrl+K / Cmd+K) in the admin navbar. Defaults to `true`. |
| `DisplayNewMenu`        | Boolean | Whether to display the 'New' menu in the admin navigation.        |
| `DisplayTitlesInTopbar` | Boolean | Whether to display page titles in the top bar.                    |

# Admin (`OrchardCore.Admin`)

The Admin module provides an admin dashboard for your site.

## Custom Admin prefix

If you want to specify another prefix in the urls to access the admin section, you can change it by using this option in the appsettings.json:

``` json
  "OrchardCore": {
    "OrchardCore_Admin": {
      "AdminUrlPrefix": "YourCustomAdminUrl"
      }
    }
```

## Customize Admin branding

By default, OrchardCore logo and site name are displayed in the top navbar.

You can change it by overriding 'AdminBranding' shape, either from a [custom admin theme](../../../guides/create-admin-theme/README.md) or using Admin Templates feature.  
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

## Navbar Shape

The navigation bar shape is available in two display types `Detail` for the frontend and `DetailAdmin` for the backend admin. The `Navbar` shape is composed and used `TheAdmin` and `TheTheme` themes. If you wish to compose and use the `Navbar` shape in other themes, you may create it using two steps


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
            .Location("DetailAdmin", "Content:20");
    }
}
```

You can change it by overriding 'VisitSiteNavbarItem' shape, either from a [custom admin theme](../../../guides/create-admin-theme/README.md) or using Admin Templates feature.  

=== "Liquid"

    ``` liquid
    <li class="nav-item">
        <a href="{{ '~' | absolute_url }}" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="bottom" title="{{ "Visit Site" | t }}" role="button">
            <i class="fa-solid fa-fw fa-external-link" aria-hidden="true"></i>
        </a>
    </li>
    ```

=== "Razor"

    ``` html
    <li class="nav-item">
        <a href="@Url.Content("~/")" class="nav-link" data-bs-toggle="tooltip" data-bs-placement="bottom" title="@T["Visit Site"]" role="button">
            <i class="fa-solid fa-fw fa-external-link" aria-hidden="true"></i>
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

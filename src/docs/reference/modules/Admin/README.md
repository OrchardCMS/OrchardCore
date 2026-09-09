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

## Admin List Layouts

Admin lists such as the content items list are rendered by the `AdminList` shape, which supports several layouts. Three layouts are built in:

| Layout  | Description                                                                                         |
| ------- | --------------------------------------------------------------------------------------------------- |
| `List`  | The default. The items are rendered in a vertical list, each item by its own `SummaryAdmin` shape. |
| `Table` | The items are rendered in a table, one column per `AdminListColumn`.                               |
| `Grid`  | The same columns as `Table`, rendered with a CSS grid instead of a `<table>`. The header, the body and the rows are `display: contents`, so every cell shares the tracks computed from the column widths and the header always lines up with the data. |

Both column layouts use a CSS container query on the list itself: when the list is narrower than 48rem (whatever the viewport, the admin sidebar takes part of it), the headers disappear and each row becomes a compact wrapped line, checkbox and title first, badges after, actions at the end, so the page never scrolls horizontally. The cells keep a `data-title` attribute with the column title for themes that want to show it.

The layout is selected in **Configuration → Settings → Admin** under **List layout**, and stored in `AdminSettings.ListLayout`. When that setting is empty the value comes from `AdminListOptions.DefaultLayout`, which a site can set per tenant in `appsettings.json`:

```json
  "OrchardCore": {
    "AdminList": {
      "DefaultLayout": "Table",
      "DefaultActionsLayout": "Menu"
    }
  }
```

`AdminListOptions` is the single place the shipped defaults are decided, so changing them for a site never means editing a template or a driver. The values are `List`, `Table` and `Grid` for `DefaultLayout`, and `Buttons` or `Menu` for `DefaultActionsLayout`. A blank or missing value falls back to `List` and `Buttons`.

The options follow Orchard Core's signal-backed options pattern. `AdminListOptionsConfiguration` layers the site settings on top of the configuration section, the module registers `AddSignalOptionsChangeTokenSource<AdminListOptions>()`, and the settings driver calls `IOptionsUpdateNotifier.RequestUpdate<AdminListOptions>()` when the choice changes. Consumers inject `IOptionsMonitor<AdminListOptions>` and read `CurrentValue`, so saving the settings takes effect without releasing the shell.

### How it works

The rows of a list are shapes built with the `SummaryAdmin` display type, e.g. `Content_SummaryAdmin`. Display drivers and `placement.json` place shapes in the zones of these rows (`Checkbox`, `Title`, `Type`, `Header`, `Tags`, `Meta`, `Actions`, `ActionsMenu`, `Content`, ...). The layout only decides how a row is presented:

- In the `List` layout the row shape is rendered as a whole, so its template (e.g. `Content.SummaryAdmin.cshtml` or `Content-BlogPost.SummaryAdmin.cshtml`) decides the look.
- In the `Table` layout each column renders one or more zones of the row in an `AdminListCell` shape. The row template is not used.

The `AdminList` shape is created by the owner of the list with these properties:

| Property       | Description                                                                                    |
| -------------- | ---------------------------------------------------------------------------------------------- |
| `Name`         | The name of the list, e.g. `Contents`.                                                         |
| `Layout`       | The layout name, usually resolved with `IAdminListService.GetLayoutAsync()`.                  |
| `Columns`      | The `AdminListColumn` collection, usually built with `IAdminListService.GetColumnsAsync()`.   |
| `Rows`         | The row shapes. (`Items` cannot be used: it is the shape's own child collection.) The `Classes` and `Attributes` of a row shape are rendered on its `<li>` or `<tr>`, e.g. `data-filter-value` for the client-side search of the `list-management` script. |
| `Header`       | The options editor shape whose `Summary` and `Actions` zones are rendered above the items.     |
| `Toolbar`      | Alternative to `Header` for lists without an options editor: a shape rendered as is above the items. The `AdminListToolbar` shape renders the item count, the select-all checkbox and a bulk actions dropdown from its `ItemsCount`, `TotalItemCount`, `StartIndex`, `EndIndex` and `BulkActions` properties. |
| `Pager`        | The pager shape.                                                                               |
| `ItemCssClass` | Optional. The CSS classes of each item in the `List` layout.                                   |
| `EmptyMessage` | Optional. The message displayed when there are no items.                                       |

```csharp
var listShape = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
{
    Name = "Contents",
    Layout = await adminListService.GetLayoutAsync("Contents", cancellationToken: HttpContext.RequestAborted),
    Columns = await adminListService.GetColumnsAsync("Contents", defaultColumns, HttpContext.RequestAborted),
    Rows = contentItemSummaries,
    Header = header,
    Pager = pagerShape,
}));
```

### Overriding templates

The following alternates are available, from the least to the most specific:

| Shape           | Alternates                                                                     | Template examples                                                        |
| --------------- | ------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| `AdminList`     | `AdminList__{Layout}`, `AdminList__{Name}`, `AdminList__{Name}__{Layout}`      | `AdminList-Table.cshtml`, `AdminList-Contents.cshtml`, `AdminList-Contents-Table.cshtml` |
| `AdminListCell` | `AdminListCell__{Column}`, `AdminListCell__{Name}__{Column}`                   | `AdminListCell-Actions.cshtml`, `AdminListCell-Contents-Title.cshtml`    |

### Column properties

| Property    | Description                                                                                                                                                                  |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Name`      | The technical name, used for the `AdminListCell__{Name}` alternates and the `admin-list-column-{name}` CSS class.                                                            |
| `Title`     | The localized header text. `null` for a column without a header, e.g. the selection checkbox.                                                                                |
| `Position`  | The position in the placement syntax (`10`, `25`, `35.5`, or `end` to stay last). Columns are sorted by position once every provider ran, so several features can insert columns between the defaults without knowing each other. Default columns without a position get `10`, `20`, ...; a provider column without a position goes after all positioned columns. |
| `Zones`     | The zones of the row shape rendered in the cell, in order.                                                                                                                   |
| `Width`     | Any CSS width (`20%`, `12rem`), or `AdminListColumn.AutoWidth` (`auto`) to make the column as narrow as its content. Columns without a width share the remaining space.        |
| `Alignment` | `Start` (default), `Center` or `End`. Applied to the header and the cells.                                                                                                   |
| `NoWrap`    | Prevents the cell content from wrapping, e.g. dates and badges.                                                                                                              |
| `CssClass`  | Extra CSS classes added to the header and the cells.                                                                                                                         |

```csharp
new AdminListColumn
{
    Name = "Modified",
    Title = S["Last modified"],
    Zones = ["Meta"],
    Width = "18%",
    NoWrap = true,
},
new AdminListColumn
{
    Name = "Actions",
    Title = S["Actions"],
    Zones = ["Actions", "ActionsMenu"],
    Width = AdminListColumn.AutoWidth,
    Alignment = AdminListColumnAlignment.End,
},
```

### Row actions

The `Actions` and `ActionsMenu` zones of a row are rendered by the `AdminListActions` shape in the layout selected under **Configuration → Settings → Admin → List actions layout** (`AdminSettings.ListActionsLayout`), falling back to `AdminListOptions.DefaultActionsLayout`. Two layouts are built in:

| Layout    | Description                                                                                                   |
| --------- | ------------------------------------------------------------------------------------------------------------- |
| `Buttons` | The default. The shapes of the `Actions` zone as buttons, followed by an "Actions" dropdown for `ActionsMenu`. |
| `Menu`    | A single dropdown opened by an ellipsis button, holding the `Actions` shapes (restyled as menu items) and the `ActionsMenu` shapes. |

Row templates render it with `@await DisplayAsync(await New.AdminListActions(Row: Model))`, and the `Table` and `Grid` layouts render it in the `Actions` column, so the actions layout applies to every list layout. The alternates are `AdminListActions__{Layout}`, `AdminListActions__{ListName}` and `AdminListActions__{ListName}__{Layout}`. An actions layout is discovered like a list layout: add `AdminListActions-Icons.cshtml` to render it and `AdminListActions-Icons.Option.cshtml` to make it selectable.

### Adding a layout

A layout is discovered from the shape table, like a content field editor. To add a `Cards` layout, add two templates to a module or a theme:

- `AdminList-Cards.cshtml` renders the list. The `Rows` are the row shapes, so the template can display a row as a whole with `@await DisplayAsync(item)`, or read its zones through `IZoneHolding`, like `AdminList-Table.cshtml` does.
- `AdminListLayout-Cards.Option.cshtml` renders an `<option>` element so the layout can be selected in the admin settings:

```html
@{
    string current = Model.ListLayout;
}
<option value="Cards" selected="@(current == "Cards")">@T["Cards"]</option>
```

### Adding a column

Implement `IAdminListColumnProvider` to add, remove or reorder the columns of a list. Each column names the zones of the row shape it renders and its `Position` decides where it goes, whatever the order the providers run in. `context.Find(name)` and `context.Remove(name)` help altering existing columns:

```csharp
public sealed class CultureColumnProvider : IAdminListColumnProvider
{
    private readonly IStringLocalizer S;

    public CultureColumnProvider(IStringLocalizer<CultureColumnProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
    {
        if (context.ListName == "Contents")
        {
            // Between "Title" (20) and "Type" (30), regardless of the other providers.
            context.Columns.Add(new AdminListColumn
            {
                Name = "Culture",
                Position = "25",
                Title = S["Culture"],
                Zones = ["Culture"],
                Width = "10%",
            });
        }

        return Task.CompletedTask;
    }
}
```

```csharp
services.AddAdminListColumnProvider<CultureColumnProvider>();
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
| `DisplayNewMenu`        | Boolean | Whether to display the 'New' menu in the admin navigation.        |
| `DisplayTitlesInTopbar` | Boolean | Whether to display page titles in the top bar.                    |

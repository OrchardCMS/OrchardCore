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

Both column layouts use a CSS container query on the list itself: when the list is narrower than 48rem (whatever the viewport, the admin sidebar takes part of it), the headers disappear and a row takes the shape of a `List` row — the selection and the title share the first line with the actions at its end, and every other cell takes a line under them — so the page never scrolls horizontally. The cells keep a `data-title` attribute with the column title for themes that want to show it.

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

### Letting a user choose

**Let users choose the layout of a list** (`AdminSettings.AllowUserListLayoutSelection`, or `AllowUserSelection` in the configuration) adds a selector to every list, one button per available layout, beside the item count:

Clicking one reloads the page with `?layout=Grid`, and the choice is kept in a cookie **per list**, so a user can read the content items as a table and the users as a grid while everyone else sees the site default. The layout resolves from the most specific source to the least:

| Source | Wins when |
| --- | --- |
| The layout the page passed to `GetLayoutAsync` | always: the page is rendering itself |
| `?layout=` in the query string | the site lets a user choose |
| The cookie, for this list | the site lets a user choose and no layout was asked for |
| The site setting, then `AdminListOptions.DefaultLayout` | otherwise |

A layout the site cannot render is ignored, so a stale cookie or a hand-written query string cannot reach for a template that is not there. Turning the setting off makes every list use the site default again, cookies and query strings included.

The pager and the page size selector keep the query string of the page they are on, so paging or changing the page size holds on to the layout. A custom layout joins the selector by declaring itself (`AdminListLayout-Cards.Option.cshtml`) and can ship the button offering it, `AdminListLayoutSelectorItem-Cards.cshtml`, which is how the built-in layouts carry their icon.

The options follow Orchard Core's signal-backed options pattern. `AdminListOptionsConfiguration` layers the site settings on top of the configuration section, the module registers `AddSignalOptionsChangeTokenSource<AdminListOptions>()`, and the settings driver calls `IOptionsUpdateNotifier.RequestUpdate<AdminListOptions>()` when the choice changes. Consumers inject `IOptionsMonitor<AdminListOptions>` and read `CurrentValue`, so saving the settings takes effect without releasing the shell.

### How it works

The rows of a list are shapes built with the `SummaryAdmin` display type, e.g. `Content_SummaryAdmin`. Display drivers and `placement.json` place shapes in the zones of these rows (`Checkbox`, `Title`, `Type`, `Header`, `Tags`, `Meta`, `Actions`, `ActionsMenu`, `Content`, ...). The layout only decides how a row is presented:

- In the `List` layout the row shape is rendered as a whole, so its template (e.g. `Content.SummaryAdmin.cshtml` or `Content-BlogPost.SummaryAdmin.cshtml`) decides the look.
- In the `Table` and `Grid` layouts each column renders one or more zones of the row in an `AdminListCell` shape. The row template is not used.

A layout renders the whole listing, not only the rows. The shipped ones render, in order: the action bar holding `Search` and `Actions`, then `Toolbar` or `Header`, then the rows (or the empty message), then a footer holding `Pager` and `PageSize`. A custom layout is free to order them differently, to leave one out, or to put the search bar beside the pager.

The `AdminList` shape is created by the owner of the list with these properties:

| Property       | Description                                                                                    |
| -------------- | ---------------------------------------------------------------------------------------------- |
| `Name`         | The name of the list, e.g. `Contents`.                                                         |
| `Layout`       | The layout name, usually resolved with `IAdminListService.GetLayoutAsync()`.                  |
| `Columns`      | The `AdminListColumn` collection, usually built with `IAdminListService.GetColumnsAsync()`.   |
| `Rows`         | The row shapes. (`Items` cannot be used: it is the shape's own child collection.) The `Classes` and `Attributes` of a row shape are rendered on its `<li>` or `<tr>`, e.g. `data-filter-value` for the client-side search of the `list-management` script. |
| `Header`       | The options editor shape whose `Summary` and `Actions` zones are rendered above the items.     |
| `Toolbar`      | Alternative to `Header` for lists without an options editor: a shape rendered as is above the items. The `AdminListToolbar` shape renders the item count, the select-all checkbox and a bulk actions dropdown from its `ItemsCount`, `TotalItemCount`, `StartIndex`, `EndIndex` and `BulkActions` properties. |
| `Search`       | Optional. The search bar of the list. The `AdminListSearch` shape renders the standard one from its `Name`, `Value`, `Placeholder`, `Id`, `SubmitName` and `Autofocus` properties, and renders its `Filters` zone before the input, e.g. a filter dropdown. |
| `Actions`      | Optional. The buttons of the page, e.g. "Add", rendered beside the search.                     |
| `Pager`        | The pager shape.                                                                               |
| `PageSize`     | Optional. The page size selector, so the layout places it instead of the pager. Build it with `PageSizeSelector.BuildOptions()` and set `ShowPageSizeSelector = false` on the pager so it is not rendered twice. |
| `RowsAttributes` | Optional. Attributes rendered on the element wrapping the rows, whichever layout renders it, e.g. the id a sortable script needs. |
| `ItemCssClass` | Optional. The CSS classes of each item in the `List` layout.                                   |
| `EmptyMessage` | Optional. The message displayed when there are no items.                                       |

Everything a listing is made of belongs to the shape, so a layout decides where each part goes: a layout can put the search bar beside the pager, drop the page size selector, or move the actions of the page under the rows. A list that passes none of the optional properties simply renders without them.

```csharp
var listShape = await shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
{
    Name = "Contents",
    Layout = await adminListService.GetLayoutAsync("Contents", cancellationToken: HttpContext.RequestAborted),
    Columns = await adminListService.GetColumnsAsync("Contents", defaultColumns, HttpContext.RequestAborted),
    Rows = contentItemSummaries,
    Header = header,
    Search = await shapeFactory.CreateAsync("AdminListSearch", Arguments.From(new
    {
        Name = "Options.Search",
        Value = options.Search,
    })),
    Pager = pagerShape,
}));
```

### Row templates

In the `List` layout the row shape renders as a whole. The shipped rows all follow the same shape, so the lists look alike:

```html
<div class="row g-0 align-items-center">
    <div class="col">
        <div class="title d-flex align-items-center">
            @* Handle, Checkbox, then the summary: Content (the title), Tags, Meta *@
        </div>
        @* Description *@
    </div>
    <div class="col-auto d-flex justify-content-end ps-2">
        @await DisplayAsync(await New.AdminListActions(Row: Model))
    </div>
</div>
```

The actions sit beside the whole row, centred on it, and everything else is stacked in the column left of them, so a description stops where the actions begin instead of running under them. The conventions inside a row:

| Zone | Renders as |
| ---- | ---------- |
| `Content` | The title, as a plain link to the edit page. Not a heading: a row should not shout next to the other lists. |
| `Tags`, `Meta` | One `<span class="badge ta-badge fw-normal">` per fact, with a leading icon and a `title` tooltip naming the fact. |
| `Description` | A `<div class="admin-list-secondary">`, which takes its own line under the title. |
| `Actions`, `ActionsMenu` | Through `AdminListActions`, never a hand-written button group. |

Items of the `ActionsMenu` zone are **bare** `<a>` or `<button>` elements with `.dropdown-item`. Wrapping them in `<li>` breaks the row: the dropdown lives inside the `<li>` of the row in the `List` layout, and the HTML parser hoists a nested `<li>` out of it, taking the rest of the row with it.

### Sortable lists

A list whose order is data, e.g. the URL rewriting rules, marks the element wrapping its rows with `RowsAttributes` so its script can find it, whichever layout renders it:

```csharp
RowsAttributes = new Dictionary<string, string> { ["id"] = "rewrite-rules-sortable-list" },
```

Two things a sortable list has to know:

- The `Grid` layout cannot be dragged: its rows are `display: contents`, so they have no box for a drag script to pick up. Fall back to `Table` when `Grid` is configured, or force `List` the way the ordering of a list part does.
- SortableJS `oldIndex` and `newIndex` count **every** sibling of the dragged element, including a toolbar rendered among the rows. Use `oldDraggableIndex` and `newDraggableIndex`, which count only the rows, so the indexes are the same in every layout.

### Overriding templates

The following alternates are available, from the least to the most specific:

| Shape              | Alternates                                                                           | Template examples                                                        |
| ------------------ | ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| `AdminList`        | `AdminList__{Layout}`, `AdminList__{Name}`, `AdminList__{Name}__{Layout}`            | `AdminList-Table.cshtml`, `AdminList-Contents.cshtml`, `AdminList-Contents-Table.cshtml` |
| `AdminListCell`    | `AdminListCell__{Column}`, `AdminListCell__{Name}__{Column}`                         | `AdminListCell-Actions.cshtml`, `AdminListCell-Contents-Title.cshtml`    |
| `AdminListActions` | `AdminListActions__{Layout}`, `AdminListActions__{Name}`, `AdminListActions__{Name}__{Layout}` | `AdminListActions-Menu.cshtml`, `AdminListActions-Contents.cshtml`, `AdminListActions-Contents-Menu.cshtml` |
| `AdminListToolbar` | `AdminListToolbar__{Layout}`, `AdminListToolbar__{Name}`, `AdminListToolbar__{Name}__{Layout}` | `AdminListToolbar-Grid.cshtml`, `AdminListToolbar-Contents.cshtml`, `AdminListToolbar-Contents-Grid.cshtml` |
| `AdminListSearch`  | `AdminListSearch__{Layout}`, `AdminListSearch__{Name}`, `AdminListSearch__{Name}__{Layout}` | `AdminListSearch-Grid.cshtml`, `AdminListSearch-Contents.cshtml`, `AdminListSearch-Contents-Grid.cshtml` |

`{Name}` is the name of the list, e.g. `Contents`, which each module publishes as a constant next to its default columns (`ContentsAdminList.Name`). The names are unique, so every alternate above targets a single list: a theme can restyle the search bar of the content items without touching any other list.

`{Layout}` is the layout rendering the list (`List`, `Table`, `Grid`, or a custom one), except for `AdminListActions`, where it is the layout of the row actions (`Buttons` or `Menu`) since that is what the shape renders.

The name and the layout reach these shapes on their own. The `AdminList` shape stamps them on everything it renders — the toolbar, the search bar, the pager, the page size selector and each row — so a row template that renders `AdminListActions` without naming a list still gets the alternates of the list it belongs to. A part that already names a list keeps its own, which is what a list rendered inside another one needs.

An alternate that names a list but no layout applies to **every** layout of that list, because it is more specific than the layout alternate: `AdminList-Users.cshtml` renders the users in `List`, `Table` and `Grid` alike, which effectively opts that list out of the layout setting. To change one mode only, name it: `AdminList-Users-Grid.cshtml`. The shape still carries the resolved layout, so a single template can also branch on `@Model.Layout`.

`AdminListCell` has no layout variant: cells only exist in the layouts with columns, and a column renders the same zones in both.

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

#### What the page knows

A provider often needs more than the name of the list to decide on a column: the content items list, for instance, is the same list whether it shows every item or only the blog posts. The page passes what it knows as the `data` argument of `GetColumnsAsync`, and it reaches the provider as `context.Data`:

```csharp
var data = new Dictionary<string, object>
{
    [ContentsAdminList.ContentTypesKey] = new[] { "BlogPost" },
};

var columns = await adminListService.GetColumnsAsync(ContentsAdminList.Name, ContentsAdminList.GetDefaultColumns(S), data, HttpContext.RequestAborted);
```

```csharp
public Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default)
{
    // Only add the column while the list is filtered by a type that has the field this column renders.
    if (context.ListName == ContentsAdminList.Name &&
        context.TryGetData<string[]>(ContentsAdminList.ContentTypesKey, out var contentTypes) &&
        contentTypes.Contains("BlogPost"))
    {
        context.Columns.Add(new AdminListColumn { Name = "Category", Position = "25", Title = S["Category"], Zones = ["Category"] });
    }

    return Task.CompletedTask;
}
```

`context.Data` is never null, and `TryGetData<T>` / `GetData<T>` fall back instead of throwing when a key is missing or holds another type, so a provider written for one list stays safe on every other one. The keys are up to the list: the content items list fills `ContentsAdminList.ContentTypesKey` and `ContentsAdminList.StereotypesKey` with the types and stereotypes from its route, and leaves them out when the listing is not filtered.

### The lists to target

A list is named by the constant its module publishes next to its default columns, e.g. `QueriesAdminList.Name`. The same name selects the columns a provider configures, the `AdminList__{Name}` alternate and the `AdminListCell__{Name}__{Column}` alternates. The pages that render one list per group, e.g. the features by category and the recipes by feature, give every group the same name.

| Name | Declared by | Module |
| --- | --- | --- |
| `AdminMenus` | `AdminMenusAdminList` | `OrchardCore.AdminMenu` |
| `AuditTrail` | `AuditTrailAdminList` | `OrchardCore.AuditTrail` |
| `BackgroundTasks` | `BackgroundTasksAdminList` | `OrchardCore.BackgroundTasks` |
| `ContentParts` | `ContentPartsAdminList` | `OrchardCore.ContentTypes` |
| `ContentTypes` | `ContentTypesAdminList` | `OrchardCore.ContentTypes` |
| `Contents` | `ContentsAdminList` | `OrchardCore.Contents` |
| `DeploymentPlans` | `DeploymentPlansAdminList` | `OrchardCore.Deployment` |
| `FeatureProfiles` | `FeatureProfilesAdminList` | `OrchardCore.Tenants` |
| `Features` | `FeaturesAdminList` | `OrchardCore.Features` |
| `Indexes` | `IndexingAdminList` | `OrchardCore.Indexing` |
| `Layers` | `LayersAdminList` | `OrchardCore.Layers` |
| `ListPartContents` | `ListPartContentsAdminList` | `OrchardCore.Lists` |
| `MediaProfiles` | `MediaProfilesAdminList` | `OrchardCore.Media` |
| `Notifications` | `NotificationsAdminList` | `OrchardCore.Notifications` |
| `OpenIdApplications` | `OpenIdApplicationsAdminList` | `OrchardCore.OpenId` |
| `OpenIdScopes` | `OpenIdScopesAdminList` | `OrchardCore.OpenId` |
| `Placements` | `PlacementsAdminList` | `OrchardCore.Placements` |
| `Queries` | `QueriesAdminList` | `OrchardCore.Queries` |
| `RateLimits` | `RateLimitsAdminList` | `OrchardCore.RateLimits` |
| `Recipes` | `RecipesAdminList` | `OrchardCore.Recipes` |
| `RemoteClients` | `RemoteClientsAdminList` | `OrchardCore.Deployment.Remote` |
| `RemoteInstances` | `RemoteInstancesAdminList` | `OrchardCore.Deployment.Remote` |
| `Roles` | `RolesAdminList` | `OrchardCore.Roles` |
| `Shortcodes` | `ShortcodesAdminList` | `OrchardCore.Shortcodes` |
| `SitemapCache` | `SitemapCacheAdminList` | `OrchardCore.Sitemaps` |
| `SitemapIndexes` | `SitemapIndexesAdminList` | `OrchardCore.Sitemaps` |
| `Sitemaps` | `SitemapsAdminList` | `OrchardCore.Sitemaps` |
| `Templates` | `TemplatesAdminList` | `OrchardCore.Templates` |
| `Tenants` | `TenantsAdminList` | `OrchardCore.Tenants` |
| `UrlRewriting` | `UrlRewritingAdminList` | `OrchardCore.UrlRewriting` |
| `Users` | `UsersAdminList` | `OrchardCore.Users` |
| `WorkflowInstances` | `WorkflowInstancesAdminList` | `OrchardCore.Workflows` |
| `WorkflowTypes` | `WorkflowTypesAdminList` | `OrchardCore.Workflows` |

A few admin pages are deliberately not lists and have no name: the media library, the widgets by zone of the layers page, the admin menu node tree, the dashboard, the CORS policies, the data localization table, the media cache and the themes gallery. They are edited or arranged rather than listed, so they keep their own presentation.

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

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
{
  "OrchardCore": {
    "Admin": {
      "AdminUrlPrefix": "YourCustomAdminUrl"
    }
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

## Admin List Layouts

Admin lists such as the content items list are rendered by the `AdminList` shape, which supports exactly two layouts, defined by the `AdminListLayout` enum:

| Layout  | Description                                                                                         |
| ------- | --------------------------------------------------------------------------------------------------- |
| `List`  | The default. The items are rendered in a vertical list, each item by its own `SummaryAdmin` shape. |
| `Grid`  | The items are rendered with one column per `AdminListColumn`, laid out with a CSS grid. The header and every row are subgrids of the list, so every cell shares the tracks computed from the column widths and the header always lines up with the data, while a row keeps a box of its own for a sortable list to drag. |

The `Grid` layout uses a CSS container query on the list itself: when the list is narrower than 48rem (whatever the viewport, the admin sidebar takes part of it), the headers disappear and a row takes the shape of a `List` row — the selection and the title share the first line with the actions at its end, and every other cell takes a line under them — so the page never scrolls horizontally. The cells keep a `data-title` attribute with the column title for themes that want to show it.

The layout is selected in **Configuration → Settings → Admin** under **List layout**, and stored in the nullable `AdminSettings.ListLayout` enum property. Select **Use configuration default** to clear the override and inherit `AdminListOptions.DefaultLayout`, which a site can set per tenant in `appsettings.json`:

```json
  "OrchardCore": {
    "Admin": {
      "List": {
        "DefaultLayout": "Grid",
        "DefaultActionsLayout": "Menu"
      }
    }
  }
```

`AdminListOptions` is the single place the shipped defaults are decided, so changing them for a site never means editing a template or a driver. The values are `List` and `Grid` for `DefaultLayout`, and `Buttons` or `Menu` for `DefaultActionsLayout`. Missing values default to `List` and `Buttons`. Both properties are enums; an invalid configured list or actions layout is an error. Templates can override these presentations but cannot register additional layouts.

### Letting a user choose

**Let users choose the layout of a list** (`AdminSettings.AllowUserListLayoutSelection`, or `AllowUserSelection` in the configuration) adds a List/Grid selector to every list beside the item count:

Clicking one reloads the page with `?layout=Grid`, and the choice is kept in a cookie **per list**, so a user can read the content items as a grid and the users as a list while everyone else sees the site default. The layout resolves from the most specific source to the least:

| Source | Wins when |
| --- | --- |
| `AdminListContext.Layout`, set by the page | always: the page can only be rendered one way, e.g. while its rows are reordered by dragging them |
| `?layout=` in the query string | the site lets a user choose |
| The cookie, for this list | the site lets a user choose and no layout was asked for |
| The site setting, then `AdminListOptions.DefaultLayout` | otherwise |

A query or cookie value other than `List` or `Grid` is ignored, so a stale cookie or a hand-written query string cannot select another template. Names are case-insensitive. Turning the setting off makes every list use the site default again, cookies and query strings included.

The pager and the page size selector keep the query string of the page they are on, so paging or changing the page size holds on to the layout.

Every list offers the selector itself. A page that renders several lists, e.g. the features, whose categories are one list each, or that would rather place the selector in a header of its own, turns it off on its lists with `ShowLayoutSelector = false` and creates an `AdminListLayoutSelector` shape:

```csharp
model.LayoutSelector = await shapeFactory.CreateAsync(AdminListConstants.LayoutSelectorShapeType, Arguments.From(new
{
    ListName = "Features",
}));

foreach (var group in groups)
{
    group.List = new AdminListContext<FeatureEntry>
    {
        Source = group.Features,
        ShowLayoutSelector = false,
    };
}
```

The selector resolves its current layout and URLs when displayed, and emits no markup when user selection is disabled. Grouped lists use the same list name so they share a preference.

A page that forces a layout sets `Layout = AdminListLayout.List` or `AdminListLayout.Grid` and turns the selector off as well, since it would offer a layout the page does not render.

The options follow Orchard Core's signal-backed options pattern. `AdminListOptionsConfiguration` layers the site settings on top of the configuration section, the module registers `AddSignalOptionsChangeTokenSource<AdminListOptions>()`, and the settings driver calls `IOptionsUpdateNotifier.RequestUpdate<AdminListOptions>()` when the choice changes. Consumers inject `IOptionsMonitor<AdminListOptions>` and read `CurrentValue`, so saving the settings takes effect without releasing the shell.

### How it works

The list resolves its layout after the view has declared columns and column providers have extended them.
It then renders the supplied model items once through their display manager, using `SummaryAdmin` for `List`
or `SummaryAdminGrid` for layouts with columns. Only the selected representation of each row is built.

Shared row shapes have a location for each display type:

```csharp
View("IndexProfile_Fields_SummaryAdmin", indexProfile)
    .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
    .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1");
```

The shape type and template are reused. In `List`, the row template renders `Content`; in `Grid`, the
`Name` column renders the row's `Name` zone directly. No column metadata or cell projection is involved.
Display types do not inherit placement rules: a `SummaryAdmin` location or placement rule does not
automatically apply to `SummaryAdminGrid`.

A shape declared only for `SummaryAdminGrid` is built only for column layouts:

```csharp
View("IndexProfile_Statistics", indexProfile)
    .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Statistics:1");
```

Controllers query the items and supply the other shapes. They do not build the list or row shapes.
An `AdminListContext<TModel>` carries a typed source collection and region shapes. Its generic argument
selects the display model type, so no `ItemType` assignment is needed:

```csharp
model.List = new AdminListContext<ContentItem>
{
    Source = pageOfContentItems,
    Header = header,
    Pager = pagerShape,
};
```

The generic context derives from `AdminListContext`, so the `context` tag attribute accepts either
version. Its `Source` is an `IEnumerable<TModel>`; use a base
class or interface as `TModel` to render derived items through that type's registered display manager,
including when the source is empty. The non-generic context still supports explicit `ItemType` or
collection-based inference.

Initialization runs during the `AdminList` displaying event, before layout-specific alternates are selected.
Row driver queries and errors therefore occur during view rendering, not in the controller.
Column providers run when needed for a column
layout or layout selection, including when the list has no rows. A fallback to `List`, or a forced `List`
layout for ordering, is resolved before the row builder runs.

The rendering view can declare columns and presentation with tag helpers. Add
`@addTagHelper *, OrchardCore.Admin.Abstractions` to the view or its `_ViewImports.cshtml`:

```cshtml
<admin-list name="Contents" context="Model.List" empty-message="@T["No results found."]">
    <admin-list-list item-css-class="list-group-item" rows-class="example-list-rows" />
    <admin-list-grid rows-class="example-grid-rows">
        <admin-list-column type="Select" />
        <admin-list-column name="Title" title="@T["Title"]" position="20" />
        <admin-list-column name="Status" title="@T["Status"]" position="30" width="auto" />
        <admin-list-column type="Actions" title="@T["Actions"]" />
    </admin-list-grid>
</admin-list>
```

The rendering view supplies the list's `name`, which identifies it for column providers, layout preferences
and alternates. `AdminListContext` does not carry this identity, so the same context can be rendered as
different named lists. Grouped pages such as Features and Recipes share a name between their groups;
their controllers and views reuse a constant when the controller also needs it for layout selection.

`admin-list` requires both `name` and `context`:

```cshtml
<admin-list name="Example" context="Model.List" show-select-all="false">
    <admin-list-grid>
        <admin-list-column name="Name" title="@T["Name"]" position="20" />
        <admin-list-column type="Actions" title="@T["Actions"]" />
    </admin-list-grid>
</admin-list>
```

Use `AdminListContext<TModel>` to select a registered base or interface display manager. The non-generic
context can set `ItemType` explicitly, or infer it from the source's unambiguous `IEnumerable<T>` interface,
including empty collections, without reading the first item. Every item must be non-null and assignable
to that type.

By default, `IAdminListItemDisplayManager<TModel>` delegates to `IDisplayManager<TModel>` with the current
update model. Register a closed `IAdminListItemDisplayManager<TModel>` to use another display pipeline.
The Contents module supplies an adapter for `ContentItem` that calls `IContentItemDisplayManager`.
ListPart uses its own typed entries and adapter to suppress selection and add drag-ordering handles.

Optional row attributes belong to the row display driver, not the controller or a grid-specific options
contract. For example, `FeatureEntryDisplayDriver` sets `data-filter-value` and the feature-state attributes
on `context.Shape` for both admin display types. Other model types do not need to provide these attributes.
Row CSS classes and attributes are preserved in both layouts.

The tag creates the shape and renders it through the standard display pipeline. It accepts `show-toolbar`,
`show-select-all`, `show-layout-selector`, `layout`, `bulk-actions`, `empty-message`, and `item-css-class`
(the latter applies to List rows). Omitted attributes preserve the context's settings.

`admin-list-list` and `admin-list-grid` configure their respective layouts; **neither selects or forces a layout**.
Both accept `rows-class` and `item-css-class`. For List, `item-css-class` overrides the common setting or default
row classes; for Grid, it adds classes alongside the required grid row class. `rows-class` merges with the
existing `RowsAttributes` class. Each layout may be declared once. These declarations become the shape's
`ListOptions` and `GridOptions`, both of type `AdminListLayoutOptions`.

Declare columns directly inside `admin-list`, or inside `admin-list-grid`. Grid columns are collected even when
the selected layout is List, so providers and the layout selector see the same available columns. Column attributes
are `name`, `title` (localized plain text), `position`, `width`, `css-class`, `alignment` (`Start`, `Center`, `End`),
`no-wrap`, and `zones` (comma- or space-separated zone names). Optional `type="Select"` and `type="Actions"` reuse
the built-in presets; other attributes override their defaults. Regular columns require a name, and duplicate
column names are rejected ignoring case. The declaration tags emit no HTML of their own.

The public list-building API is the context plus the `admin-list` tag. Supply raw `Source` models,
not prebuilt rows or row-building callbacks. Use `IAdminListItemDisplayManager<TModel>` when a model needs
a specialized display pipeline. Shape creation and row building are implementation details.
Built-in listing models expose their raw queried records eagerly, e.g. `ListContentsViewModel.ContentItems`
contains `ContentItem` records, not deferred summary shapes. Their `List` property is an `AdminListContext`.

| Property | Description |
| --- | --- |
| `Name` | Required shape identity, supplied by the tag's `name`, not by `AdminListContext`. Used by preferences, column providers and alternates. |
| `Source` | Raw model collection supplied by the context; rows are built only after resolving the display type. |
| `ItemType` | Optional registered display model type, otherwise inferred from the source's `IEnumerable<T>`. |
| `Rows` | Output row shapes built during display, never an input. Row classes and attributes are preserved in either layout. |
| `DisplayType` | Resolved row display type: `SummaryAdmin` for `List`, `SummaryAdminGrid` for column layouts. |
| `Columns` | View-defined `AdminListColumn` instances, extended by providers and sorted by position. |
| `Data` | Optional contextual data for column providers, e.g. the filtered content types. |
| `Layout` | Optional forced `AdminListLayout.List` or `AdminListLayout.Grid`; otherwise resolved from the user's preference or site settings. |
| `Headers` | Optional zone-holding shape with a zone per column for custom headers; otherwise the column title is used. |
| `Header` | Options editor shape whose `Summary` and `Actions` zones render above the rows. |
| `Toolbar` | Optional toolbar shape instead of `Header`. Otherwise a generic toolbar is built from the row count, pager, `BulkActions`, `ShowSelectAll` and `ToolbarActions`. |
| `ShowToolbar` | Set to `false` to omit the generic toolbar. |
| `ShowSelectAll` | Set to `false` to show the item count without selection. |
| `BulkActions` | Bulk action options for the generic toolbar. |
| `ToolbarActions` | Optional shape rendered at the end of the generic toolbar. |
| `Search` | Optional search-bar shape, e.g. `AdminListSearch` with `Name`, `Value`, `Placeholder`, `Id`, `SubmitName`, `Autofocus` and a `Filters` zone. |
| `Actions` | Optional page buttons rendered beside the search. |
| `LayoutSelector` | Built when user layout selection is enabled and columns are available. |
| `ShowLayoutSelector` | Set to `false` when the page places its own selector or forces a layout. |
| `CellRenderings` | Built for column layouts to select direct cell rendering or an overridden `AdminListCell` template. |
| `Pager` | Pager shape. |
| `PageSize` | Optional separately rendered page-size selector; set `ShowPageSizeSelector = false` on the pager to avoid rendering twice. |
| `RowsAttributes` | Attributes on the rows container, e.g. a sortable-list ID. |
| `ListOptions`, `GridOptions` | Optional `AdminListLayoutOptions` for each layout: `RowsCssClass` and `ItemCssClass`. Settings apply only to that layout; declaring them does not force it. |
| `ItemCssClass` | Row classes for the `List` layout. |
| `EmptyMessage` | Localized message when the list has no rows. |

A list without columns falls back to `List` and cannot offer a layout selector. A warning is logged when
column rendering was requested but no columns were declared. A list rendered only as `List` does not run column providers.

The shipped layouts render the search and page actions, toolbar, rows, pager and page-size selector.
Only an action bar carrying the search is sticky. The footer centres the pager and places the page-size selector at its end.
A template override can reorder or omit these regions.

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
        @await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))
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

```cshtml
<admin-list name="Rules" context="Model.List" rows-id="rewrite-rules-sortable-list">
    ...
</admin-list>
```

Two things a sortable list has to know:

- The rows are what a drag script picks up. In the `List` layout they are the items of a list group; in the `Grid` layout they are subgrids spanning the columns, so they have a box of their own while their cells stay on the tracks of the list. The element holding the rows is `display: contents` in the `Grid` layout, so a script must measure the rows, never that element.
- SortableJS `oldIndex` and `newIndex` count **every** sibling of the dragged element, including a toolbar rendered among the rows. Use `oldDraggableIndex` and `newDraggableIndex`, which count only the rows, so the indexes are the same in every layout.

### Overriding templates

The following alternates are available, from the least to the most specific:

| Shape              | Alternates                                                                           | Template examples                                                        |
| ------------------ | ------------------------------------------------------------------------------------ | ------------------------------------------------------------------------ |
| `AdminList`        | `AdminList__{Layout}`, `AdminList__{Name}`, `AdminList__{Name}__{Layout}`            | `AdminList-Grid.cshtml`, `AdminList-Contents.cshtml`, `AdminList-Contents-Grid.cshtml` |
| `AdminListCell`    | `AdminListCell__{Column}`, `AdminListCell__{Name}__{Column}`                         | `AdminListCell-Actions.cshtml`, `AdminListCell-Contents-Title.cshtml`    |
| `AdminListActions` | `AdminListActions__{Layout}`, `AdminListActions__{Name}`, `AdminListActions__{Name}__{Layout}` | `AdminListActions-Menu.cshtml`, `AdminListActions-Contents.cshtml`, `AdminListActions-Contents-Menu.cshtml` |
| `AdminListToolbar` | `AdminListToolbar__{Layout}`, `AdminListToolbar__{Name}`, `AdminListToolbar__{Name}__{Layout}` | `AdminListToolbar-Grid.cshtml`, `AdminListToolbar-Contents.cshtml`, `AdminListToolbar-Contents-Grid.cshtml` |
| `AdminListSearch`  | `AdminListSearch__{Layout}`, `AdminListSearch__{Name}`, `AdminListSearch__{Name}__{Layout}` | `AdminListSearch-Grid.cshtml`, `AdminListSearch-Contents.cshtml`, `AdminListSearch-Contents-Grid.cshtml` |

`{Name}` is the stable name of the list, e.g. `Contents`. The names are unique, so every alternate above targets a single list: a theme can restyle the search bar of the content items without touching any other list.

`{Layout}` is the layout rendering the list (`List` or `Grid`), except for `AdminListActions`, where it is the layout of the row actions (`Buttons` or `Menu`) since that is what the shape renders.

The name and the layout reach these shapes on their own. The `AdminList` shape stamps them on everything it renders — the toolbar, the search bar, the pager, the page size selector and each row — so a row template that renders `AdminListActions` without naming a list still gets the alternates of the list it belongs to. A part that already names a list keeps its own, which is what a list rendered inside another one needs.

An alternate that names a list but no layout applies to **every** layout of that list, because it is more specific than the layout alternate: `AdminList-Users.cshtml` renders the users in `List` and `Grid` alike, which effectively opts that list out of the layout setting. To change one mode only, name it: `AdminList-Users-Grid.cshtml`. The shape still carries the resolved layout, so a single template can also branch on `@Model.Layout`.

`AdminListCell` has no layout variant: cells only exist in the layouts with columns, and a column renders the same zones and cells in each of them.

The `Grid` layout only creates an `AdminListCell` shape for the cells a template overrides: `AdminListCell`, `AdminListCell-{Column}` or `AdminListCell-{Name}-{Column}`, from a theme or another module. The shape carries the `Row` and `Column`. The other cells render the row's column zone, or its `Actions` and `ActionsMenu` zones for the `Actions` column, without a shape for every row and column. The `AdminList` shape's `CellRenderings` property carries one `AdminListCellRendering` per column for template overrides to use.

### Column properties

| Property    | Description                                                                                                                                                                  |
| ----------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Name`      | The technical name, unique in the list ignoring case, used for the `AdminListCell__{Name}` alternates and the `admin-list-column-{name}` CSS class. It is the key of the column in `context.Columns`, so it is set before the column is added. |
| `Title`     | The localized header text. `null` for a column without a header, e.g. the selection checkbox. It also labels the cells when a narrow list hides the headers. |
| `Position`  | The position in the placement syntax (`10`, `25`, `35.5`, or `end` to stay last). Columns are sorted after all providers run, so features can insert columns independently. A column without a position goes after positioned columns. |
| `Zones`     | Optional row zones to render in order instead of the zone named after the column. |
| `Width`     | Any CSS width (`20%`, `12rem`), or `AdminListColumn.AutoWidth` (`auto`) to make the column as narrow as its content. Columns without a width share the remaining space.        |
| `Alignment` | `Start` (default), `Center` or `End`. Applied to the header and the cells.                                                                                                   |
| `NoWrap`    | Prevents the cell content from wrapping, e.g. dates and badges.                                                                                                              |
| `CssClass`  | Extra CSS classes added to the header and the cells.                                                                                                                         |

```csharp
context.Columns.Add(new AdminListColumn
{
    Name = "Modified",
    Position = "50",
    Title = S["Last modified"],
    Width = "18%",
    NoWrap = true,
});
```

Most lists share their first and last columns, so `AdminListColumns` declares them: `AdminListColumns.Select()` renders the `Select` zone at position `10` for a list with bulk actions, and `AdminListColumns.Actions(S["Actions"])` renders the `Actions` and `ActionsMenu` zones at position `end`, aligned to the end.

### Row actions

The `Actions` and `ActionsMenu` zones of a row are rendered by the `AdminListActions` shape in the layout selected under **Configuration → Settings → Admin → Actions layout**. `AdminSettings.ActionsLayout` is a nullable `AdminListActionsLayout` enum. Select **Use configuration default** to clear the override and inherit `AdminListOptions.DefaultActionsLayout`. Exactly two layouts are supported:

| Layout    | Description                                                                                                   |
| --------- | ------------------------------------------------------------------------------------------------------------- |
| `Buttons` | The default. The shapes of the `Actions` zone as buttons, followed by an "Actions" dropdown for `ActionsMenu`. |
| `Menu`    | A single dropdown opened by an ellipsis button, holding the `Actions` shapes (restyled as menu items) and the `ActionsMenu` shapes. |

Row templates render it with `@await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))`, which creates the shape with its properties rather than through the dynamic `New`, since it is rendered once per row. The `Grid` layout renders it in the `Actions` column, so the actions layout applies to every list layout. The alternates are `AdminListActions__{Layout}`, `AdminListActions__{ListName}` and `AdminListActions__{ListName}__{Layout}`. Themes can override the Buttons or Menu templates, including per list, but option shapes do not register additional choices. For example, override the Buttons presentation to render icon buttons without adding a third selectable mode.

### Edit page action bar

The submit buttons of admin edit pages, such as **Publish**, **Save** and **Cancel**, are rendered in a bar fixed to the bottom of the viewport, so they are always reachable without scrolling. Views opt in by wrapping their buttons in an element with the `action-bar-bottom` class, typically the last child of the form:

```cshtml
<div class="action-bar-bottom">
    <button class="btn btn-primary save" type="submit">@T["Save"]</button>
    <a class="btn btn-danger action-bar-secondary" asp-action="Delete">@T["Delete"]</a>
    <a class="btn btn-secondary cancel" asp-action="Index">@T["Cancel"]</a>
</div>
```

Mark every action other than the main ones with the `action-bar-secondary` class. A secondary action can be a single button or a whole control group, for example the date input and the button of **Publish Later**. When the **Actions layout** setting resolves to `Menu`, the admin theme moves the secondary actions into a single menu opened by an ellipsis button, and keeps only the main actions visible. On small screens the secondary actions are always moved into this menu, whatever the setting. The page content is padded so the bar never hides the end of the form.

#### Remembering the last used action

A split button offering an alternative action, such as **Save** with **Save and continue** in its dropdown, can remember which of the two the user chose last. When the alternative was used, the next page with the same group shows it as the main button, and moves the default action into the dropdown. Add these attributes to the `btn-group` element, which contains one submit button and one `dropdown-item` submit button:

| Attribute | Description |
| --- | --- |
| `data-action-group` | The key under which the last used action is stored. Pages sharing a key share the choice, such as the create and edit pages of a template. |
| `data-action-default` | The `value` of the default submit button. |
| `data-label-default` | The text of the default action. |
| `data-action-alt` | The `value` of the alternative submit button. |
| `data-label-alt` | The text of the alternative action shown on the main button, such as **Save and continue**. |

```cshtml
<div class="btn-group"
     role="group"
     aria-label="@T["Save actions"]"
     data-action-group="oc-template-save-action"
     data-action-default="Save"
     data-label-default="@T["Save"]"
     data-action-alt="SaveAndContinue"
     data-label-alt="@T["Save and continue"]">
    <button type="submit" name="submit" value="Save" class="btn btn-primary save">@T["Save"]</button>
    <button type="button" class="btn btn-primary dropdown-toggle dropdown-toggle-split" data-bs-toggle="dropdown" aria-expanded="false">
        <span class="visually-hidden">@T["Toggle Dropdown"]</span>
    </button>
    <div class="dropdown-menu">
        <button type="submit" name="submit" value="SaveAndContinue" class="dropdown-item save-continue">@T["and continue"]</button>
    </div>
</div>
```

The choice is stored per tenant in the browser's admin preferences. Both buttons must have an explicit `value`, since they swap their values.
### Declaring the columns of a list

Declare the columns in the view that renders the list. Their names are unique, ignoring case.
`Position` sorts columns independently of provider order; `AdminListColumns.Select()` and
`AdminListColumns.Actions()` supply the shared selection and actions columns.

```cshtml
@using OrchardCore.Admin
@using OrchardCore.Admin.Models
<admin-list name="Contents" context="Model.List" empty-message="@T["No results found."]">
    <admin-list-list item-css-class="list-group-item" />
    <admin-list-grid>
        <admin-list-column type="Select" />
        <admin-list-column name="Title" title="@T["Title"]" position="20" />
        <admin-list-column name="Status" title="@T["Status"]" position="30" width="auto" />
        <admin-list-column type="Actions" title="@T["Actions"]" />
    </admin-list-grid>
</admin-list>
```

#### Placing cell shapes

Use the row's existing display driver, not a column-specific driver:

```csharp
public override IDisplayResult Display(ContentItem contentItem, BuildDisplayContext context)
{
    return Initialize<CultureViewModel>("ContentCulture_SummaryAdmin",
        model => model.Culture = contentItem.As<LocalizationPart>()?.Culture)
        .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
        .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Culture:10");
}
```

The normal placement pipeline applies `placement.json` and `IShapePlacementProvider` rules before the shape is built.
A theme can move one shape to another column, hide it, change its template, or add wrappers and alternates:

```json
{
  "ContentCulture_SummaryAdmin": [
    { "displayType": "SummaryAdminGrid", "place": "Status:10" }
  ]
}
```

Use `place: "-"` to hide a shape. Moving a shape to another column means moving it to that row zone.
A zone not rendered by any declared column is omitted; removing a column therefore hides its cells.
The optional `Zones` property renders several zones in a cell instead of the zone named after the column.
Placement for `SummaryAdminGrid` is independent of `SummaryAdmin`, so list rendering remains unchanged.

The `Actions` column renders the row's `Actions` and `ActionsMenu` zones through `AdminListActions`,
preserving the configured Buttons/Menu mode. An action moved to another zone is no longer in those zones
and is not duplicated. Overridden `AdminListCell` templates receive the row as `Row` and its definition as `Column`.

#### Extending columns

Implement `IAdminListColumnsProvider` to add, remove or change the view's columns, and register it for a
target list name or globally for all lists. Providers see the view-defined columns before sorting,
including when there are no rows:

```csharp
public sealed class CultureColumnsProvider : IAdminListColumnsProvider
{
    private readonly IStringLocalizer S;

    public CultureColumnsProvider(IStringLocalizer<CultureColumnsProvider> localizer)
    {
        S = localizer;
    }

    public Task PopulateAsync(AdminListColumnsContext context)
    {
        context.Columns.Add(new AdminListColumn
        {
            Name = "Culture",
            Position = "25",
            Title = S["Culture"],
            Width = AdminListColumn.AutoWidth,
        });

        return Task.CompletedTask;
    }
}
```

Register it with `services.AddAdminListColumnsProvider<CultureColumnsProvider>(ContentsAdminList.Name)`.
Multiple providers can target the same list, and the same provider can be registered for different lists.
The helper prevents duplicate provider registrations for the same list. Named providers are scoped keyed services,
resolved with `GetKeyedServices<IAdminListColumnsProvider>(new AdminListColumnsProviderKey(listName))`.
The record key keeps them distinct from services registered with plain string keys. List names are
case-sensitive; only named providers registered for the rendering's exact name are constructed and invoked.

Omit the list name to register a scoped global provider:

```csharp
services.AddAdminListColumnsProvider<SiteColumnsProvider>();
```

Global providers use unkeyed registrations and run for every list that builds columns. They must tolerate
different column sets: check `context.Find(name)` before modifying an optional column. Repeated registration
of the same global provider is ignored. Global and named registrations are separate scopes of applicability;
registering a provider both globally and for a list invokes it twice for that list.

Global providers run first, then the matching named providers, so a list can refine a global convention.
Registration order is preserved within each group, and columns are sorted after both groups finish.
Within each group, providers run in feature dependency order; depend on the owning
feature when changing its columns. Duplicate column names throw; provider errors are logged and other providers continue.
`context.Find(name)` and `context.Remove(name)` access existing columns.

#### Contextual data

The page passes data through `AdminListContext.Data`:

```csharp
var list = new AdminListContext<ContentItem> { Source = contentItems };
list.Data[ContentsAdminList.ContentTypesKey] = new[] { "BlogPost" };
model.List = list;
```

Providers read `context.Data`, `TryGetData<T>()` or `GetData<T>()`. Missing keys or mismatched types return
the supplied default. The content items list supplies `ContentsAdminList.ContentTypesKey` and
`ContentsAdminList.StereotypesKey` when its route is filtered by types or stereotypes.

### The lists to target

A list has a stable name declared in its rendering view, e.g. `name="Queries"`. Use the string value directly when it is used only once in its module; keep a constant for reused names or shared contracts in an abstractions project. The lists other modules are most likely to extend publish theirs, `ContentsAdminList.Name` and `UsersAdminList.Name`, so a column provider targets them without repeating the string. The same name tells column providers which list they extend, and selects the layout a user picked for the list, the `AdminList__{Name}` alternate and the `AdminListCell__{Name}__{Column}` alternates. The pages that render one list per group, e.g. the features by category and the recipes by feature, give every group the same name.

The columns of a list are declared in its rendering view.

| Name | Items | Module |
| --- | --- | --- |
| `AdminMenus` | `AdminMenuEntry` | `OrchardCore.AdminMenu` |
| `AuditTrail` | `AuditTrailEvent` | `OrchardCore.AuditTrail` |
| `BackgroundTasks` | `BackgroundTaskEntry` | `OrchardCore.BackgroundTasks` |
| `ContentParts` | `ContentPartEntry` | `OrchardCore.ContentTypes` |
| `ContentTypes` | `ContentTypeEntry` | `OrchardCore.ContentTypes` |
| `Contents` | `ContentItem` | `OrchardCore.Contents` |
| `DeploymentPlans` | `DeploymentPlanEntry` | `OrchardCore.Deployment` |
| `FeatureProfiles` | `FeatureProfileEntry` | `OrchardCore.Tenants` |
| `Features` | `FeatureEntry` | `OrchardCore.Features` |
| `Indexes` | `IndexProfile` | `OrchardCore.Indexing` |
| `Layers` | `Layer` | `OrchardCore.Layers` |
| `ListPartContents` | `ListPartAdminListEntry` | `OrchardCore.Lists` |
| `MediaProfiles` | `MediaProfileEntry` | `OrchardCore.Media` |
| `Notifications` | `Notification` | `OrchardCore.Notifications` |
| `OpenIdApplications` | `OpenIdApplicationEntry` | `OrchardCore.OpenId` |
| `OpenIdScopes` | `OpenIdScopeEntry` | `OrchardCore.OpenId` |
| `Placements` | `ShapePlacement` | `OrchardCore.Placements` |
| `Queries` | `Query` | `OrchardCore.Queries` |
| `RateLimits` | `RateLimitPolicy` | `OrchardCore.RateLimits` |
| `Recipes` | `RecipeEntry` | `OrchardCore.Recipes` |
| `RemoteClients` | `RemoteClient` | `OrchardCore.Deployment.Remote` |
| `RemoteInstances` | `RemoteInstance` | `OrchardCore.Deployment.Remote` |
| `Roles` | `RoleEntry` | `OrchardCore.Roles` |
| `Shortcodes` | `ShortcodeTemplateEntry` | `OrchardCore.Shortcodes` |
| `SitemapCache` | `SitemapCacheEntry` | `OrchardCore.Sitemaps` |
| `SitemapIndexes` | `SitemapIndexListEntry` | `OrchardCore.Sitemaps` |
| `Sitemaps` | `SitemapListEntry` | `OrchardCore.Sitemaps` |
| `Templates` | `TemplateEntry` | `OrchardCore.Templates` |
| `TemplateSources` | `SourceTemplateEntry` | `OrchardCore.Templates` |
| `TemplateAlternates` | `TemplateAlternateEntry` | `OrchardCore.Templates` |
| `Tenants` | `ShellSettingsEntry` | `OrchardCore.Tenants` |
| `UrlRewriting` | `RewriteRule` | `OrchardCore.UrlRewriting` |
| `Users` | `User` | `OrchardCore.Users` |
| `WorkflowInstances` | `WorkflowEntry` | `OrchardCore.Workflows` |
| `WorkflowTypes` | `WorkflowTypeEntry` | `OrchardCore.Workflows` |

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

## Navigation menu behavior

The **Navigation menu behavior** option of the admin settings (**Configuration** > **Settings** > **Admin**) decides how the sections of the admin menu open and close:

| Behavior | What it does |
|----------|--------------|
| `Focused` (default) | Only the section holding the current page is open when a page loads, and opening a section closes the other sections at the same level. |
| `Persistent` | Every section keeps the state the user left it in, from page to page, so that several sections can be open at once. What is open is remembered per user in the browser. |

With either behavior, the section holding the current page is always open, so the page being edited can always be found in the menu.

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
        "DisplayTitlesInTopbar": true,
        "MenuBehavior": "Focused"
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
| `MenuBehavior`          | String  | How the sections of the admin menu open and close: `Focused` (default) or `Persistent`. See [Navigation menu behavior](#navigation-menu-behavior). |

# Navigation (`OrchardCore.Navigation`)

## Purpose

Provides the `Navigation`, `Breadcrumb`, `Pager` and `PagerSlim` shapes.

## Theming

Navigation can be themed by adding the appropriate partial view files to your theme's views folder.
A good example can be found in the [`TheAdmin` theme project](https://github.com/OrchardCMS/OrchardCore/tree/dev/src/OrchardCore.Themes/TheAdmin).

That theme creates the standard, vertical navigation menu that is found on the admin dashboard of any OrchardCore application.
The `TheAdmin` theme provides the following alternates to the default ones provided in the `Navigation` module:  

- `Navigation-admin.cshtml`  
- `NavigationItem-admin.cshtml`  
- `NavigationItemLink-admin.cshtml`  

The theme developer has full control over how and where navigation is displayed in their OrchardCore application.

### Pager

This is a multi-purpose pagination component that renders links to specific page numbers.
It can optionally render _First_ and _Last_ links.

| Parameter        | Type     | Description                                                                    |
|------------------|----------|--------------------------------------------------------------------------------|
| `Page`           | `int`    | Active page number.                                                            |
| `PageSize`       | `int`    | Number of items per page.                                                      |
| `TotalItemCount` | `double` | Total number of items (used to calculate the number of the last page).         |
| `Quantity`       | `int?`   | Number of pages to show, 7 if not specified.                                   |
| `FirstText`      | `object` | Text of the "First" link, default: `S["<<"]` .                                 |
| `PreviousText`   | `object` | Text of the "Previous" link, default: `S["<"]`.                                |
| `NextText`       | `object` | Text of the "Next" link, default: `S[">"]` .                                   |
| `LastText`       | `object` | Text of the "Last" link, default: `S[">>"]`.                                   |
| `GapText`        | `object` | Text of the "Gap" element, default: `S["..."]`.                                |
| `PagerId`        | `string` | An identifier for the pager. Used to create alternate like `Pager__[PagerId]`. |
| `ShowNext`       | `bool`   | If true, the "Next" link is always displayed.                                  |

Properties inherited from the `List` shape:

| Parameter        | Type                         | Description                                               |
|------------------|------------------------------|-----------------------------------------------------------|
| `ItemTagName`    | `string`                     | The HTML tag used for the pages, default: `li`.           |
| `ItemClasses`    | `List<string>`               | Classes that are assigned to the pages, default: _none_.  |
| `ItemAttributes` | `Dictionary<string, string>` | Attributes that are assigned to the pages.                |
| `FirstClass`     | `string`                     | The HTML class used for the first page, default: `first`. |
| `LastClass`      | `string`                     | The HTML tag used for last page, default: `last`.         |

Properties inherited from the base Shape class:

| Parameter    | Type                         | Description                                         |
|--------------|------------------------------|-----------------------------------------------------|
| `Id`         | `string`                     | The HTML id used for the pager, default: _none_.    |
| `TagName`    | `string`                     | The HTML tag used for the pager, default: `ul`.     |
| `Attributes` | `Dictionary<string, string>` | Attributes that are assigned to the main container. |
| `Classes`    | `Dictionary<string, string>` | CSS classes to add to the main Tag element.         |

The `PagerId` property is used to create templates for specific instances. For instance, assigning
the value `MainBlog` to `PagerId` and then rendering the pager will look for a template named
`Pager-MainBlog.cshtml`.

A pager can be further customized by defining templates for the following shapes:

- `Pager_Gap`
- `Pager_First`
- `Pager_Previous`
- `Pager_Next`
- `Pager_Last`
- `Pager_CurrentPage`

Each of these shapes are ultimately morphed into `Pager_Link`.
Alternates for each of these shapes are created using the `PagerId` like `Pager_Previous__[PagerId]` which
would in turn look for the template `Pager-MainBlog.Previous.cshtml`.

### `PagerSlim`

This shape renders a pager that is comprised of two links: _Previous_ and _Next_.

| Parameter       | Type                         | Description                                                                     |
|-----------------|------------------------------|---------------------------------------------------------------------------------|
| `PreviousClass` | `string`                     | The HTML class used for the _Previous_ link, default: _none_.                   |
| `NextClass`     | `string`                     | The HTML class used for the _Next_ link, default: _none_.                       |
| `PreviousText`  | `object`                     | Text of the "Previous" link, default: `S["<"]`.                                 |
| `NextText`      | `object`                     | Text of the "Next" link, default: `S[">"]`.                                     |
| `UrlParams`     | `Dictionary<string, string>` | QueryString params to pass to the pager. Parameter name and value in that order |

Properties inherited from the `List` shape:

| Parameter        | Type                         | Description                                               |
|------------------|------------------------------|-----------------------------------------------------------|
| `ItemTagName`    | `string`                     | The HTML tag used for the pages, default: `li`.           |
| `ItemClasses`    | `List<string>`               | Classes that are assigned to the pages, default: _none_.  |
| `ItemAttributes` | `Dictionary<string, string>` | Attributes that are assigned to the pages.                |
| `FirstClass`     | `string`                     | The HTML class used for the first page, default: `first`. |
| `LastClass`      | `string`                     | The HTML tag used for last page, default: `last`.         |

Properties inherited from the base Shape class:

| Parameter    | Type                         | Description                                         |
|--------------|------------------------------|-----------------------------------------------------|
| `Id`         | `string`                     | The HTML id used for the pager, default: _none_.    |
| `TagName`    | `string`                     | The HTML tag used for the pager, default: `ul`.     |
| `Attributes` | `Dictionary<string, string>` | Attributes that are assigned to the main container. |
| `Classes`    | `Dictionary<string, string>` | CSS classes to add to the main Tag element.         |

A slim pager can be further customized by defining templates for the following shapes:

- `Pager_Previous`
- `Pager_Next`

Examples of Liquid alternates or templates for `Pager_Next` and `Pager_Previous`:

```liquid
{% shape_clear_alternates Model %}
{% shape_type Model "Pager_Link" %}
{% shape_add_classes Model "page-link" %}
{{ Model | shape_render }}
```

Each of these shapes are ultimately morphed into `Pager_Link`.
Alternates for each of these shapes are created using the `PagerId` like `Pager_Previous` `[PagerId]` which
would in turn look for the template `Pager-MainBlog.Previous.cshtml`.

## SEO

In order to block search engines from crawling all your pagers links, it is possible to override the Pager anchors "rel" attributes with "no-follow". To achieve this, you can simply do this:

=== "Liquid"

    ``` liquid
    {% shape_pager Model.Pager attributes: "{\"rel\": \"no-follow\"}" %}
    ```

=== "C#"

    ``` html
    Model.Pager.Attributes["rel"] = "no-follow;
    @await DisplayAsync(Model.Pager)
    ```

## Breadcrumbs

The `Breadcrumb` shape renders the trail of pages leading to the page being displayed, e.g. _Dashboard › Manage Content › Edit Article_.

It is meant to stand in for the title of an admin screen rather than to sit beside it: the last node of the trail is the page itself, it is rendered inside a heading tag so that it keeps the look and the semantics of the `<h1>` it replaces, and its text is registered as a segment of the `<title>` of the page. A screen that renders a breadcrumb therefore stops rendering a title of its own.

A trail is identified by a **name**, and it is built by every `IBreadcrumbProvider` registered on the tenant. That is what makes it extensible: a module adds a node to, or removes a node from, a trail described by another module without either of them knowing about the other.

The shapes and the services live in the `OrchardCore.Navigation` module, which is always enabled, so a module that renders a breadcrumb does not need to depend on a feature.

### Adding a breadcrumb to your own screen

Four steps, of which only the last one touches the view.

#### 1. Reference the tag helpers

The `<breadcrumb>` tag helper lives in `OrchardCore.Navigation.Core`. Reference the project (or the `OrchardCore.Navigation.Core` package) from your module, then add it to the `Views/_ViewImports.cshtml` of your module:

```html
@addTagHelper *, OrchardCore.Navigation.Core
```

#### 2. Name the trail

Every screen that renders a breadcrumb publishes the name of its trail, and the keys of the contextual data the trail carries, so that another module can react to them. Put them on a constants class of your module's abstractions, next to your permissions. The class name ends in `Constants`; reuse your module's existing `{Module}Constants` when it has one, and only add a new file when it does not:

```csharp
namespace My.Module;

public static class MyConstants
{
    public const string List = "Records";

    public const string Edit = "RecordsEdit";

    public const string RecordKey = "Record";
}
```

When a `{Module}Constants` name would collide with a framework type (for instance `Microsoft.AspNetCore.Cors.Infrastructure.CorsConstants`), name the class `{Module}BreadcrumbConstants` instead.

Name a trail after the screen that renders it, in PascalCase and without separators, because the name also becomes the shape alternate of the trail. A screen that also publishes a name for something else of its own, such as the name an admin list is known by, reuses that name for its breadcrumb, so that one screen has one name rather than two: the content items list is `Contents` for both.

#### 3. Describe the trail

Implement `IBreadcrumbProvider`, or inherit from `NamedBreadcrumbProvider` when the provider only contributes to a single trail:

```csharp
public sealed class MyBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public MyBreadcrumbProvider(IStringLocalizer<MyBreadcrumbProvider> localizer)
        : base(MyConstants.Edit)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        if (!builder.TryGetData<Record>(MyConstants.RecordKey, out var record))
        {
            return ValueTask.CompletedTask;
        }

        builder.Add(S["Records"], item => item
            .Id("Records")
            .Action(nameof(RecordController.Index), "Record", new { area = "My.Module" })
            .Permission(MyPermissions.ManageRecords));

        builder.Add(S["Edit {0}", record.Name], item => item.Id("Record"));

        return ValueTask.CompletedTask;
    }
}
```

Register it in the `Startup` of your module:

```csharp
services.AddBreadcrumbProvider<MyBreadcrumbProvider>();
```

The provider is where the whole trail is decided, including the node of the page itself. Nothing about the trail lives in the view, which is what lets another module change it.

#### 4. Render it in the view

Put the tag helper in the `Title` zone of the view, in place of the `<h1>` it replaces, and pass the contextual data the providers read back:

```html
<zone Name="Title">
    <breadcrumb name="@MyConstants.Edit" data="@(new { Record = record })" />
</zone>
```

That is the whole change to the view. There is no `@RenderTitleSegments(...)` call to keep: the tag helper registers the text of the current node as a segment of the page title on its own.

Rendering the trail in the `Title` zone is what makes it behave like the title it replaces, including honouring the **Display titles in the top bar** admin setting. A trail that is meant to sit above the page rather than to replace its title goes in the `Breadcrumbs` zone instead, and is rendered with `heading=""` so that it carries no heading. Outside the admin that is already the default, because a front end page has a heading of its own and a second one would be wrong.

### The `<breadcrumb>` tag helper

| Attribute      | Type     | Description                                                                                                                                                                   |
|----------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `name`         | `string` | Required. The name of the trail to render, e.g. `ContentsEdit`.                                                                                                                |
| `data`         | `object` | The contextual data of the page. Each property becomes an entry of `BreadcrumbBuilder.Data`, which every provider of the trail can read back.                                   |
| `heading`      | `string` | The html tag wrapping the text of the current node, so the breadcrumb can stand in for the title of the page. Defaults to `h1` on the admin and to no heading elsewhere. Set it to an empty value to render none. |
| `page-title`   | `bool`   | Whether the text of the current node is registered as a segment of the `<title>` of the page. Defaults to `true`.                                                               |
| `display-type` | `string` | The display type of the trail, which becomes an alternate of every shape it renders. Defaults to `DetailAdmin` on a request to the admin, and to `Detail` everywhere else.      |

Nothing is rendered when no provider contributes a node to the trail, so a screen keeps working when the feature owning its provider is disabled.

### Describing the nodes

`BreadcrumbBuilder` carries the `Name` of the trail being built, the `Data` the page provided, and the nodes added so far:

| Member                       | Description                                                                                          |
|------------------------------|--------------------------------------------------------------------------------------------------|
| `Name`                       | The name of the trail being built. Check it before adding anything, unless you inherit from `NamedBreadcrumbProvider`. |
| `Data`                       | The contextual data of the page, keyed by the property names of the `data` attribute.               |
| `TryGetData<T>` / `GetData<T>` | Reads one contextual value, when it exists and is of the expected type.                           |
| `Add(text, [position], [item => …])` | Adds a node.                                                                                |
| `Remove(predicate)`          | Removes every node matching the predicate, whichever provider added it.                             |
| `Items`                      | The nodes added so far. They are mutable, so a provider can also change a node instead of replacing it. |

A node is configured through `BreadcrumbItemBuilder`:

| Method                             | Description                                                                                                                    |
|------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| `Text(string)`                     | The text to display for the node. It is also the first argument of `BreadcrumbBuilder.Add()`.                                  |
| `Id(string)`                       | The identifier of the node, used to build its shape alternates, and to let another provider find it.                           |
| `Url(string)` / `Action(…)`        | What the node links to. A node with neither is rendered as plain text.                                                         |
| `Position(string)`                 | The position of the node among the nodes of the trail, using the same syntax as [placement](../Placement/README.md#position-format), e.g. `5`, `after`, `start`, `end`. It is also the second argument of `BreadcrumbBuilder.Add()`. |
| `Permission(…)` / `Permissions(…)` | The permissions the user must all have for the node to be rendered as a link.                                                  |
| `Resource(object)`                 | The resource the permissions of the node are evaluated against.                                                                |
| `AddClass(string)`                 | A css class to render with the node.                                                                                           |

`IBreadcrumbManager` then orders the nodes by their position, computes the url of each of them, and marks the last one as the current node:

- The **current node never links to itself**, whatever url its provider gave it.
- A node the user is **not authorized** to reach is rendered as plain text rather than removed, so that the trail stays complete instead of losing a step.
- A provider that throws is **ignored**, and the nodes of the other providers are still rendered.

### Adding a node to a trail you don't own

Because every provider sees every trail, a module extends a trail described by another module by reacting to its name and positioning its node:

```csharp
public sealed class LocalizationBreadcrumbProvider : NamedBreadcrumbProvider
{
    public LocalizationBreadcrumbProvider()
        : base(ContentsConstants.Edit)
    {
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        // The nodes of the trail have no position by default, so this one lands after them.
        builder.Add("Translations", position: "after", item => item.Id("Translations"));

        return ValueTask.CompletedTask;
    }
}
```

The same builder also removes a node another provider added, which is how a trail is shortened:

```csharp
builder.Remove(item => item.Id == "Contents");
```

The `OrchardCore.AdminDashboard` feature does this for real, and for every trail rather than for one of them. `DashboardBreadcrumbProvider` adds the dashboard at the `start` position, the sentinel that sorts before every other position, so the node leads the trail whatever positions its other nodes use:

```csharp
public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
{
    var httpContext = _httpContextAccessor.HttpContext;

    // The dashboard is the root of the admin only. A trail rendered by a front end theme doesn't lead to it.
    if (httpContext is null || !AdminAttribute.IsApplied(httpContext))
    {
        return ValueTask.CompletedTask;
    }

    builder.Add(S["Dashboard"], "start", item => item
        .Id("Dashboard")
        .Url("~/" + _adminOptions.AdminUrlPrefix)
        .Permission(Permissions.AccessAdminDashboard));

    return ValueTask.CompletedTask;
}
```

`Manage Content › Edit Article` therefore becomes `Dashboard › Manage Content › Edit Article` when the feature is enabled, and goes back to what it was when it is disabled, without the content management screens knowing that the dashboard exists.

### Building a trail from code

A display driver, or any other code that needs the shape rather than the tag helper, builds it in two calls:

```csharp
var items = await _breadcrumbManager.BuildBreadcrumbAsync(ContentsConstants.Edit, ViewContext, new Dictionary<string, object>
{
    { ContentsConstants.ContentItemKey, contentItem },
});

var shape = await _shapeFactory.BreadcrumbAsync(ContentsConstants.Edit, items, heading: "h1");
```

`BuildBreadcrumbAsync` returns the ordered nodes, so it is also how code reads a trail without rendering it.

### Theming

The trail renders as the `Breadcrumb` shape, with one `BreadcrumbItem` shape per node. Both carry alternates built from the name of the trail, so a single screen can be templated on its own:

| Shape            | Alternates (least to most specific)                                                                                                                                                                                                  |
|------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Breadcrumb`     | `Breadcrumb__[Name]`, `Breadcrumb_[DisplayType]`, `Breadcrumb_[DisplayType]__[Name]`, e.g. `Breadcrumb-ContentsEdit.cshtml`, `Breadcrumb.DetailAdmin.cshtml`, `Breadcrumb-ContentsEdit.DetailAdmin.cshtml`                          |
| `BreadcrumbItem` | `BreadcrumbItem__[Name]`, `BreadcrumbItem__[Id]`, `BreadcrumbItem__[Name]__[Id]`, then the same three prefixed with the display type, e.g. `BreadcrumbItem-ContentsEdit-ContentItem.DetailAdmin.cshtml`                             |

The `BreadcrumbItem` shape carries the following properties:

| Property    | Type             | Description                                                                                    |
|-------------|------------------|------------------------------------------------------------------------------------------------|
| `Name`      | `string`         | The name of the trail the node belongs to.                                                      |
| `Item`      | `BreadcrumbItem` | The node being rendered, with its classes, permissions and route values.                        |
| `Text`      | `string`         | The text to display for the node.                                                               |
| `Href`      | `string`         | The url the node links to, or `null` when the node is not a link.                               |
| `IsCurrent` | `bool`           | Whether the node is the page being rendered.                                                    |
| `Level`     | `int`            | The zero based index of the node in the trail.                                                  |
| `Heading`   | `string`         | The html tag wrapping the text of the current node, e.g. `h1`. It is `null` on the other nodes. |

A trail rendered on the admin and a trail rendered by a front end theme are the same shape, so they are told apart by their **display type**, the way the rest of the display system tells those contexts apart. The tag helper sets it to `DetailAdmin` on a request to the admin and to `Detail` everywhere else, and the `display-type` attribute overrides it.

The module ships one template for both, because the markup of a trail does not differ between them: what differs is the heading, and the tag helper already decides that per context. The display type is there so that a theme can diverge without dragging the other context along: templating `Breadcrumb.DetailAdmin.cshtml` restyles the admin and leaves the front end on the default, and `Breadcrumb.Detail.cshtml` does the opposite.

The `Breadcrumb` shape carries the `oc-breadcrumb` class, alongside a class built from the name of the trail, e.g. `breadcrumb-contents-edit`, which is how `TheAdmin` styles it. The markup is a plain [Bootstrap breadcrumb](https://getbootstrap.com/docs/5.3/components/breadcrumb/), so a theme restyles it with the `--bs-breadcrumb-*` custom properties.

### Content management trails

The content management screens describe the following trails, whose names and data keys are exposed by `OrchardCore.Contents.ContentsConstants`:

| Name              | Rendered by                    | Contextual data                                                                         |
|-------------------|--------------------------------|-----------------------------------------------------------------------------------------|
| `Contents`        | The content items list         | `ContentType`, the name of the content type the list is filtered on, when there is one. |
| `ContentsCreate`  | The content item creation form | `ContentItem`, the content item being created.                                          |
| `ContentsEdit`    | The content item edition form  | `ContentItem`, the content item being edited.                                           |

## Extending Navigation

Navigation can be extended, through code, by implementing `INavigationProvider` and registering it in the extending module (or theme) `Startup.cs` file.

Below is a sample implementation of an `INavigationProvider` used to extend the "main" navigation section of the site.

```csharp
public sealed class MainMenu : INavigationProvider
{
    internal readonly IStringLocalizer S;

    public MainMenu(IStringLocalizer<MainMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigation(string name, NavigationBuilder builder)
    {
        //Only interact with the "main" navigation menu here.
        if (!String.Equals(name, "main", StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Notifications"], S["Notifications"], notifications => notifications
                .Action("Index", "Template", new { area = "CRT.Client.OrchardModules.CommunicationTemplates", groupId = "1" })
                .LocalNav()
            );

        return ValueTask.CompletedTask;
    }
}
```  

### Implementing `INavigationProvider` to Add Menu Items

As mentioned about, you can implement the `INavigationProvider` interface to add menu items to any menu in your application. Below are specific implementations to guide you:

| **Class Name**            | **Description**                                                                    |
|---------------------------|------------------------------------------------------------------------------------|
| `NamedNavigationProvider` | Inherit from this class to add menu items to a menu with a specific name.          |
| `AdminNavigationProvider` | Inherit from this class to add menu items that will only appear in the admin menu. |



This provider will be called as long as the site is using a theme that includes a line similar to the following, which causes the navigation menu to be rendered by your theme at the location specified:
`@await DisplayAsync(await New.Navigation(MenuName: "main", RouteData: @ViewContext.RouteData))`

Examples of extending the admin navigation can be found in various OrchardCore modules. Searching the repository for `AdminMenu` will locate various settings. Below is a partial list:

- `OrchardCore.Modules/OrchardCore.Admin/AdminFilter.cs`
- `OrchardCore.Modules/OrchardCore.Media/AdminMenu.cs`

At this time, the Admin Menu is the only navigation with code dynamically adding items in the OrchardCore git repository. However, as the example above shows, the pattern can be used to control any named navigation.

## Pager Code examples

=== "Liquid"

    ``` liquid
    {% assign previousText = "← Newer Posts" | t %}
    {% assign nextText = "Older Posts →" | t %}
    {% assign previousClass = "previous" | t %}
    {% assign nextClass = "next" | t %}
    {% assign itemClasses = "itemclass1 itemclass2" | split: " " %}

    {% shape_pager Model.Pager previous_text: previousText, next_text: nextText,
        previous_class: previousClass, next_class: nextClass, tag_name: "div", item_tag_name: "div", attributes: "{\"key1\": \"value1\",\"key2\":\"value2\"}", item_attributes: "{\"key1\": \"value1\",\"key2\":\"value2\"}", classes: "class1 class2", item_classes: itemClasses %}

    {{ Model.Pager | shape_render }}
    ```

=== "C#"

    ``` html
    public async Task<IActionResult> List(MyViewModel viewModel, PagerParameters pagerParameters)
    {
        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pager = new Pager(pagerParameters, siteSettings.PageSize);
        var query = _session.Query<ContentItem, ContentItemIndex>();
        var maxPagedCount = siteSettings.MaxPagedCount;
        
        if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
            pager.PageSize = maxPagedCount;
                    
        var pagerShape = (await New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync()).RouteData(routeData).TagName("div").ItemTagName("div").Classes("class1 class2").ItemClasses(new List<string>(){ "itemclass1", "itemclass2" }).Attributes(new Dictionary<string, string>() { { "attribute", "value" } }).ItemAttributes(new Dictionary<string, string>() { { "itemattribute", "value" } });

        // Or you can also set the Shape base properties this way too :
        pagerShape.Id = "myid";
        pagerShape.TagName = "span";
        pagerShape.Attributes.Add("myattribute", "value");
        pagerShape.Classes.Add("myclassname");

        model.Pager = pagerShape;
        return View(viewModel);
    }
    ```

## Video

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/3w68lDwUzFQ" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

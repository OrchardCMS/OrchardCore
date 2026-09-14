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

The `Breadcrumb` shape renders the trail of pages leading to the page being displayed, e.g. _Manage Content › Edit Site Page_. It is meant to stand in for the title of an admin screen: the last node of the trail is the page itself, and it is rendered inside a heading tag so that it keeps the look, and the semantics, of the `<h1>` it replaces.

A trail is identified by a **name**, and it is built by every `IBreadcrumbProvider` registered on the tenant. That is what makes it extensible: a module adds a node to, or removes a node from, a trail described by another module without either of them knowing about the other.

### Rendering a breadcrumb

Add the `<breadcrumb>` tag helper to the `Title` zone of an admin view, and give it the name of the trail:

```html
@addTagHelper *, OrchardCore.Navigation.Core

<zone Name="Title">
    <breadcrumb name="@ContentsBreadcrumbs.Edit" data="@(new { ContentItem = contentItem })" />
</zone>
```

| Attribute    | Type     | Description                                                                                                                                                                   |
|--------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `name`       | `string` | Required. The name of the trail to render, e.g. `Contents.Edit`.                                                                                                              |
| `data`       | `object` | The contextual data of the page. Each property becomes an entry of `BreadcrumbBuilder.Data`, which every provider of the trail can read back.                                  |
| `heading`    | `string` | The html tag wrapping the text of the current node, so the breadcrumb can stand in for the title of the page. Defaults to `h1`. Set it to an empty value to render no heading. |
| `page-title` | `bool`   | Whether the text of the current node is registered as a segment of the `<title>` of the page. Defaults to `true`.                                                              |

The trail can also be built from code, which is what the tag helper does:

```csharp
var items = await _breadcrumbManager.BuildBreadcrumbAsync(ContentsBreadcrumbs.Edit, ViewContext, new Dictionary<string, object>
{
    { ContentsBreadcrumbs.ContentItemData, contentItem },
});

var shape = await _shapeFactory.BreadcrumbAsync(ContentsBreadcrumbs.Edit, items, heading: "h1");
```

### Describing a trail

Implement `IBreadcrumbProvider`, or inherit from `NamedBreadcrumbProvider` when the provider only contributes to a single trail, and register it with `services.AddBreadcrumbProvider<T>()`:

```csharp
public sealed class ContentsBreadcrumbProvider : NamedBreadcrumbProvider
{
    internal readonly IStringLocalizer S;

    public ContentsBreadcrumbProvider(IStringLocalizer<ContentsBreadcrumbProvider> localizer)
        : base(ContentsBreadcrumbs.Edit)
    {
        S = localizer;
    }

    protected override ValueTask BuildAsync(BreadcrumbBuilder builder)
    {
        if (!builder.TryGetData<ContentItem>(ContentsBreadcrumbs.ContentItemData, out var contentItem))
        {
            return ValueTask.CompletedTask;
        }

        builder.Add(S["Manage Content"], item => item
            .Id("Contents")
            .Action("List", "Admin", new RouteValueDictionary { { "area", "OrchardCore.Contents" } }));

        builder.Add(S["Edit {0}", contentItem.ContentType], item => item.Id("ContentItem"));

        return ValueTask.CompletedTask;
    }
}
```

Every provider of the tenant is called for every trail that is rendered, so a provider must check `builder.Name` before it adds anything. `NamedBreadcrumbProvider` does that for you.

A node is configured through `BreadcrumbItemBuilder`:

| Method                             | Description                                                                                                                    |
|------------------------------------|--------------------------------------------------------------------------------------------------------------------------------|
| `Text(string)`                     | The text to display for the node. It is also the first argument of `BreadcrumbBuilder.Add()`.                                  |
| `Id(string)`                       | The identifier of the node, used to build its shape alternates, and to let another provider find it.                           |
| `Url(string)` / `Action(…)`        | What the node links to. A node with neither is rendered as plain text.                                                         |
| `Position(string)`                 | The position of the node among the nodes of the trail, using the placement syntax, e.g. `5`, `end`.                            |
| `Permission(…)` / `Permissions(…)` | The permissions the user must all have for the node to be rendered as a link.                                                  |
| `Resource(object)`                 | The resource the permissions of the node are evaluated against.                                                                |
| `AddClass(string)`                 | A css class to render with the node.                                                                                           |

The manager orders the nodes by their position, computes the url of each of them, and marks the last one as the current node. The current node never links to itself. A node the user is not authorized to reach is rendered as plain text rather than removed, so that the trail stays complete.

### Adding a node to an existing trail

Because every provider sees every trail, a module extends a trail it does not own by reacting to its name and positioning its node:

```csharp
public sealed class LocalizationBreadcrumbProvider : NamedBreadcrumbProvider
{
    public LocalizationBreadcrumbProvider()
        : base(ContentsBreadcrumbs.Edit)
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

### Theming

The trail renders as the `Breadcrumb` shape, with one `BreadcrumbItem` shape per node. Both carry alternates built from the name of the trail, so a single screen can be templated on its own:

| Shape            | Alternates (least to most specific)                                                                                                     |
|------------------|-------------------------------------------------------------------------------------------------------------------------------------------|
| `Breadcrumb`     | `Breadcrumb__[Name]`, e.g. `Breadcrumb-Contents_Edit.cshtml`                                                                               |
| `BreadcrumbItem` | `BreadcrumbItem__[Name]`, `BreadcrumbItem__[Id]`, `BreadcrumbItem__[Name]__[Id]`, e.g. `BreadcrumbItem-Contents_Edit-ContentItem.cshtml`   |

The `.` of a name is encoded as `_` in a template file name, as it is for every other alternate.

The `BreadcrumbItem` shape carries the following properties:

| Property    | Type             | Description                                                                                     |
|-------------|------------------|---------------------------------------------------------------------------------------------------|
| `Name`      | `string`         | The name of the trail the node belongs to.                                                       |
| `Item`      | `BreadcrumbItem` | The node being rendered, with its classes, permissions and route values.                         |
| `Text`      | `string`         | The text to display for the node.                                                                |
| `Href`      | `string`         | The url the node links to, or `null` when the node is not a link.                                |
| `IsCurrent` | `bool`           | Whether the node is the page being rendered.                                                     |
| `Level`     | `int`            | The zero based index of the node in the trail.                                                   |
| `Heading`   | `string`         | The html tag wrapping the text of the current node, e.g. `h1`. It is `null` on the other nodes.  |

`TheAdmin` theme styles the trail through the `oc-breadcrumb` class the `Breadcrumb` shape carries, alongside a class built from its name, e.g. `breadcrumb-contents-edit`.

### Content management trails

The content management screens describe the following trails, whose names and data keys are exposed by `OrchardCore.Contents.ContentsBreadcrumbs`:

| Name              | Rendered by                    | Contextual data                                                                         |
|-------------------|--------------------------------|-------------------------------------------------------------------------------------------|
| `Contents.List`   | The content items list         | `ContentType`, the name of the content type the list is filtered on, when there is one.  |
| `Contents.Create` | The content item creation form | `ContentItem`, the content item being created.                                           |
| `Contents.Edit`   | The content item edition form  | `ContentItem`, the content item being edited.                                            |

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

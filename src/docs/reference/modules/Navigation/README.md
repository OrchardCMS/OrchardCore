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

The `<breadcrumb>` tag helper renders a trail of pages leading to the current page, e.g. _Dashboard › Manage Content › Edit Article_.

The parent helper's required `title` attribute takes `IHtmlContent` and normalizes it to the current page's plain-text label, the admin heading (`<h1>` by default), and a segment of the browser's `<title>`. For a visible trail, the helper appends this label as the final, unlinked current node after processing ancestors. A hidden admin trail uses only the explicit title, without evaluating children or providers. A view using these defaults removes its separate title output to avoid duplication. For a visible trail, `Breadcrumb.cshtml` renders the trail and heading together, so a theme can override that template to change their arrangement.

A trail is normally declared in Razor with a named `<breadcrumb>` tag helper and inline `<breadcrumb-item>` children. All children are **ancestors**, never the current page. The view supplies dynamic ancestor text and links using its model or injected services. Registered `IBreadcrumbProvider` implementations can postprocess these ancestors or add ancestors when there are no children, but cannot change the explicit current title. Trail names and ancestor IDs are stable literal strings; the helper assigns the current node the fixed `Id = "Title"`. The trail name provides its screen-specific alternate hook without a separate current-node attribute. No constants class or contextual-data contract is required.

The shapes live in the always-enabled `OrchardCore.Navigation` module. Inline authoring requires no custom provider registration. Ordinary navigation menus still use `INavigationProvider`, as described in [Extending Navigation](#extending-navigation); that API does not build breadcrumb trails.

### Adding a breadcrumb to your own screen

#### 1. Reference the tag helpers

The `<breadcrumb>` and `<breadcrumb-item>` tag helpers live in `OrchardCore.Navigation.Core`. Reference the project (or the `OrchardCore.Navigation.Core` package) from your module, then add it to the `Views/_ViewImports.cshtml` of your module:

```razor
@addTagHelper *, OrchardCore.Navigation.Core
```

#### 2. Declare the title and ancestor children

Put the tag helper in the `Title` zone of the view. Set `title` to the localized current-page text and declare only ancestors as children:

```razor
<zone Name="Title">
    <breadcrumb name="RecordsEdit" title="@T["Edit {0}", record.Name]">
        <breadcrumb-item id="Records" action="Index" controller="Record" area="My.Module" permission="@MyPermissions.ManageRecords.Name">@T["Records"]</breadcrumb-item>
    </breadcrumb>
</zone>
```

Use stable literal values for `name` and ancestor `id`. The name controls provider targeting, CSS classes, shape alternates, and name-based deduplication such as Dashboard. The current node is automatically identified as `Title`; do not duplicate it with a child. Name a trail after its screen, preferably in PascalCase without separators so alternate file names stay readable. If the screen has an admin list name, reuse it: the content items list uses `Contents` for both. No breadcrumb constants class or `data` attribute is needed.

#### 3. Avoid duplicate title output

Do not also render a separate `<h1>` or call `@RenderTitleSegments(...)` for the same title. The tag helper renders the heading and registers the current node's text as a page-title segment.

Rendering the trail in the `Title` zone is what makes it behave like the title it replaces, including honouring the **Display titles in the top bar** admin setting: when titles are shown in the top bar, the theme keeps the page title there and leaves the trail to the content area. `TheAdmin` does this in its own `Breadcrumb.cshtml`, which moves the trail to the `Breadcrumbs` zone when the setting is on. A trail that is meant to sit above the page without carrying its title goes in the `Breadcrumbs` zone instead, and is rendered with `heading=""` so that it renders no title of its own. Outside the admin that is already the default, because a front end page has a heading of its own and a second one would be wrong.

Set `page-title="false"` as well when the page already registers its browser title separately.

### The `<breadcrumb>` tag helper

| Attribute      | Type     | Description                                                                                                                                                                   |
|----------------|----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `name` | `string` | Required stable trail name, e.g. `ContentsEdit`. Used for provider targeting, the trail's CSS class, shape alternates, and name-based ancestor deduplication. |
| `title` | `Microsoft.AspNetCore.Html.IHtmlContent` | Required and non-null, including `LocalizedHtmlString`. Empty content such as `HtmlString.Empty` suppresses heading/browser-title text but retains the current node in a visible trail. |
| `heading`      | `string` | The html tag of the page title, rendered below the trail from the current node's text, so the breadcrumb carries the title of the page. Defaults to `h1` on the admin and to no title elsewhere. Set it to an empty value to render none. |
| `page-title`   | `bool`   | Whether the text of the current node is registered as a segment of the `<title>` of the page. Defaults to `true`.                                                               |
| `display-type` | `string` | The display type of the trail, which becomes an alternate of every shape it renders. Defaults to `DetailAdmin` on a request to the admin, and to `Detail` everywhere else.      |

Use the usual Razor localizer directly, as with `RenderTitleSegments`: `title="@T["Edit Record"]"` or `title="@T["Edit {0}", record.Name]"`. The helper handles formatting and normalization; do not extract `.Value` from the HTML-localized result or convert it yourself. Ancestor child text also uses `@T["..."]`.

Plain strings and non-HTML localizer results must be wrapped in the existing `OrchardCore.DisplayManagement.Html.HtmlContentString`, which encodes untrusted strings when written. Use `new HtmlContentString(value ?? string.Empty)` with `@using OrchardCore.DisplayManagement.Html`. For `Microsoft.Extensions.Localization.LocalizedString` and `OrchardCore.Localization.Data.DataLocalizedString` (from `IDataLocalizer`), wrap their `.Value`, not the localizer result itself. Never extract `T[...].Value` or use `Html.Raw` as a conversion.

With that namespace imported, prepare titles as follows:

| Value in the view | Title expression |
|-------------------|------------------|
| HTML-localized label | `title="@T["Edit {0}", record.Name]"` |
| Plain model string | `title="@(new HtmlContentString(Model.DisplayText ?? string.Empty))"` |
| `LocalizedString` | `title="@(new HtmlContentString(localizedTitle.Value ?? string.Empty))"` |
| `DataLocalizedString` | `title="@(new HtmlContentString(dataTitle.Value ?? string.Empty))"` |

An intentionally empty title can use `title="@Microsoft.AspNetCore.Html.HtmlString.Empty"`. This is non-null content; a null `Title` is invalid even when no output is needed.

Every title is normalized the same way when needed: render its `IHtmlContent` once through `WriteTo(writer, HtmlEncoder.Default)`, then HTML-decode the result once to obtain plain text. That string is safely encoded for the heading, current node, and browser title. Literal `&amp;` in a string wrapped with `HtmlContentString` remains literal `&amp;`, serialized as `&amp;amp;` in HTML. `IHtmlContent` does **not** enable title markup: final output is text-only, including any markup-looking text.

Inline children are the normal authoring path for ancestors, but providers may supply all ancestors. A self-closing helper must still declare both `name` and `title`:

```razor
<breadcrumb name="Example" title="@T["Edit Example"]" />
```

For a visible trail, the helper collects ancestor children first, then awaits providers sequentially in DI registration order on the same mutable ancestor list. It stably orders those ancestors, then **unconditionally appends the explicit title node as final and current**, with `Id = "Title"` and no link. This node remains even if providers clear the ancestor list or add an ancestor with `Position = "end"`. With no ancestors, a visible trail contains just the current node. Providers never supply or replace the heading or browser-title text.

### The `<breadcrumb-item>` tag helper

Each child supplies one ancestor, not the current page. Use localized text, such as `@T["Records"]`, rather than markup. The child helper HTML-decodes content already encoded by Razor and stores plain text. The parent's `IHtmlContent` title follows the render-and-decode step above. The heading and shape templates safely encode the resulting text, so names containing characters such as `&` or `<` remain text rather than markup.

| Attribute | Type | Description |
|-----------|------|-------------|
| `id` | `string` | Stable identifier used for shape alternates, not an HTML `id`. |
| `action`, `controller`, `area` | `string` | The route the node links to. Prefer these over constructing a URL for an MVC action. |
| `route-*` | `string` | Additional action route parameters, for example `route-id="@record.Id"`. |
| `route-values` | `IDictionary<string, string>` | Additional action route parameters supplied as a dictionary. |
| `url` | `string` | The URL the node links to. Ignored when `action` is set. |
| `permission` | `string` | Stored as `BreadcrumbItem.PermissionName` and resolved through `IPermissionService` only when finalizing a visible ancestor link. An unknown name is ignored; use a registered permission name. |
| `resource` | `object` | The resource against which the permission is evaluated. |
| `link-enabled` | `bool` | Whether the node may be linked. Defaults to `true`; set to `false` when the view has already determined that the link should be unavailable. |
| `position` | `string` | Relative position using the [placement syntax](../Placement/README.md#position-format), for example `10`, `before`, or `end`. |

After providers finish, the parent tag helper orders ancestors with `FlatPositionComparer`, preserving their current list order for equal positions. Without provider reordering, that is the children's declaration order. Most trails can omit `position` on every child. Explicit positions sort as `start`, `before`, numbers, `after`, free text, then `end`; numeric subpositions such as `1.1` allow insertion between positions. These positions only order ancestors; none can sort after the subsequently appended title node.

The appended title node is the **current page** and never becomes a link. All child and provider nodes remain ancestors, including the last sorted ancestor. An ancestor with no link target, a denied permission, or `link-enabled="false"` stays in the trail as plain text. Permissions control links rather than removing nodes.

Named permissions are not resolved while collecting children or running providers. Only surviving ancestors with linking enabled and a link target need that lookup during visible link processing. Removed or otherwise unlinkable ancestors do not resolve `PermissionName`, and the explicit current node never needs link processing. The existing `BreadcrumbItem.Permissions` list also accepts `Permission` objects directly; authorization evaluates the applicable permissions against `Resource`. Hidden trails perform neither permission-name lookup nor link authorization.

### Dynamic parents and nested editors

A nested editor should link back through its parent entity, using the parent's real display name. Read that entity from the view model or resolve it with an existing service injected into Razor. Do not introduce a breadcrumb-specific service or pass a contextual-data object to the helper.

For example, a record editor whose model exposes `Parent` and `Record` can declare:

```razor
<zone Name="Title">
    <breadcrumb name="RecordsEdit" title="@T["Edit {0}", Model.Record.Name]">
        <breadcrumb-item id="Records" action="Index" controller="Record" area="My.Module" permission="@MyPermissions.ManageRecords.Name">@T["Records"]</breadcrumb-item>
        <breadcrumb-item id="Parent" action="Edit" controller="Record" area="My.Module"
                         route-id="@Model.Parent.Id"
                         permission="@MyPermissions.ManageRecords.Name"
                         resource="@Model.Parent">@T["Edit {0}", Model.Parent.Name]</breadcrumb-item>
    </breadcrumb>
</zone>
```

When only a parent ID is available, an existing service can supply the entity. For example, a view whose model exposes `ParentContentItemId` can load a content item using:

```razor
@inject OrchardCore.ContentManagement.IContentManager ContentManager
@{
    var parent = await ContentManager.GetAsync(Model.ParentContentItemId);
}
```

Use the resulting entity's ID and display text in the inline children. Follow the screen's existing missing-parent handling and error reporting.

Resolve this data normally regardless of breadcrumb visibility. Views and partials must not read `AdminSettings.ShowBreadcrumb` or use it to guard parent lookups, node declarations, or the helper itself. The tag helper alone owns the visibility decision and the provider/title pipeline.

### Postprocessing with a provider

Implement `OrchardCore.Navigation.IBreadcrumbProvider` to postprocess a visible trail's ancestors after inline children have been collected and before the explicit current node is appended:

```csharp
public interface IBreadcrumbProvider
{
    ValueTask BuildBreadcrumbAsync(BreadcrumbContext context);
}
```

`BreadcrumbContext`, also in `OrchardCore.Navigation`, exposes these read-only properties:

| Property | Type | Description |
|----------|------|-------------|
| `Name` | `string` | The literal trail name. Filter it explicitly when the provider targets a particular trail. |
| `Title` | `string` | The normalized plain-text current-page title, not the original `IHtmlContent`. Read-only: providers cannot replace it. |
| `Items` | `List<BreadcrumbItem>` | The shared mutable ancestor list. Add, insert, remove, replace, or edit ancestors; later providers see those changes. The list reference is read-only, not its contents. The current node is not in this list. |
| `ViewContext` | `Microsoft.AspNetCore.Mvc.Rendering.ViewContext` | The current MVC view and request context, including `HttpContext`, for request-specific filtering. |
| `ShowTrail` | `bool` | Whether the trail is visible. Helper-created provider contexts always have `ShowTrail == true`; hidden trails do not create a context or invoke providers. |

Its public constructor is `BreadcrumbContext(string name, string title, List<BreadcrumbItem> items, ViewContext viewContext, bool showTrail)`, where `ViewContext` is the MVC type above. The tag helper passes the normalized plain string to this context; the provider contract does not accept the original HTML-aware object.

For example, this provider targets trail names starting with `Example` and adds an `Examples` ancestor, including when there are no inline children. It skips the `Examples` list itself by name, not by comparing localized title text:

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

public sealed class ExampleBreadcrumbProvider : IBreadcrumbProvider
{
    private readonly IStringLocalizer S;

    public ExampleBreadcrumbProvider(IStringLocalizer<ExampleBreadcrumbProvider> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbContext context)
    {
        if (!context.Name.StartsWith("Example", StringComparison.Ordinal) || context.Name == "Examples" ||
            context.Items.Any(item => item.Id == "Examples"))
        {
            return ValueTask.CompletedTask;
        }

        context.Items.Insert(0, new BreadcrumbItem
        {
            Id = "Examples",
            Text = S["Examples"].Value,
            Position = "start",
            Url = "~/examples",
        });

        return ValueTask.CompletedTask;
    }
}
```

Register it in the module's `Startup.ConfigureServices`:

```csharp
services.AddBreadcrumbProvider<ExampleBreadcrumbProvider>();
```

`AddBreadcrumbProvider<TProvider>()` registers a scoped implementation using `TryAddEnumerable`. Providers run only for visible trails, in DI registration order; each provider must filter `context.Name` or `context.ViewContext` when its changes apply only to certain trails or requests. For example, an admin-only provider checks `AdminAttribute.IsApplied(context.ViewContext.HttpContext)`.

Providers work only on ancestors, before positional sorting and before `IsCurrent` and `Href` are finalized. Set ancestor `Text` to a plain string, optionally from a localizer, not pre-encoded HTML. Set `Url` or `RouteValues` for a link, `PermissionName` for a named permission, or add `Permission` objects to `Permissions`; the helper finalizes links later. Do not rely on an item's incoming `IsCurrent` or `Href`, or try to make an ancestor current. After sorting, the helper appends the explicit title node last regardless of ancestor positions.

Providers cannot remove, replace, or edit the current title: it is read-only context data and is not part of `Items`. The helper creates provider contexts only for visible trails, always with `ShowTrail == true`. For hidden admin trails it bypasses provider resolution and execution entirely; only the explicit title can supply heading and browser-title text.

### Dashboard ancestor

The `OrchardCore.AdminDashboard` feature registers `OrchardCore.AdminDashboard.Services.DashboardBreadcrumbProvider`. It checks `AdminAttribute.IsApplied(context.ViewContext.HttpContext)` and inserts a localized Dashboard ancestor at list index `0`, with ID `Dashboard` and position `start`. It skips insertion when `context.Name == "Dashboard"` or an existing ancestor already has ID `Dashboard`. It does not compare localized title text.

The ancestor sets `Url` to `"~/" + AdminOptions.AdminUrlPrefix` and adds the static `OrchardCore.AdminDashboard.Permissions.AccessAdminDashboard` permission object to `Permissions`. It does not require a permission-name lookup. When Dashboard is a visible ancestor, denied access leaves it as plain text.

For a visible admin trail, the provider can add Dashboard when there are no other ancestors, but the helper still appends the explicit current title afterward with `Id = "Title"`. It does not change front-end trails. Like all providers, it is skipped entirely for hidden admin trails and never changes the heading or browser title. The Dashboard page uses `name="Dashboard"` to prevent a duplicate ancestor.

Thus `Manage Content › Edit Article` becomes `Dashboard › Manage Content › Edit Article` when the feature is enabled. Disabling the feature removes its provider contribution. This is an example of provider-based cross-module extension, not a special option on the tag helper.

### Disabling admin breadcrumbs

The **Show breadcrumb** setting under *Configuration → Settings → Admin* (`AdminSettings.ShowBreadcrumb`, enabled by default) hides the admin trail but retains the heading and browser title according to `heading` and `page-title`.

The helper checks the setting before any child or provider work. When `ShowBreadcrumb` is false, it never calls `GetChildContentAsync` and does not resolve, enumerate, construct, or invoke `IBreadcrumbProvider` implementations, including the Dashboard provider. It creates no `BreadcrumbContext` or `BreadcrumbItem` objects, not even the current title node. This title-only path applies regardless of provider registration.

Only the explicit `IHtmlContent` title is used. When a heading or browser title is needed, the helper formats it once through `WriteTo(writer, HtmlEncoder.Default)`, HTML-decodes once, and safely encodes the resulting text for output. It renders the heading directly with `oc-breadcrumb-title` and registers a browser-title segment according to `page-title`. There is no trail, URL generation, permission lookup, link authorization, or shape creation; breadcrumb templates and alternates do not run.

If the trail is hidden and both `heading=""` and `page-title="false"` are set, the helper validates the non-null title but also skips `WriteTo`, because no output is needed. Hidden trails always skip all child content. Parent attribute expressions and Razor code or model lookups outside child content still execute normally: `T[...]` results and `HtmlContentString` wrappers are created before the helper runs, even when their `WriteTo` call is skipped. Do not add visibility-setting reads or guard branches to views.

This setting only affects admin requests. Front-end trails still evaluate their children, invoke providers, and render shapes, including when they do not supply a heading or browser title.

### Customizing a trail

Change the owning Razor view or its partials for the explicit title and screen-specific ancestor children, using the model or injected services for conditional ancestors and parent lookups. Use `IBreadcrumbProvider` for shared or cross-module ancestor postprocessing, explicitly filtering the trail or request as needed. Stable names and ancestor IDs, together with the automatic current-node ID `Title`, support the shape alternates below for presentation changes. No separate manager, builder, named-provider base class, or `data` attribute is involved.

### Low-level shape creation

`IShapeFactory.BreadcrumbAsync` remains a presentation API for code that already has a sequence of `BreadcrumbItem` objects:

```csharp
var shape = await _shapeFactory.BreadcrumbAsync("RecordsEdit", items, heading: "h1", displayType: "DetailAdmin");
```

The caller must supply the complete ordered sequence, including its terminal current node, with authorized `Href` values and `IsCurrent` already resolved, keeping the last node unlinked. This API does not append an explicit title node, invoke providers, build a trail from a name, load parent entities, authorize links, add the Dashboard node, or register the browser title. Prefer the tag helper with `title` and inline ancestors when authoring a view.

### Theming

The trail renders as the `Breadcrumb` shape, with one `BreadcrumbItem` shape per node. Both carry alternates built from the name of the trail, so a single screen can be templated on its own.

For an ancestor, the node ID comes from its child `id` or provider-supplied `Id`. The helper always assigns its appended current node `Id = "Title"`. The existing alternate factory combines that ID with the trail name and display type; no separate current-node attribute or shape type is needed.

| Shape            | Alternates (least to most specific)                                                                                                                                                                                                  |
|------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Breadcrumb`     | `Breadcrumb__[Name]`, `Breadcrumb_[DisplayType]`, `Breadcrumb_[DisplayType]__[Name]`, e.g. `Breadcrumb-ContentsEdit.cshtml`, `Breadcrumb.DetailAdmin.cshtml`, `Breadcrumb-ContentsEdit.DetailAdmin.cshtml`                          |
| `BreadcrumbItem` | `BreadcrumbItem__[Name]`, `BreadcrumbItem__[Id]`, `BreadcrumbItem__[Name]__[Id]`, `BreadcrumbItem_[DisplayType]`, then the first three with the display type, e.g. `BreadcrumbItem-ContentsEdit-Title.DetailAdmin.cshtml` |

For the current title node, use `BreadcrumbItem-Title.cshtml`, `BreadcrumbItem-[Name]-Title.cshtml`, `BreadcrumbItem-Title.[DisplayType].cshtml`, or `BreadcrumbItem-[Name]-Title.[DisplayType].cshtml`. For example, `BreadcrumbItem-ContentsEdit-Title.DetailAdmin.cshtml` targets only the current node of the `ContentsEdit` admin trail. Ancestor ID-based alternates work independently.

The `BreadcrumbItem` shape carries the following properties:

| Property    | Type             | Description                                                                                    |
|-------------|------------------|------------------------------------------------------------------------------------------------|
| `Name`      | `string`         | The name of the trail the node belongs to.                                                      |
| `Item`      | `BreadcrumbItem` | The node being rendered, with its classes, permissions and route values.                        |
| `Text`      | `string`         | The text to display for the node.                                                               |
| `Href`      | `string`         | The url the node links to, or `null` when the node is not a link.                               |
| `IsCurrent` | `bool`           | Whether the node is the page being rendered.                                                    |
| `Level`     | `int`            | The zero based index of the node in the trail.                                                  |

The enclosing `Breadcrumb` shape carries the trail `Name`, the `Heading` tag of the page title, the current node's text as `Title`, and `ShowTrail`. `Breadcrumb.cshtml` renders the `<ol>` of nodes and then the page title in that heading tag. Code creating the shape through `BreadcrumbAsync` can pass `showTrail: false` to render only the title through the template. The tag helper instead bypasses shapes entirely when the **Show breadcrumb** admin setting is disabled, as described above.

A trail rendered on the admin and a trail rendered by a front end theme are the same shape, so they are told apart by their **display type**, the way the rest of the display system tells those contexts apart. The tag helper sets it to `DetailAdmin` on a request to the admin and to `Detail` everywhere else, and the `display-type` attribute overrides it.

The module ships one template for both, because the markup of a trail does not differ between them: what differs is the heading, and the tag helper already decides that per context. The display type is there so that a theme can diverge without dragging the other context along: templating `Breadcrumb.DetailAdmin.cshtml` restyles the admin and leaves the front end on the default, and `Breadcrumb.Detail.cshtml` does the opposite.

The `Breadcrumb` shape carries the `oc-breadcrumb` class, alongside a class built from the name of the trail, e.g. `breadcrumb-contents-edit`, which is how `TheAdmin` styles it. The markup is a plain [Bootstrap breadcrumb](https://getbootstrap.com/docs/5.3/components/breadcrumb/), so a theme restyles it with the `--bs-breadcrumb-*` custom properties.

`Breadcrumb.cshtml` renders the trail and the page title together, so it is the single template a theme overrides to change how the two sit. To render the title **above** the trail, for example, override `Breadcrumb.DetailAdmin.cshtml` in the theme and swap the order of the two:

```razor
@{
    var list = new TagBuilder("ol");
    list.AddCssClass("breadcrumb");

    string heading = Model.Heading;
    string title = Model.Title;

    foreach (var item in Model)
    {
        list.InnerHtml.AppendHtml(await DisplayAsync(item));
    }

    TagBuilder nav = Tag(Model, "nav");
    nav.Attributes["aria-label"] = T["Breadcrumb"].Value;
    nav.InnerHtml.AppendHtml(list);
}

@if (!string.IsNullOrEmpty(heading) && !string.IsNullOrEmpty(title))
{
    TagBuilder titleTag = new(heading);
    titleTag.AddCssClass("oc-breadcrumb-title");
    titleTag.InnerHtml.Append(title);

    @titleTag
}
@if (Model.ShowTrail)
{
    @nav
}
```

The same template is where a theme drops the `oc-breadcrumb-title` block to let the trail stand in for the title, or skips the `IsCurrent` node so the current page is not repeated in the trail. To turn the trail off across the whole admin without touching a template, clear **Show breadcrumb** under *Configuration → Settings → Admin*; the page title is then rendered on its own.

### Content management trails

The content management views use these literal trail names. Their children obtain any content type or content item information directly from Razor; there are no shared breadcrumb data keys.

| Name | Rendered by |
|------|-------------|
| `Contents` | The content items list. |
| `ContentsCreate` | The content item creation form. |
| `ContentsEdit` | The content item editing form. |

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

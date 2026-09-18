---
name: orchardcore-breadcrumbs
description: Authors explicit breadcrumb titles, inline ancestors, and lightweight IBreadcrumbProvider postprocessors on OrchardCore admin screens and the front end. Use when declaring current-page titles, extending ancestors, resolving a parent entity, documenting hidden-trail processing, or theming breadcrumbs through name-based shape alternates.
---

# OrchardCore Breadcrumbs

A breadcrumb is a **named trail with an explicit current-page title**. The parent `<breadcrumb>` requires an `IHtmlContent` title and normalizes it to text; every `<breadcrumb-item>` child is an ancestor. For visible trails, the view supplies dynamic ancestor links and localized text, and registered `IBreadcrumbProvider` implementations can postprocess only those ancestors. After ordering ancestors, the helper appends the normalized title as the final, unlinked current node. Hidden admin trails use only the explicit title and skip children and providers entirely. On the admin, the trail appears above a heading containing that text; on the front end, no heading is rendered by default.

Keep built-in views inline, with the current page in `title` rather than a child. Use lightweight providers for shared or cross-module ancestor changes, not a separate manager, builder, named-provider base class, or contextual-data contract. Customize the owning view or its partials for the title and screen-specific ancestors, and use shape alternates for presentation. `INavigationProvider` remains the separate extension point for ordinary navigation menus.

**Keep all Show breadcrumb logic in the tag helper.** Views and partials must not read `AdminSettings.ShowBreadcrumb` or gate lookups, child declarations, or breadcrumb markup on it. Declare nodes and resolve needed data normally; the helper alone chooses visibility and the provider/title pipeline.

## Mental model

1. Require a stable `name` and a non-null `Microsoft.AspNetCore.Html.IHtmlContent` title, including `LocalizedHtmlString`. Empty content such as `HtmlString.Empty` suppresses heading/browser-title text but still leaves a current node in a visible trail.
2. Check visibility before any child or provider work. A hidden admin trail never calls `GetChildContentAsync`, resolves or enumerates providers, constructs or invokes them, or creates a `BreadcrumbContext` or any breadcrumb items, including the current node.
3. For a hidden trail, only the explicit title matters. When heading or browser-title output is needed, call `WriteTo(writer, HtmlEncoder.Default)` once and HTML-decode once, then safely encode the text for output. With `heading=""` and `page-title="false"`, validate the non-null title but also skip `WriteTo`.
4. For a visible trail, normalize the title, collect ancestor children, and await registered providers sequentially in DI registration order on the shared mutable ancestor list. Helper-created provider contexts always have `ShowTrail == true`.
5. Stably sort visible ancestors with `FlatPositionComparer`, then unconditionally append the explicit title node with fixed `Id = "Title"`, last/current and never linked. Clearing ancestors or using `Position = "end"` cannot remove or displace it.
6. Visible trails finalize ancestor links and render shapes. Hidden trails render only the requested explicit heading/browser title, with no URL generation, permission lookup, link authorization, or shapes. The admin setting does not affect front-end trails.

## Naming

- Use stable literal strings directly in `.cshtml`: `name="ContentsEdit"` and ancestor `id="Contents"`.
- Use `name` for provider targeting, the trail's CSS class, shape alternates, and name-based deduplication, including Dashboard. The current node automatically has `Id = "Title"`; its name-specific alternate needs no separate attribute.
- Reuse a screen's admin list name when it has one. The content items list uses `Contents`.
- Prefer PascalCase names without separators so shape alternate file names remain readable.
- Do not introduce breadcrumb constants classes, contextual-data keys, or a `data` attribute.
- Ancestor IDs and the automatic current-node ID `Title` identify node shape alternates; they are not rendered as HTML `id` attributes.

## Workflow A: add a breadcrumb to a view

### Step 1: Import the tag helpers

Reference `OrchardCore.Navigation.Core` and add this to the module's `Views/_ViewImports.cshtml` if it is not already imported:

```razor
@addTagHelper *, OrchardCore.Navigation.Core
```

The always-enabled `OrchardCore.Navigation` module supplies the shapes. Inline authoring needs no custom provider registration.

### Step 2: Declare the explicit title and ancestor children

```razor
<zone Name="Title">
    <breadcrumb name="RecordsEdit" title="@T["Edit {0}", record.Name]">
        <breadcrumb-item id="Records" action="Index" controller="Record" area="My.Module" permission="@MyPermissions.ManageRecords.Name">@T["Records"]</breadcrumb-item>
    </breadcrumb>
</zone>
```

Prefer paired parent tags with inline ancestors. The parent supplies the current page through `title`; the helper appends it with ID `Title`. Do not add a duplicate current-page child.

Pass the normal Razor localizer result directly, as with `RenderTitleSegments`: `title="@T["Edit Record"]"` or `title="@T["Edit {0}", record.Name]"`. The helper renders the HTML-aware value when needed, including its formatting arguments. Do not extract `.Value` from `T[...]` or add caller conversions. Keep `@T["..."]` for ancestor child text too.

Plain strings require the existing safe wrapper `new OrchardCore.DisplayManagement.Html.HtmlContentString(value ?? string.Empty)`. For `Microsoft.Extensions.Localization.LocalizedString` and `OrchardCore.Localization.Data.DataLocalizedString` from `IDataLocalizer`, wrap `.Value ?? string.Empty`. Do not pass these non-HTML values directly, extract `T[...].Value`, or use `Html.Raw`: `HtmlContentString` encodes untrusted strings when written.

A self-closing helper still requires both attributes, for example `<breadcrumb name="Example" title="@T["Edit Example"]" />`. Providers may supply ancestors, but not the current title. With no ancestors, a visible trail still contains the appended current node.

| Parent attribute | Behavior |
|------------------|----------|
| `name` | Required stable trail name used for provider targeting, CSS, shape alternates, and name-based ancestor deduplication. |
| `title` | Required non-null `Microsoft.AspNetCore.Html.IHtmlContent`, including `LocalizedHtmlString`. Empty content such as `HtmlString.Empty` suppresses heading/browser-title text, not the current node in a visible trail. |
| `heading` | Defaults to `h1` on admin requests and no heading on the front end. `heading=""` suppresses the heading. |
| `page-title` | Defaults to `true`; adds the nonempty explicit title as a browser-title segment. |
| `display-type` | Defaults to `DetailAdmin` on admin requests and `Detail` elsewhere. Supplies shape alternates. |

### Title normalization

With `@using OrchardCore.DisplayManagement.Html`, prepare the view's values as follows:

| Value in the view | Title expression |
|-------------------|------------------|
| HTML-localized label | `title="@T["Edit {0}", record.Name]"` |
| Plain model string | `title="@(new HtmlContentString(Model.DisplayText ?? string.Empty))"` |
| `LocalizedString` | `title="@(new HtmlContentString(localizedTitle.Value ?? string.Empty))"` |
| `DataLocalizedString` | `title="@(new HtmlContentString(dataTitle.Value ?? string.Empty))"` |

Use `title="@Microsoft.AspNetCore.Html.HtmlString.Empty"` for intentionally empty content. A null title is invalid, including on the no-output path.

When needed, every title follows the same normalization: render its `IHtmlContent` once with `WriteTo(writer, HtmlEncoder.Default)`, then HTML-decode once into plain text. The helper safely encodes that string for the heading, current node, and browser title. Literal `&amp;` in a string wrapped with `HtmlContentString` stays literal `&amp;`, serialized as `&amp;amp;` in HTML. Do not pre-encode the wrapper's input. `IHtmlContent` does not enable arbitrary title markup: final title output remains text-only.

### Step 3: Remove duplicate title output

Remove the view's old page heading and `RenderTitleSegments` call. The helper supplies both by default on the admin. Keep the helper in the `Title` zone so it works with the admin theme's top-bar title setting.

For a trail that does not own the heading, use the `Breadcrumbs` zone and `heading=""`. Also set `page-title="false"` if the page registers its browser title separately.

## Child attributes and link behavior

| Attribute | Type | Behavior |
|-----------|------|----------|
| `id` | `string` | Stable node identifier for shape alternates. |
| `action`, `controller`, `area` | `string` | MVC route target. Prefer these over hand-built action URLs. |
| `route-*` | `string` | Additional action route values, such as `route-id="@record.Id"`. |
| `route-values` | `IDictionary<string, string>` | Additional action route values supplied as a dictionary. |
| `url` | `string` | Direct link target; ignored when `action` is set. |
| `permission` | `string` | Stored as `BreadcrumbItem.PermissionName`, resolved through `IPermissionService` during visible ancestor link processing after providers; unknown names are ignored. |
| `resource` | `object` | Resource passed to authorization for the permission. |
| `link-enabled` | `bool` | Defaults to `true`. Use `false` when the view has already determined that the link should be unavailable. |
| `position` | `string` | Relative position, such as `10`, `before`, or `end`. |

The inner content is localized ancestor text, for example `@T["Records"]`. Child content is HTML-decoded after Razor evaluates it, then safely encoded by the shape template. The parent's `IHtmlContent` title is rendered and decoded once as described above. Templates safely encode normalized title and provider-supplied text; do not emit them with `Html.Raw`.

Every child and provider item remains an ancestor, including the last sorted item. The helper appends a separate title node after all ancestors; only that node is current and **never linked**. Ancestors with denied permissions, `link-enabled="false"`, or no link target remain visible as text. Do not omit ancestors solely because the user cannot follow their links.

Children and providers only store named permissions. The helper resolves `PermissionName` after providers, sorting, and link-eligibility checks, so removed or unlinkable ancestors do not cause a name lookup. The explicit title node never needs link processing. Providers can also add `Permission` objects to `BreadcrumbItem.Permissions`; authorization evaluates applicable permissions against `Resource`. Hidden trails perform neither permission lookup nor authorization.

## Workflow B: resolve a nested editor's parent

Use the real parent's display name and route ID, obtained from the view model when available. For example, a model exposing `Parent` and `Record` can render:

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

If only the parent ID is available, inject an existing domain service into Razor, not a breadcrumb-specific service. A model with `ParentContentItemId`, for example, can use:

```razor
@inject OrchardCore.ContentManagement.IContentManager ContentManager
@{
    var parent = await ContentManager.GetAsync(Model.ParentContentItemId);
}
```

Use the loaded parent's ID and display text in ancestor child items. Follow the screen's existing missing-parent handling and error reporting. Do not guard parent lookups on the breadcrumb visibility setting. Hidden trails always skip child content, but parent attribute expressions and Razor code or model lookups outside that child content still execute.

## Workflow C: postprocess a trail with a provider

Implement `OrchardCore.Navigation.IBreadcrumbProvider`:

```csharp
public interface IBreadcrumbProvider
{
    ValueTask BuildBreadcrumbAsync(BreadcrumbContext context);
}
```

`BreadcrumbContext` is also in `OrchardCore.Navigation`. Its public constructor is `BreadcrumbContext(string name, string title, List<BreadcrumbItem> items, ViewContext viewContext, bool showTrail)`, with `ViewContext` from `Microsoft.AspNetCore.Mvc.Rendering`. The helper passes the normalized plain-string title, not the original `IHtmlContent`. It exposes read-only properties:

| Property | Type | Use |
|----------|------|-----|
| `Name` | `string` | Filter the trail's literal name explicitly. |
| `Title` | `string` | Normalized plain-text explicit title, which providers cannot replace. |
| `Items` | `List<BreadcrumbItem>` | Shared mutable ancestor list. Add, insert, remove, replace, or edit ancestors; do not replace the read-only list reference. The explicit current node is not in the list. |
| `ViewContext` | `Microsoft.AspNetCore.Mvc.Rendering.ViewContext` | Read request context, such as `HttpContext`, when filtering by admin/front-end request. |
| `ShowTrail` | `bool` | Whether the trail is visible. Helper-created provider contexts always have `ShowTrail == true`; hidden trails create no context and invoke no providers. |

Providers see all visible trails unless they explicitly filter the context. This example targets names starting with `Example`, excluding the `Examples` list itself by name rather than localized title text:

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

Register the provider in `Startup.ConfigureServices`:

```csharp
services.AddBreadcrumbProvider<ExampleBreadcrumbProvider>();
```

Registration is scoped and uses `TryAddEnumerable`. For visible trails, providers run in DI registration order after collecting ancestor children, before sorting ancestors, appending the explicit current node, and finalizing `IsCurrent` and `Href`. Later providers observe earlier ancestor mutations.

Use `context.Name` to target a trail, or `context.ViewContext` to target a request. An admin-only provider checks `AdminAttribute.IsApplied(context.ViewContext.HttpContext)`. Set `Text` to a plain string, not encoded HTML; use `Url` or `RouteValues`, `PermissionName` or the `Permissions` list, `Resource`, and `LinkEnabled` to describe links. Do not depend on incoming `IsCurrent` or `Href`, which the helper computes afterward.

The example also supplies an ancestor to a trail with no inline children. The view must still provide its title:

```razor
<breadcrumb name="Example" title="@T["Edit Example"]" />
```

Providers cannot remove or replace the explicit title: `Title` is read-only, and `Items` contains only ancestors. The helper appends the title node with `Id = "Title"` after sorting, even if a provider cleared every ancestor or used position `end`.

Hidden admin trails bypass providers completely: the helper does not resolve, enumerate, construct, or invoke `IBreadcrumbProvider` implementations. It creates no `BreadcrumbContext`; the retained `ShowTrail` property and constructor parameter are always true in helper-created provider contexts. Only the explicit title supplies hidden heading/browser-title output.

## Dashboard ancestor

The Admin Dashboard feature registers `OrchardCore.AdminDashboard.Services.DashboardBreadcrumbProvider` with `AddBreadcrumbProvider<DashboardBreadcrumbProvider>()`.

The provider checks `AdminAttribute.IsApplied(context.ViewContext.HttpContext)` and leaves front-end trails unchanged. It skips insertion if `context.Name == "Dashboard"` or an ancestor already has ID `Dashboard`; it does not compare localized title text. Otherwise it inserts a localized Dashboard ancestor at index `0`, with ID `Dashboard`, position `start`, and `Url = "~/" + adminOptions.AdminUrlPrefix`.

Its `Permissions` list contains the static `OrchardCore.AdminDashboard.Permissions.AccessAdminDashboard` permission object, so no named-permission lookup is required. Denied access makes a visible Dashboard ancestor plain text. The Dashboard page uses `name="Dashboard"` and `title="@T["Dashboard"]"` so only the explicit current node, with ID `Title`, represents Dashboard.

For visible admin trails, Dashboard can be added when there are no other ancestors, but the explicit current title is still appended afterward. Hidden trails skip this provider entirely; it never affects title text. Disabling the feature removes this provider contribution; there is no Dashboard option on the tag helper.

## Positions

`FlatPositionComparer` orders ancestor positions as `start`, `before`, numbers, `after`, free text, then `end`. Numeric subpositions such as `1.1` sort between `1` and `2`. Sorting happens after providers, and equal positions preserve the resulting ancestor list order.

Most inline ancestors can omit `position` and rely on declaration order unless a provider changes it. Positions cannot change the current page: the helper appends the explicit title after every ancestor, including those at `end`.

## Disabled admin trails

**Show breadcrumb** under **Configuration > Settings > Admin** (`AdminSettings.ShowBreadcrumb`, default on) controls admin trail visibility, not front-end trails.

When disabled, the helper **skips all providers and breadcrumb items**, regardless of provider registration. It never calls `GetChildContentAsync`, never resolves, enumerates, constructs, or invokes providers (including Dashboard), and creates no `BreadcrumbContext` or items, including the title node. Child text, positions, and links are not evaluated.

Only the explicit title supplies heading/browser-title text. When needed, it is formatted once through `WriteTo(writer, HtmlEncoder.Default)` and HTML-decoded once. The helper safely encodes the normalized string, renders the heading directly with `oc-breadcrumb-title`, and registers the browser title when `page-title` is true. It does not generate URLs, resolve permission names, authorize links, or create shapes. Breadcrumb templates and alternates do not run.

When the trail is hidden and both `heading=""` and `page-title="false"` are set, the helper validates the non-null title but also skips `WriteTo`. Parent attribute expressions and Razor code or model lookups outside child content still run: `T[...]` results and `HtmlContentString` wrappers are created before the helper, even when it skips `WriteTo`. All child-only work is skipped for every hidden trail; do not add visibility guards to views. For a visible trail, these attributes suppress only heading/browser-title output, not ancestors, providers, or the explicit current node.

## Theming and low-level shapes

Visible trails use the existing `Breadcrumb` and `BreadcrumbItem` shapes and their name, ID, and display-type alternates:

- Whole trail: `Breadcrumb.cshtml`, `Breadcrumb-ContentsEdit.cshtml`, `Breadcrumb.DetailAdmin.cshtml`, `Breadcrumb-ContentsEdit.DetailAdmin.cshtml`.
- One node: `BreadcrumbItem.cshtml`, `BreadcrumbItem-Dashboard.cshtml`, `BreadcrumbItem-ContentsEdit-Title.DetailAdmin.cshtml`.

`Breadcrumb.cshtml` owns both the visible trail and heading. Override it to rearrange them. Keep text safely encoded. See `references/theming.md` for alternate precedence, encoding, and the distinction between hidden admin trails and shape-level `ShowTrail`.

The helper assigns the current node `Id = "Title"`. The existing alternate factory provides `BreadcrumbItem-Title.cshtml`, `BreadcrumbItem-[Name]-Title.cshtml`, `BreadcrumbItem-Title.[DisplayType].cshtml`, and `BreadcrumbItem-[Name]-Title.[DisplayType].cshtml`; no extra attribute or shape type is needed. Ancestor IDs still come from child `id` or provider-supplied `Id`.

`IShapeFactory.BreadcrumbAsync` is a low-level presentation API for prebuilt `BreadcrumbItem` objects. Its caller supplies the complete ordered sequence, including the terminal current node, with authorized `Href` values and `IsCurrent` already resolved. It does not append a title node, invoke providers, build trails from names, load parents, add Dashboard, or register the browser title. It is not the recommended authoring path for a view.

## Gotchas

- **Missing title is invalid.** Every helper, including a self-closing one, needs a non-null `IHtmlContent` title, even on the no-output path. `HtmlString.Empty` is valid and still leaves a current node in a visible trail.
- **Wrap non-HTML title values safely.** Pass `@T[...]` directly, never its `.Value`. Use `new HtmlContentString(value ?? string.Empty)` for strings and `new HtmlContentString(localizedTitle.Value ?? string.Empty)` for non-HTML localizer results; never `Html.Raw`.
- **HTML-aware input is still text-only output.** Do not render normalized title text as raw HTML or decode it again.
- **Children are ancestors only.** Declare the current page in parent `title`; the helper appends it with ID `Title`, so do not duplicate it with a child.
- **Providers cannot change the current title.** Clearing ancestors or assigning position `end` never removes or replaces the appended current node.
- **Hidden means title-only.** The helper skips all provider resolution and execution, child evaluation, contexts, and items. Provider contexts created for visible trails always have `ShowTrail == true`.
- **No visibility guards in views.** Do not read `AdminSettings.ShowBreadcrumb` or wrap lookups or markup in setting-dependent branches; visibility belongs to the helper.
- **Named permissions are deferred.** Do not look them up while collecting or postprocessing ancestors; removed or unlinkable ancestors do not need that work.
- **Second heading.** Remove the old title output when the helper owns the title.
- **Wrong dictionary type.** `route-values` accepts `IDictionary<string, string>`, not an anonymous object or `RouteValueDictionary`.
- **Link checks are not endpoint security.** Keep authorization on the target action; `permission` and `link-enabled` control only the rendered link.
- **Renamed identifiers affect themes and providers.** Keep trail names and ancestor IDs stable. The automatic current-node ID is always `Title`; screen-specific title alternates use the trail name.
- **Hidden trails bypass templates.** A breadcrumb shape override cannot change the helper's directly rendered disabled-state heading.

## References

- `references/theming.md` - alternate tables, display types, front-end usage, and heading behavior.
- `src/OrchardCore/OrchardCore.Navigation.Core/` - `BreadcrumbTagHelper`, `BreadcrumbItemTagHelper`, `IBreadcrumbProvider`, `BreadcrumbContext`, `BreadcrumbItem`, and `ShapeFactoryExtensions`.
- `src/OrchardCore.Modules/OrchardCore.AdminDashboard/Services/DashboardBreadcrumbProvider.cs` - feature-registered admin ancestor provider.
- `src/OrchardCore.Modules/OrchardCore.Navigation/` - `BreadcrumbShapes`, `BreadcrumbAlternatesFactory`, and the breadcrumb views.
- `src/docs/reference/modules/Navigation/README.md` - canonical breadcrumb reference.
- `AGENTS.md` - project conventions.

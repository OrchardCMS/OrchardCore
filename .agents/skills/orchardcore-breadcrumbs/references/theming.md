# Breadcrumb theming & shape alternates

For a visible trail, the `<breadcrumb>` tag helper produces a `Breadcrumb` shape holding one `BreadcrumbItem` shape per final node. It collects ancestor children in declaration order, invokes `IBreadcrumbProvider` implementations in DI registration order, preserves the resulting list order, then unconditionally appends the parent's explicit `title` as the final/current node with `Id = "Title"`. Both shapes are ordinary display-managed shapes, so a theme overrides their rendering by supplying a `.cshtml` whose name matches an alternate. The display engine tries alternates **most specific first**; the last one in each list below is the most specific.

Declare the current-page label in the parent's required `title` attribute, and keep only ancestors as inline children. Use stable literal `name` and ancestor `id` values; the current node's ID is automatically `Title`. Providers can add, remove, replace, or edit ancestors across modules, but cannot change the current title. A provider-only ancestor trail still needs an explicit title, for example `<breadcrumb name="Example" title="@T["Edit Example"]" />`. Providers implement `ValueTask BuildBreadcrumbAsync(BreadcrumbContext context)`, filtering the trail name or MVC request context when appropriate. Shape alternates customize presentation, not the provider pipeline.

The parent's `Title` property is required, non-null `Microsoft.AspNetCore.Html.IHtmlContent`, including `LocalizedHtmlString`. Empty content such as `Microsoft.AspNetCore.Html.HtmlString.Empty` suppresses heading/browser-title text but retains the terminal current node in a visible trail. `BreadcrumbContext.Title` is a read-only normalized plain `string`; its constructor is `BreadcrumbContext(string name, string title, List<BreadcrumbItem> items, ViewContext viewContext, bool showTrail)`. Providers see only ancestors in `Items`. Helper-created provider contexts always have `ShowTrail == true`, because hidden trails skip providers and create no context.

The stable `name` enables provider targeting, the trail's CSS class, shape alternates, and name-based deduplication such as Dashboard. Combined with the fixed ID `Title`, it provides a screen-specific current-title hook without a separate attribute.

## Display types

The trail's display type separates the admin presentation from the front end, the same way the rest of the display system tells those contexts apart.

| Context | Default display type |
|---------|----------------------|
| Admin request (`AdminAttribute.IsApplied`) | `DetailAdmin` |
| Front end | `Detail` |
| Explicit | value of the `display-type` attribute |

The default `Breadcrumb.cshtml` / `BreadcrumbItem.cshtml` render both contexts. Ship a display-type-specific template **only** when the admin and the front end genuinely need to differ — don't duplicate an identical `Breadcrumb.DetailAdmin.cshtml`.

## `Breadcrumb` alternates (whole trail)

For name `ContentsEdit`, display type `DetailAdmin`:

| Alternate | File | Matches |
|-----------|------|---------|
| `Breadcrumb` | `Breadcrumb.cshtml` | every trail (base) |
| `Breadcrumb__ContentsEdit` | `Breadcrumb-ContentsEdit.cshtml` | this trail, any context |
| `Breadcrumb_DetailAdmin` | `Breadcrumb.DetailAdmin.cshtml` | every trail on admin |
| `Breadcrumb_DetailAdmin__ContentsEdit` | `Breadcrumb-ContentsEdit.DetailAdmin.cshtml` | this trail on admin |

The base shape also gets the css classes `oc-breadcrumb` and `breadcrumb-{name-htmlclassified}` (e.g. `breadcrumb-contents-edit`), so you can also style without a template override.

## `BreadcrumbItem` alternates (one node)

For name `ContentsEdit`, automatic current-node `Id = "Title"`, display type `DetailAdmin`:

| Alternate | File |
|-----------|------|
| `BreadcrumbItem` | `BreadcrumbItem.cshtml` (base) |
| `BreadcrumbItem__ContentsEdit` | `BreadcrumbItem-ContentsEdit.cshtml` |
| `BreadcrumbItem__Title` | `BreadcrumbItem-Title.cshtml` |
| `BreadcrumbItem__ContentsEdit__Title` | `BreadcrumbItem-ContentsEdit-Title.cshtml` |
| `BreadcrumbItem_DetailAdmin` | `BreadcrumbItem.DetailAdmin.cshtml` |
| `BreadcrumbItem_DetailAdmin__ContentsEdit` | `BreadcrumbItem-ContentsEdit.DetailAdmin.cshtml` |
| `BreadcrumbItem_DetailAdmin__Title` | `BreadcrumbItem-Title.DetailAdmin.cshtml` |
| `BreadcrumbItem_DetailAdmin__ContentsEdit__Title` | `BreadcrumbItem-ContentsEdit-Title.DetailAdmin.cshtml` |

Alternates keyed on `[Id]` are only produced when a node has an ID. An ancestor gets its ID from a child `id` or provider-supplied `Id`; the helper always assigns the current node `Id = "Title"`. The existing factory therefore supplies `BreadcrumbItem-Title.cshtml`, `BreadcrumbItem-[Name]-Title.cshtml`, `BreadcrumbItem-Title.[DisplayType].cshtml`, and `BreadcrumbItem-[Name]-Title.[DisplayType].cshtml`. No extra attribute, factory, or shape type is needed. Ancestor IDs and alternates work independently. These IDs are not HTML `id` attributes.

The Admin Dashboard feature registers `OrchardCore.AdminDashboard.Services.DashboardBreadcrumbProvider`. For visible trails, it checks `AdminAttribute.IsApplied(context.ViewContext.HttpContext)` and inserts a localized ancestor at index `0` with ID `Dashboard`, unless `context.Name == "Dashboard"` or an ancestor already has that ID. It does not compare localized title text. Later providers can change its place in the list. It can add Dashboard when no other ancestors exist, but hidden admin trails skip it entirely. It does not change front-end trails or the explicit current title.

The ancestor uses `Url = "~/" + adminOptions.AdminUrlPrefix` and the static `OrchardCore.AdminDashboard.Permissions.AccessAdminDashboard` permission object in its `Permissions` list. Denied access leaves it as text. The Dashboard page's `name="Dashboard"` prevents a duplicate ancestor; its current node has ID `Title` and is never linked. Use `BreadcrumbItem-Dashboard-Title.cshtml` to restyle that current node specifically. The ancestor's ID-based `BreadcrumbItem-Dashboard.cshtml` alternate is still available; as a name-based alternate it also matches nodes in the `Dashboard` trail.

## Name/id encoding

`EncodeAlternateElement` maps a name/id to its alternate token:

- `.` → `_`
- `-` → `__`

So a trail named `My.Trail` contributes `Breadcrumb__My_Trail`. Keep trail names and node ids simple (letters/digits) to keep alternate file names readable.

## The default templates

`Breadcrumb.cshtml` emits a Bootstrap 5 breadcrumb: `<nav aria-label>`, containing `<ol class="breadcrumb">`, with one `<li class="breadcrumb-item">` per node rendered by `BreadcrumbItem.cshtml`. The shape's `Heading` selects the optional heading tag (`h1` by default on admin requests), and `Title` holds the current node's text. The heading has class `oc-breadcrumb-title`.

After providers finish, the parent helper preserves the ancestor list order without sorting. It appends the explicit title node afterward, even if all ancestors were cleared or reordered. Only this appended node is current and never linked; its `<li>` gets `active` and `aria-current="page"`. The last ancestor in the list remains an ancestor. Ancestors render an `<a>` only when they have an authorized `Href`; denied permissions, `link-enabled="false"`, or no link target leave plain text. Overrides should respect `Href` rather than rebuilding links from route values.

The child `permission` attribute and provider-assigned `BreadcrumbItem.PermissionName` are resolved only during visible ancestor link processing. Removed or otherwise unlinkable ancestors do not resolve the name; unknown names are ignored. The explicit current node never needs link processing. `BreadcrumbItem.Permissions` also accepts `Permission` objects directly. All permission lookup and link authorization are skipped for hidden trails.

Prefer `@T[...]` directly for simple or formatted parent titles, for example `title="@T["Edit {0}", record.Name]"`; never extract `T[...].Value`. Wrap plain strings with `new OrchardCore.DisplayManagement.Html.HtmlContentString(value ?? string.Empty)`. For `Microsoft.Extensions.Localization.LocalizedString` or `OrchardCore.Localization.Data.DataLocalizedString` from `IDataLocalizer`, wrap `.Value ?? string.Empty`. The wrapper encodes untrusted strings; do not substitute `Html.Raw`. Ancestor child text still uses `@T["..."]` and is HTML-decoded after Razor evaluation.

When needed, the title is normalized by rendering its `IHtmlContent` once with `WriteTo(writer, HtmlEncoder.Default)`, then HTML-decoding once into a plain string. This applies localization formatting and preserves safely encoded arguments. Literal `&amp;` in a string wrapped with `HtmlContentString` stays literal `&amp;`, serialized as `&amp;amp;` in HTML. Do not pre-encode the wrapper's input or decode the normalized text again.

The normalized title is safely encoded for the heading, current node, and browser title. Accepting `IHtmlContent` does not enable arbitrary title markup: output remains text-only. Providers also assign plain strings to ancestor `Text`. Templates use `@Model.Text` or `TagBuilder.InnerHtml.Append(title)`, not `Html.Raw`; they must not decode normalized title text again or interpret it as markup.

Because the trail and heading are produced together, `Breadcrumb.cshtml` (or its name/display-type alternates) is the override point for their arrangement. Render the heading first, omit it, or skip displaying the `IsCurrent` item in the trail while retaining `Title`. Bootstrap's breadcrumb variables control the separator and link color; prefer theme CSS when only styling changes.

## Hidden admin trails versus shape-level ShowTrail

When an administrator clears **Show breadcrumb** (`AdminSettings.ShowBreadcrumb`, default on), the helper takes a title-only path. It never calls `GetChildContentAsync` and does not resolve, enumerate, construct, or invoke providers, including Dashboard. It creates no `BreadcrumbContext` or breadcrumb items, including the current title node, regardless of provider registration. There is no URL generation, permission lookup, link authorization, or shape creation; templates and alternates are bypassed.

Only the explicit title supplies output. When heading or browser-title output is needed, the helper calls `WriteTo(writer, HtmlEncoder.Default)` once and HTML-decodes once, then safely encodes the resulting string. It renders the nonempty heading directly with `oc-breadcrumb-title` and registers a browser-title segment if `page-title` is true. If both `heading=""` and `page-title="false"` are set, the helper validates the non-null title but also skips `WriteTo`. This admin setting does not affect front-end trails or their provider pipeline.

The tag helper alone reads `AdminSettings.ShowBreadcrumb` and decides visibility. Authoring views and partials declare the title and ancestors and resolve their data normally; do not read the setting there or guard lookups or markup on it. Hidden trails always skip child-only work. Parent attribute expressions and Razor code or model lookups outside child content still execute normally: `T[...]` results and `HtmlContentString` wrappers are created before the helper, even when it skips `WriteTo`.

Separately, `IShapeFactory.BreadcrumbAsync` accepts a complete prebuilt sequence, including the terminal current node, with authorized `Href` and `IsCurrent` values already resolved. Its `showTrail: false` argument creates a shape with `ShowTrail` false and the last item's text as `Title`; the template renders only the heading. A custom template should honor `ShowTrail` and use `Title` rather than relying on child shapes in that case. This low-level presentation API does not append a title node, invoke providers, build or authorize a trail, add Dashboard, or register the browser title. Prefer the tag helper with explicit `title` and inline ancestors for view authoring.

## Front end usage

Front-end views use the same inline authoring model, with display type `Detail` and **no heading** by default. `page-title` still defaults to `true`; set it to `false` if the page already registers its browser title:

```razor
@using OrchardCore.DisplayManagement.Html

<breadcrumb name="ProductDetails" title="@(new HtmlContentString(Model.Product.Name ?? string.Empty))" page-title="false">
    <breadcrumb-item id="Catalog" action="Index" controller="Catalog" area="My.Module">@T["Catalog"]</breadcrumb-item>
</breadcrumb>
```

Dynamic parents come from the view model or services injected into Razor, just as on the admin. Providers can postprocess front-end trails too, subject to their explicit filtering. Use the `Detail` display-type alternates for front-end-specific markup. The admin visibility setting does not apply here, and the Dashboard provider makes no front-end changes. Even with no heading and `page-title="false"`, a visible front-end trail evaluates its children and providers and renders normally.

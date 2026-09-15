# Breadcrumb theming & shape alternates

The `<breadcrumb>` tag helper produces a `Breadcrumb` shape holding one `BreadcrumbItem` shape per node. Both are ordinary display-managed shapes, so a theme (or a module with a later placement precedence) overrides their rendering by supplying a `.cshtml` whose name matches an alternate. The display engine tries alternates **most specific first**; the last one in each list below is the most specific.

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

For name `ContentsEdit`, node id `ContentItem`, display type `DetailAdmin`:

| Alternate | File |
|-----------|------|
| `BreadcrumbItem` | `BreadcrumbItem.cshtml` (base) |
| `BreadcrumbItem__ContentsEdit` | `BreadcrumbItem-ContentsEdit.cshtml` |
| `BreadcrumbItem__ContentItem` | `BreadcrumbItem-ContentItem.cshtml` |
| `BreadcrumbItem__ContentsEdit__ContentItem` | `BreadcrumbItem-ContentsEdit-ContentItem.cshtml` |
| `BreadcrumbItem_DetailAdmin` | `BreadcrumbItem.DetailAdmin.cshtml` |
| `BreadcrumbItem_DetailAdmin__ContentsEdit` | `BreadcrumbItem-ContentsEdit.DetailAdmin.cshtml` |
| `BreadcrumbItem_DetailAdmin__ContentItem` | `BreadcrumbItem-ContentItem.DetailAdmin.cshtml` |
| `BreadcrumbItem_DetailAdmin__ContentsEdit__ContentItem` | `BreadcrumbItem-ContentsEdit-ContentItem.DetailAdmin.cshtml` |

Alternates keyed on `[Id]` are only produced when the node set an `Id`. This is why a globally injected node (e.g. the dashboard) sets `.Id("Dashboard")` — so a theme can restyle just that node via `BreadcrumbItem-Dashboard.cshtml` regardless of which trail it appears in.

## Name/id encoding

`EncodeAlternateElement` maps a name/id to its alternate token:

- `.` → `_`
- `-` → `__`

So a trail named `My.Trail` contributes `Breadcrumb__My_Trail`. Keep trail names and node ids simple (letters/digits) to keep alternate file names readable.

## The default templates

`Breadcrumb.cshtml` emits a Bootstrap 5 breadcrumb — `<nav aria-label>` → `<ol class="breadcrumb">` → one `<li class="breadcrumb-item">` per node (`BreadcrumbItem.cshtml`) — and then, on admin, the page title below it: a heading tag (`h1` by default, from the shape's `Heading`) holding the current node's text, with class `oc-breadcrumb-title`. The current node's `<li>` gets `active` + `aria-current="page"`; a node with an href renders an `<a>`, otherwise plain text. Because the trail and the title are produced together here, `Breadcrumb.cshtml` (or its `__[Name]` / `_[DisplayType]` alternates) is the single override point for their arrangement — e.g. drop the `oc-breadcrumb-title` block to let the trail stand in for the title, or wrap the current node inline instead. Bootstrap's breadcrumb variables control the `›` separator and link color — override them in theme CSS rather than the template when only styling changes.

## Front end usage

The system is not admin-only. On a front-end view, render `<breadcrumb name="…" />`; it defaults to display type `Detail` and **no** heading (the page owns its own `<h1>`). Register a provider that reacts to that trail name exactly as on the admin. Use the `Detail` display-type alternates to give the theme its own markup if the default Bootstrap structure doesn't fit.

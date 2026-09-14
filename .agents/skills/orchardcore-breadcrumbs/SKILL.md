---
name: orchardcore-breadcrumbs
description: Renders and extends the breadcrumb trail that replaces the title on OrchardCore admin screens (and optionally on the front end). Use when the user needs to add a breadcrumb to a view, write or change an IBreadcrumbProvider, add a node to a trail owned by another module, resolve a parent entity for a nested editor, or theme a breadcrumb through shape alternates.
---

# OrchardCore Breadcrumbs

This skill guides you through OrchardCore's breadcrumb system following project conventions.

A breadcrumb is a **named trail** rendered by the `<breadcrumb>` tag helper in a view. Every `IBreadcrumbProvider` registered on the tenant is called for *every* trail; a provider decides which trail it contributes to by its name, and appends nodes to the shared builder. This is how one module adds a node to a trail owned by another module (e.g. `OrchardCore.AdminDashboard` prepends a "Dashboard" node to every admin trail). On the admin the trail **replaces the page `<h1>`** and feeds the page title; on the front end it renders without a heading.

## Mental model

```
<breadcrumb name="ContentsEdit" data="…" />          ← view (front end concern)
        │
        ▼
IBreadcrumbManager  ──asks every──▶  IBreadcrumbProvider(s)  ──append──▶  BreadcrumbBuilder (nodes)
        │                                                                        │
        ▼                                                                        ▼
order by position → resolve urls → auth → mark last node current      Breadcrumb shape + BreadcrumbItem shapes → HTML
```

- **Trail name** — a stable string (e.g. `ContentsEdit`). Declared as a `const` and passed to `<breadcrumb name="…">`. Providers match on it.
- **Provider** — `IBreadcrumbProvider` / `NamedBreadcrumbProvider`. Adds `BreadcrumbItem` nodes to the builder for the trail(s) it recognizes.
- **Node** (`BreadcrumbItem`) — text + optional link (`Url`/`Action`) + `Position` + `Id` + `Permission`s. The manager never links the *current* (last) node, and renders a node the user can't reach as plain text so the trail stays complete.
- **Data** — the contextual object the view passes (`data="@(new { ContentItem = item })"`). Providers read it via `builder.GetData<T>(key)` / `TryGetData`.
- **Shapes** — the trail renders as a `Breadcrumb` shape containing one `BreadcrumbItem` shape per node, so a theme can override presentation via alternates.

## Naming convention (do this first)

- **Reuse the AdminList name.** A screen's trail name must equal the `Name` its admin list uses, so the list and its breadcrumb never diverge. The content list trail is `Contents` — the same string as its admin list name.
- **Declare names on a `*Constants` class**, never inline strings. The class name **must end in `Constants`**. Reuse an existing `{Module}Constants` if the module already has one (often in the module's `.Core`/abstractions project — same or ancestor namespace, so no extra `using`); only add a new file when none exists. If a `{Module}Constants` name collides with a framework type (e.g. `Microsoft.AspNetCore.Cors.Infrastructure.CorsConstants`), use `{Module}BreadcrumbConstants` instead.
- Put both the trail-name consts (`List`, `Create`, `Edit`, `Display`, …) and the `data` keys (`ContentItemKey = "ContentItem"`, …) on that class, with XML doc comments describing which trail carries which data.

```csharp
public static class ContentsConstants
{
    /// <summary>The breadcrumb of the content items list. It carries <see cref="ContentTypeKey"/>.</summary>
    public const string List = "Contents";      // == the admin list name
    public const string Edit = "ContentsEdit";  // carries ContentItemKey
    public const string ContentItemKey = "ContentItem";
}
```

## Workflow A: add a breadcrumb to a view

### Step 1: Render the trail in the Title zone

Replace the `<h1>`/title with the tag helper, inside the `Title` zone. Pass the entities the trail needs as `data`.

```razor
<zone Name="Title">
    <breadcrumb name="@ContentsConstants.Edit" data="@(new { ContentItem = contentItem })" />
</zone>
```

- Each property of `data` becomes a `builder.Data` entry keyed by the property name — match the provider's `*Key` consts exactly.
- `heading` — override the wrapping tag of the current node. Defaults to `h1` on admin, none on the front end. `heading=""` renders no heading.
- `page-title` — defaults `true`; the current node's text is added as a page-title segment.
- `display-type` — defaults `DetailAdmin` on admin, `Detail` elsewhere. Rarely set by hand.

### Step 2: Do NOT leave a second heading

Because the current node becomes the `<h1>` on admin, remove any old `<h1>`/`<h5>` title the view rendered, or the title shows twice.

### Step 3: `@using` the constants

Add the module namespace to the view's `_ViewImports.cshtml` (or the view) so `@SomeConstants.Edit` resolves — the Razor SDK type-checks these at build time.

## Workflow B: write a provider

### Step 1: Inherit the right base

- **`NamedBreadcrumbProvider`** — contributes to a *single* trail. Pass the name to `base(...)`; implement `BuildAsync`. Best for a one-screen trail.
- **`IBreadcrumbProvider`** — implement directly and `switch (builder.Name)` when one module owns several related trails (list/create/edit/display).

### Step 2: Add nodes

```csharp
public sealed class ContentsBreadcrumbProvider : IBreadcrumbProvider
{
    internal readonly IStringLocalizer S;
    public ContentsBreadcrumbProvider(IStringLocalizer<ContentsBreadcrumbProvider> s) => S = s;

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
        => builder.Name switch
        {
            ContentsConstants.List => BuildListAsync(builder),
            ContentsConstants.Edit => BuildEditAsync(builder),
            _ => ValueTask.CompletedTask,      // ignore trails this provider doesn't own
        };

    private ValueTask BuildEditAsync(BreadcrumbBuilder builder)
    {
        // Parent node → links back to the list.
        builder.Add(S["Manage Content"], item => item
            .Id("Contents")
            .Action("List", "Admin", new { area = "OrchardCore.Contents" })
            .Permission(CommonPermissions.ListContent));

        // Current node → no link needed; the manager strips the last node's href.
        builder.TryGetData<ContentItem>(ContentsConstants.ContentItemKey, out var ci);
        builder.Add(S["Edit {0}", ci?.DisplayText], item => item.Id("ContentItem"));

        return ValueTask.CompletedTask;
    }
}
```

Node builder API: `.Text` / `.Id` / `.Position` / `.Url` / `.Action(action, controller, values)` / `.Permission(...)` / `.Permissions(...)` / `.Resource(...)` / `.AddClass`. Prefer `.Action(...)` over hand-built `.Url(...)` so path base and routing are handled.

### Step 3: Register the provider

```csharp
// In the module's Startup.ConfigureServices
services.AddBreadcrumbProvider<ContentsBreadcrumbProvider>();
```

`AddBreadcrumbs()` (which registers the `IBreadcrumbManager`) and the `Breadcrumb`/`BreadcrumbItem` shape table providers live in `OrchardCore.Navigation`'s `Startup` — a module only registers its own providers.

## Workflow C: a nested editor must lead back through its parent

Every child screen reachable only from a parent entity must show that parent in its trail. The child view passes the **parent id** in `data`; the provider loads the parent's name from that id using an injected module service.

```csharp
// view: data="@(new { MenuId = menuId, ... })"
private async ValueTask AddNodesAsync(BreadcrumbBuilder builder)
{
    var menuId = builder.GetData<string>(AdminMenuConstants.MenuIdKey);
    if (string.IsNullOrEmpty(menuId)) return;

    var menu = _adminMenuService.GetAdminMenuById(await _adminMenuService.GetAdminMenuListAsync(), menuId);
    builder.Add(S["Edit Nodes for '{0}'", menu?.Name], item => item
        .Id("Nodes")
        .Action("List", "Node", new RouteValueDictionary(s_routeValues) { { "id", menuId } })
        .Permission(AdminMenuPermissions.ManageAdminMenu));
}
```

Inject whatever loads the parent by id: `IContentManager`, `ISession`/YesSql, `IAdminMenuService`, `ISitemapManager`, `IRateLimitPolicyStore`, `IEnumerable<IDeploymentStepFactory>`, etc. Show the parent's **real display name**, not a generic "Edit Step". If a lookup fails, fall back gracefully (skip the node or use a derived name) — a throwing provider is logged and ignored, dropping only that provider's nodes.

## Workflow D: inject a node into every trail (cross-module)

A provider that reacts to *every* trail can prepend/append a global node. Use position `start` (sorts before all) or `end`. Gate on context so it only appears where it should. This is the mechanism behind the extensibility guarantee — other modules gain the node when the feature is enabled and lose it when disabled, knowing nothing about it.

```csharp
public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
{
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext is null || !AdminAttribute.IsApplied(httpContext))
        return ValueTask.CompletedTask;   // dashboard is the admin root only

    builder.Add(S["Dashboard"], "start", item => item
        .Id("Dashboard")
        .Url("~/" + _adminOptions.AdminUrlPrefix)
        .Permission(Permissions.AccessAdminDashboard));

    return ValueTask.CompletedTask;
}
```

## Positions

`FlatPositionComparer` orders nodes: `start` < `before` < numbers (`0`,`1`,`1.1`,`2`,`10` — numeric, dot inserts between) < `after` < free text < `end`. Equal positions keep insertion order. Most nodes leave `Position` null (treated as `0`) and rely on insertion order; use `start`/`end` for cross-cutting nodes.

## Theming: shape alternates

Node `Id` is **not** an HTML `id` — it only feeds alternates and lets another provider find/remove the node. Override a template by dropping a `.cshtml` in a theme (most specific wins):

- Whole trail: `Breadcrumb.cshtml` → `Breadcrumb__[Name]` (`Breadcrumb-ContentsEdit.cshtml`) → `Breadcrumb_[DisplayType]` (`Breadcrumb.DetailAdmin.cshtml`) → `Breadcrumb_[DisplayType]__[Name]`.
- One node: `BreadcrumbItem.cshtml` → `BreadcrumbItem__[Name]` → `BreadcrumbItem__[Id]` (`BreadcrumbItem-Dashboard.cshtml`) → `BreadcrumbItem__[Name]__[Id]` → their `_[DisplayType]` variants.

`.` and `-` in a name/id are encoded (`.`→`_`, `-`→`__`) by `EncodeAlternateElement`. See `references/theming.md`.

## Gotchas

- **Data key mismatch.** `data="@(new { ContentItem = x })"` must match the provider's `GetData<T>("ContentItem")`. A typo silently yields `default`, so the node's text/parent goes missing.
- **Second heading.** Remove the view's old `<h1>`/`<h5>`; the current node already renders the heading on admin.
- **Trail name ≠ admin list name.** Diverging names is the bug this convention exists to prevent — reuse the list `Name`.
- **Don't link the last node.** The manager clears the current node's href; you can still add an `.Action(...)`, it's just ignored for the current node.
- **Provider must be registered** with `AddBreadcrumbProvider<T>()` or the trail renders nothing (the tag helper suppresses output for an empty trail — a common "why is it blank" cause).
- **`Constants` class collisions.** Merge into an existing `{Module}Constants` (in `.Core`) rather than creating a duplicate; rename to `{Module}BreadcrumbConstants` only to dodge a framework type.
- **Permissions, not removal.** Give a node `.Permission(...)` rather than omitting it for unauthorized users — the manager renders it as plain text so the trail stays whole.

## References

- `references/theming.md` — full alternate tables, display types, front-end vs admin, overriding a node template
- `src/OrchardCore/OrchardCore.Navigation.Core/` (repo) — `BreadcrumbBuilder`, `BreadcrumbItem(Builder)`, `IBreadcrumbProvider`, `NamedBreadcrumbProvider`, `IBreadcrumbManager`, `BreadcrumbManager`
- `src/OrchardCore.Modules/OrchardCore.Navigation/` (repo) — `BreadcrumbShapes`, `BreadcrumbAlternatesFactory`, `Views/Breadcrumb.cshtml`, `Views/BreadcrumbItem.cshtml`
- `src/docs/reference/modules/Navigation/README.md` (repo) — official breadcrumb reference
- `AGENTS.md` (repo root) — build commands

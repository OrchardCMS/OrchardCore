# Admin List Conversion Plan

Bring every **eligible** admin index (CRUD listing) screen in OrchardCore onto the `AdminList` shape,
so the whole admin has one look and feel and one set of user-configurable layouts.

- **Branch**: `admin-list-layouts`
- **Reference PR**: "Add switchable admin list layouts (List, Table, Grid)" (`c6df6d7040`)
- **Skill to follow**: `.agents/skills/orchardcore-admin-index-views/SKILL.md`
- **Docs to keep current**: `src/docs/reference/modules/Admin/README.md`, `src/docs/releases/4.0.0.md`

**Scope**: 36 eligible screens (3 already done, 33 to convert) and 9 screens explicitly excluded.
Part A is the work. Part B is the exclusion list, with the reason each screen is out.

---

## 0. Note to the implementer — one commit per UI

Convert **one** screen at a time and do not start the next one until the current one is pushed.

1. Convert the single screen (driver, columns class, controller, view).
2. Build and run the affected unit tests.
3. Run the full visual validation in section 5 against `http://localhost:5010`.
4. Tick that screen's row in the section 6 tracker, in the same commit.
5. **Commit and push that one screen on its own.** One screen = one commit, so each conversion can be
   reviewed, reverted or bisected independently.
   - Message: `Convert the {Screen} admin list to the AdminList shape`
   - Do not batch two screens into one commit, even trivial ones like Sitemap Cache and Media Cache.
6. Only then move to the next screen.

If a screen turns out to need a shared change (a new shape property, a CSS rule, an abstraction),
land that shared change as its **own** commit first, then the screen conversion on top of it.

If a screen turns out to be harder than its tier suggests, or its interaction cannot survive
Table/Grid, stop and move it to Part B with a written reason rather than forcing the conversion.

---

## 1. Definition of done (per screen)

A screen is **converted** only when every box below is ticked.

| # | Requirement | How to verify |
|---|-------------|---------------|
| 1 | Rows come from a display driver as a `SummaryAdmin` shape — no hard-coded `<li>` row markup | `@await DisplayAsync(Model.List)` is the only row rendering in the view |
| 2 | Every visible piece of a row (checkbox, title, badges, meta, buttons, menu items) lives in its own shape placed in a **zone** | Row template has no literal markup other than zone rendering |
| 3 | A `{Module}AdminList` static class declares `Name` + `GetDefaultColumns(IStringLocalizer)` | File exists next to the module root |
| 4 | Controller injects `IAdminListService` via `[FromServices]` and builds the `AdminList` shape with `Name`, `Layout`, `Columns`, `Rows`, `Toolbar`/`Header`, `Pager`, `EmptyMessage` | Controller `Index`/`List` action |
| 5 | Rows carry `Classes.Add("item")` and `Attributes["data-filter-value"]` when the page uses client-side search | Controller loop over row shapes |
| 6 | Action bar uses `row gx-2` (**not** `gx-3`) | `grep gx-3` returns nothing for the view |
| 7 | Search box is an `input-group has-search` with a visible **Go** button (`name="submit.Filter"`) | View markup |
| 8 | Hidden `submit.Filter` button stays **first** in the form (Enter key) | View markup |
| 9 | Bulk actions still work: `#select-all` + inputs named `itemIds` | Manual click-through |
| 10 | Row actions render through `AdminListActions`, so the Buttons/Menu setting applies | Row `Actions` zone template |
| 11 | Renders correctly in **List**, **Table** and **Grid** layouts, and in **Buttons** and **Menu** action modes | Playwright validation, section 5 |
| 12 | Column provider extension point documented if the module exposes one | README |

### Row template conventions

The **Manage Content** list is the reference for how a row looks. Match it, not the earlier conversions:

| Zone | Renders as | Do not |
|------|-----------|--------|
| `Content` | The title as a **plain link** to the edit page, normal weight | `<h5>`, `<strong>`, or any heading — it makes the row shout next to Manage Content |
| `Tags` | One `<span class="badge ta-badge fw-normal">` per fact, with a leading `<i>` icon and a `title` tooltip naming the fact | A bare `<span class="hint">`, plain text, or a parenthetical |
| `Meta` | Badges too (date, author) | Plain text |
| `Actions` / `ActionsMenu` | Through `AdminListActions`. Menu items are **bare** `<a>` or `<button>` with `.dropdown-item` | Wrapping menu items in `<li>` — see below |

> **Never wrap ActionsMenu items in `<li>`.** `AdminListActions` renders the zone inside a
> `<div class="dropdown-menu">`, and the `List` layout puts the whole row inside an `<li>`. A `<div>` does
> not stop the HTML parser closing an open `<li>`, so nested `<li>` items are hoisted out and render as
> stray bullets after the row. Symptom: the dropdown is empty and its items appear at the bottom of the
> list. Every module already emits bare items except `RateLimits` (fixed during its conversion) and
> `ContentLocalization/Views/LocalizationPart.SummaryAdminLinks.cshtml` — check that one when its list is
> converted.

> **Known debt**: `IndexProfile.Fields.SummaryAdmin.cshtml` (Indexes, already shipped) and
> `RewriteRule.Fields.SummaryAdmin.cshtml` (URL Rewriting) both use `<h5>` for the title and so do not
> match Manage Content. Fix both to a plain link — Indexes as its own follow-up commit, URL Rewriting
> as part of its conversion.

### Anti-patterns to remove while converting

- `<ul class="list-group with-checkbox">` plus a hand-written `<li class="list-group-item text-bg-theme">`
  header row containing `select-all` / `items` / `selected-items` / `#actions` dropdown.
  Replaced by the `AdminListToolbar` shape (or the options editor `Header`).
- `int startIndex = (Model.Pager.Page - 1) * Model.Pager.PageSize + 1;` computed in the view.
  Moves to the controller and is passed to `AdminListToolbar`.
- `<div class="has-search">` without `input-group` and without the Go button.
- `float-end` / `float-start` positioning inside rows — breaks Table and Grid.
- Naming the rows property `Items` on the shape — silently renders nothing, it must be `Rows`.

---

## 2. Conversion recipe

1. **Driver** — if the entity has no `DisplayDriver<T>`, write one and register it in `Startup`.
   Split the row into zone shapes: `Checkbox`, `Content`, `Tags`, `Meta`, `Actions`, `ActionsMenu`.

   > **The model type name is the shape type.** `DisplayManager<TModel>` uses `typeof(TModel).Name`
   > verbatim and offers no override, so a driver registered against `FooViewModel` produces
   > `FooViewModel_SummaryAdmin` and forces theme authors to override
   > `FooViewModel-SummaryAdmin.cshtml`. Register the driver against a domain model named exactly what
   > the shape should be called (`ShapePlacement`, not `ShapePlacementViewModel`), adding one under
   > `Models/` when only a view model exists. Missing templates surface as
   > `InvalidOperationException: The shape type '...' is not found for the theme 'TheAdmin'`.
2. **Row template** — `{Entity}.SummaryAdmin.cshtml` renders zones only. The actions zone uses
   `@await DisplayAsync(await New.AdminListActions(Row: Model))`.
3. **Columns** — add `{Module}AdminList.cs` with `Name` and `GetDefaultColumns(S)`.
4. **Controller** — build the row shapes, then the `AdminListToolbar`, then the `AdminList` shape.
   Pass `HttpContext.RequestAborted` to both `IAdminListService` calls.
5. **View** — keep only the `<zone Name="Title">`, the form, the action bar (`gx-2` + Go button),
   `@await DisplayAsync(Model.List)`, the no-results alert, any modals, and the
   `<script asp-name="list-management" at="Foot">`.
6. **Validate** — section 5.
7. **Tick the tracker, commit, push** — section 0.

---

# Part A — Eligible screens

The admin route is the path after the admin prefix (default `/Admin`).

## A.0 — Already converted (reference implementations)

| Screen | Route | View | Notes |
|--------|-------|------|-------|
| Manage Content | `Contents/ContentItems/{type?}` | `OrchardCore.Contents/Views/ContentsAdminList.cshtml` | `ContentsAdminList.cs`, `Header` pattern |
| Users | `Users` | `OrchardCore.Users/Views/UsersAdminList.cshtml` | `UsersAdminList.cs`, `Header` pattern |
| Indexes | `indexing` | `OrchardCore.Indexing/Views/Admin/Index.cshtml` | `IndexingAdminList.cs`, `Toolbar` pattern — **closest template for Tier 1/2** |

## A.1 — Rows already driver-rendered (low effort)

These already call `@await DisplayAsync(entry.Shape)` per row. The driver exists; the work is moving
the checkbox and buttons into zones, adding the columns class, and rewriting the view.

| # | Screen | Route | View | Driver | What is missing |
|---|--------|-------|------|--------|-----------------|
| 1.1 | URL Rewriting | `UrlRewriting` | `OrchardCore.UrlRewriting/Views/Admin/Index.cshtml` | `RewriteRulesDisplayDriver` — already has `Fields`/`Buttons`/`DefaultMeta`/`DefaultTags` shapes | `gx-3`→`gx-2`, Go button, `Checkbox` zone, hand-written toolbar `<li>`, `UrlRewritingAdminList.cs`, controller wiring. **Blocked on decision 3**: the rules are a drag-sortable pipeline (`sortingListManager`, `#rewrite-rules-sortable-list`, `SortRulesEndpoint`) and the rows container differs per layout, so the shape needs a sortable-container hook first. Bulk-action inputs are named `ruleIds`, not `itemIds` |
| 1.2 | Queries | `Queries` | `OrchardCore.Queries/Views/Admin/Index.cshtml` | `QueryDisplayDriver` plus `SqlQueryDisplayDriver`, `LuceneQueryDisplayDriver`, `ElasticsearchQueryDisplayDriver` | `gx-3`→`gx-2`, Go button, `Checkbox` zone, new `Tags` zone for the source badge, hand-written toolbar `<li>`, `QueriesAdminList.cs`, controller wiring. No sortable, no naming variance — **start here**. Per-source row templates only apply in the `List` layout |
| 1.3 | ~~Placements~~ | | | | **Re-tiered to A.2 #2.16** — its rows are hard-coded `<li>` markup, not driver shapes. The survey grep that put it here matched `DisplayAsync(Model.Pager)`, not a row shape |
| 1.4 | Rate Limits | `RateLimits` | `OrchardCore.RateLimits/Views/Admin/Index.cshtml` | `RateLimitPolicyDisplayDriver` — has `ActionsMenuItems`, `DefaultMeta`, `Buttons` | Same. The page also lists limiters nested under each policy — the nested list stays as-is |
| 1.5 | Audit Trail | `AuditTrail` | `OrchardCore.AuditTrail/Views/AuditTrailAdminList.cshtml` | `AuditTrailEventDisplayDriver` — has `Meta`/`Tags`/`Actions` shapes | `Header` (options editor) pattern; `gx-3`→`gx-2`; add the Go button to `AuditTrailAdminListSearch.cshtml`; no bulk actions |
| 1.6 | Notifications | `notifications` | `OrchardCore.Notifications/Views/NotificationsAdminList.cshtml` | `NotificationDisplayDriver` | `Header` pattern like Contents — pass `Header` instead of `Toolbar`; `gx-3`→`gx-2`; Go button goes in the search shape |

## A.2 — Hard-coded rows, standard bulk-action list (medium effort)

All share the same shape: `list-group with-checkbox`, a hand-written toolbar `<li>`, hard-coded row
`<li>`, `@await DisplayAsync(Model.Pager)`. Each needs a **new display driver** plus zone shapes.

| # | Screen | Route | View | New driver needed for |
|---|--------|-------|------|-----------------------|
| 2.1 | **OpenID Applications** | `OpenId/Application` | `OrchardCore.OpenId/Views/Application/Index.cshtml` | `OpenIdApplicationEntry`. Currently the least consistent page in the admin: no sticky `action-bar`, no search box, no bulk actions, no `data-list-management`, buttons floated inside the row. Needs the full treatment |
| 2.2 | **OpenID Scopes** | `OpenId/Scope` | `OrchardCore.OpenId/Views/Scope/Index.cshtml` | `OpenIdScopeEntry`. Identical gaps to 2.1 |
| 2.3 | Templates | `Templates` | `OrchardCore.Templates/Views/Template/Index.cshtml` | `TemplateEntry` — shared by site and admin templates |
| 2.4 | Shortcode Templates | `Shortcodes` | `OrchardCore.Shortcodes/Views/Admin/Index.cshtml` | `ShortcodeTemplateEntry`. Note: the existing `ShortcodeDescriptorDisplayDriver` serves the shortcodes *reference table*, not this list |
| 2.5 | Admin Menus | `AdminMenu` | `OrchardCore.AdminMenu/Views/Menu/List.cshtml` | `AdminMenu` entry. This is the flat list of menus — the node tree editor behind it is **not eligible**, see B.2 |
| 2.6 | Media Profiles | `MediaProfiles` | `OrchardCore.Media/Views/MediaProfiles/Index.cshtml` | `MediaProfile` |
| 2.7 | Feature Profiles | `TenantFeatureProfiles` | `OrchardCore.Tenants/Views/FeatureProfiles/Index.cshtml` | `FeatureProfile` |
| 2.8 | Background Tasks | `BackgroundTasks` | `OrchardCore.BackgroundTasks/Views/BackgroundTask/Index.cshtml` | `BackgroundTaskEntry` — rows show schedule and enabled state, good `Tags`/`Meta` candidates |
| 2.9 | Deployment Plans | `DeploymentPlan` | `OrchardCore.Deployment/Views/DeploymentPlan/Index.cshtml` | `DeploymentPlan`. Only the plans **index** — the sortable step list inside a plan is **not eligible**, see B.5 |
| 2.10 | Remote Instances | `Deployment/RemoteInstance` | `OrchardCore.Deployment.Remote/Views/RemoteInstance/Index.cshtml` | `RemoteInstance` |
| 2.11 | Remote Clients | `Deployment/RemoteClient` | `OrchardCore.Deployment.Remote/Views/RemoteClient/Index.cshtml` | `RemoteClient` |
| 2.12 | Sitemaps | `Sitemaps/List` | `OrchardCore.Sitemaps/Views/Admin/List.cshtml` | `Sitemap` |
| 2.13 | Sitemap Indexes | `SitemapIndexes/List` | `OrchardCore.Sitemaps/Views/SitemapIndex/List.cshtml` | `SitemapIndex` |
| 2.14 | Workflow Types | `Workflows/Types` | `OrchardCore.Workflows/Views/WorkflowType/Index.cshtml` | `WorkflowType`. The workflow **designer** is not a list and is out of scope, see B.6 |
| 2.15 | Tenants | `Tenants` | `OrchardCore.Tenants/Views/Admin/Index.cshtml` | `ShellSettingsEntry` — partially shape-based already (`TenantActionTags`, `TenantActionButtons`), so promote those into zones rather than writing them from scratch |
| 2.16 | Placements | `Placements` | `OrchardCore.Placements/Views/Admin/Index.cshtml` | `ShapePlacementViewModel`. Moved here from A.1 once the rows turned out to be hard-coded. Only one field (the shape type), so the list is Select / Shape type / Actions |

## A.3 — Simple lists, no pager and/or no bulk actions (low to medium effort)

Client-side search only. They still need the action bar normalisation and the `AdminList` shape so
the layout setting applies; the toolbar can omit bulk actions.

| # | Screen | Route | View | Notes |
|---|--------|-------|------|-------|
| 3.1 | Roles | `Roles` | `OrchardCore.Roles/Views/Admin/Index.cshtml` | Hard-coded rows, `data-filter-value` already present, system-role badge becomes a `Tags` zone |
| 3.2 | Content Types | `ContentTypes/List` | `OrchardCore.ContentTypes/Views/Admin/List.cshtml` | Hard-coded. `data-type-name` is read by JS — preserve it as a row attribute |
| 3.3 | Content Parts | `ContentTypes/ListParts` | `OrchardCore.ContentTypes/Views/Admin/ListParts.cshtml` | Same as 3.2 |
| 3.4 | Recipes | `Recipes` | `OrchardCore.Recipes/Views/Admin/Index.cshtml` | Hard-coded, rows grouped by feature |
| 3.5 | Sitemap Cache | `SitemapsCache/List` | `OrchardCore.Sitemaps/Views/SitemapCache/List.cshtml` | Very small list |
| 3.6 | Media Cache | `MediaCache` | `OrchardCore.Media/Views/MediaCache/Index.cshtml` | No action bar at all today |
| 3.7 | Workflow Instances | `Workflows/Types/{id}/Instances` | `OrchardCore.Workflows/Views/Workflow/Index.cshtml` | Has a pager and bulk actions but no search bar today — add one |
| 3.8 | List Part contents | inside a List content item | `OrchardCore.Lists/Views/ListPartDetailAdmin.cshtml` | Rows are already `Content SummaryAdmin` shapes, so reuse the `ContentsAdminList` columns. Drag-ordering is optional here (`EnableOrdering`) — when it is on, force the `List` layout. See decision 3 |

## A.4 — Eligible but high effort (do last)

| # | Screen | Route | View | Why it is more work |
|---|--------|-------|------|---------------------|
| 4.1 | Themes | `Themes` | `OrchardCore.Themes/Views/Admin/Index.cshtml` | Renders a Bootstrap card grid via `ThemeEntryDisplayDriver`, not a list. The `Grid` layout is exactly this presentation, so it converts cleanly — but it needs a per-list default layout so it keeps looking like cards. See decision 1 |
| 4.2 | Features | `Features/{tenant?}` | `OrchardCore.Features/Views/Admin/Features.cshtml` | Server-rendered (`@foreach`), so it is convertible. But rows are grouped by category with a per-category select-all, checkboxes are named `featureIds` (not `itemIds`), and each row carries dependency badges, "always enabled" tooltips and enable/disable state. Plan: one `AdminList` per category, a `FeaturesAdminList` with `Select`/`Name`/`Dependencies`/`State`/`Actions` columns, and the bulk-action name kept as `featureIds` |
| 4.3 | Layers | `Layers` | `OrchardCore.Layers/Views/Admin/Index.cshtml` | Convert the **Layers list pane only**. The widget-by-zone pane on the same page is **not eligible**, see B.3. The layers list is sortable, so decision 3 applies |

---

# Part B — Not eligible

These screens are excluded from the plan. Each is a purpose-built UI whose interaction model is not a
row-per-record list, so forcing it into `AdminList` would remove function rather than add consistency.
Do **not** convert them. Where a cosmetic fix is safe and worthwhile it is noted; anything more needs a
new decision recorded here first.

| # | Screen | Route | View | Why it is not eligible | Cosmetic fix still worth doing |
|---|--------|-------|------|------------------------|-------------------------------|
| B.1 | Media Library | `Media` | `OrchardCore.Media/Views/Admin/Index.cshtml` | A file explorer: folder tree, thumbnail grid, upload dropzone, multi-select drag and drop. Records are files on a provider, not entities with drivers | None |
| B.2 | Admin Menu Nodes | `AdminMenu/Node` | `OrchardCore.AdminMenu/Views/Node/List.cshtml` | A hierarchical tree editor built on `jQuery.nestedSortable` (`<ol id="menu">`). Rows nest and re-parent by drag; a flat column layout cannot express depth. The parent **Admin Menus** list (A.2 #2.5) is eligible | Normalise the "Add Node" card to the standard action bar |
| B.3 | Layers — widget/zone pane | `Layers` | `OrchardCore.Layers/Views/Admin/Index.cshtml` | Widgets grouped by theme zone with drag-and-drop between zones. It is a placement canvas, not a list. The **Layers list** on the same page is eligible (A.4 #4.3) | None |
| B.4 | Data Localization | `DataLocalization` | `OrchardCore.DataLocalization/Views/Admin/Index.cshtml` | A Vue-rendered `<table>` of translation strings with inline editing, client-side grouping by provider and its own filter. Rows are edited in place, never navigated to | Normalise the search input to `input-group has-search` + Go |
| B.5 | Deployment plan steps | `DeploymentPlan/{id}` | `OrchardCore.Deployment/Views/DeploymentPlan/Display.cshtml` | A sortable step pipeline (`#stepOrder`, `ui-sortable-handle`) inside an edit page. Order is the data; a Table/Grid layout would break reordering. The plans **index** is eligible (A.2 #2.9) | None |
| B.6 | Workflow designer | `Workflows/Types/{id}` | `OrchardCore.Workflows/Views/WorkflowType/Edit.cshtml` | An activity graph canvas with connectors. Not a list at all | None |
| B.7 | CORS Policies | `Cors` | `OrchardCore.Cors/Views/Admin/Index.cshtml` | A Vue master/detail SPA: the list (`v-for="policy in policies"`) is client-rendered from a JSON model and swapped for a `<policy-details>` editor component in place. There is no server-side row to drive | Normalise the search input to `input-group has-search` + Go |
| B.8 | GraphQL | `GraphQL` | `OrchardCore.Apis.GraphQL/Views/Admin/Index.cshtml` | Hosts the GraphiQL explorer. No records | None |
| B.9 | Dashboard | `dashboard/manage` | `OrchardCore.AdminDashboard/Views/Dashboard/Index.cshtml` | A resizable widget canvas | None |

> If a screen in Part A proves to belong here during implementation, move its row into this table with
> a written reason and update the tracker in section 6. Do not silently skip it.

---

## 4. Execution order

One screen per commit, pushed before the next starts (section 0). The order lets each batch reuse the
previous batch's learning.

**Batch A — prove the recipe on driver-backed lists**
1.2 Queries → 2.16 Placements → 1.4 Rate Limits → *(sortable-container hook, own commit)* → 1.1 URL Rewriting

> URL Rewriting moved to the end of Batch A: it is a drag-sortable pipeline, so it needs the shared
> sortable-container hook (decision 3) landed first. Queries leads instead — same driver-backed
> structure, no special behaviour.

**Batch B — the `Header`-pattern lists**
1.5 Audit Trail → 1.6 Notifications

**Batch C — the worst offenders, highest visible win**
2.1 OpenID Applications → 2.2 OpenID Scopes

**Batch D — standard bulk-action lists, the same recipe 13 times**
2.3 Templates → 2.4 Shortcodes → 2.5 Admin Menus → 2.6 Media Profiles → 2.7 Feature Profiles →
2.8 Background Tasks → 2.9 Deployment Plans → 2.10 Remote Instances → 2.11 Remote Clients →
2.12 Sitemaps → 2.13 Sitemap Indexes → 2.14 Workflow Types → 2.15 Tenants

**Batch E — simple lists**
3.1 Roles → 3.2 Content Types → 3.3 Content Parts → 3.4 Recipes → 3.5 Sitemap Cache →
3.6 Media Cache → 3.7 Workflow Instances → 3.8 List Part contents

**Batch F — high effort**
4.1 Themes (decision 1 first) → 4.2 Features → 4.3 Layers (decision 3 first)

**Batch G — close-out**
Cosmetic fixes on the Part B screens that have one, docs, release notes, a sweep for any remaining
`gx-3` action bar, unit tests.

---

## 5. Visual validation protocol

Run after **each** screen, before the commit. Local site on **port 5010**.

```bash
dotnet run --project src/OrchardCore.Cms.Web -- --urls http://localhost:5010
```

Then, with the Playwright MCP server:

1. Navigate to the screen's admin route (Part A) and sign in as admin.
2. `browser_snapshot` — confirm the action bar, search box, Go button, toolbar, rows and pager.
3. Set **Layout = List** in `Settings → Admin` (`/Admin/Settings/admin`), reload, screenshot.
4. Set **Layout = Table**, reload, screenshot. Confirm every column header appears and no cell is
   empty that should not be.
5. Set **Layout = Grid**, reload, screenshot. Confirm the header lines up with the data.
6. Set **Row actions = Menu**, reload, confirm the kebab menu contains every action.
7. Set **Row actions = Buttons**, reload, confirm the button group.
8. `browser_resize` to 480x900 — confirm the container query stacks the columns and nothing overflows.
9. Type in the search box and press Enter, then click **Go** — confirm both filter.
10. If the list has bulk actions: tick `select-all`, confirm the count label and the Actions dropdown.
11. `browser_console_messages` — no new errors.

Record the outcome in section 6. Reset the layout setting to **List** before starting the next screen.

---

## 6. Progress tracker

Legend: ☐ not started · ◐ in progress · ☑ converted, validated and pushed

| Screen | Driver | Columns | Controller | View | List | Table | Grid | Buttons | Menu | Pushed |
|--------|:------:|:-------:|:----------:|:----:|:----:|:-----:|:----:|:-------:|:----:|:------:|
| Manage Content | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| Users | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| Indexes | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| 1.1 URL Rewriting | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 1.2 Queries | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| 2.16 Placements | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| 1.4 Rate Limits | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| 1.5 Audit Trail | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ | ☑ |
| 1.6 Notifications | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.1 OpenID Applications | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.2 OpenID Scopes | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.3 Templates | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.4 Shortcode Templates | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.5 Admin Menus | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.6 Media Profiles | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.7 Feature Profiles | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.8 Background Tasks | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.9 Deployment Plans | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.10 Remote Instances | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.11 Remote Clients | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.12 Sitemaps | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.13 Sitemap Indexes | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.14 Workflow Types | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 2.15 Tenants | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.1 Roles | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.2 Content Types | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.3 Content Parts | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.4 Recipes | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.5 Sitemap Cache | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.6 Media Cache | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.7 Workflow Instances | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 3.8 List Part contents | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 4.1 Themes | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 4.2 Features | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |
| 4.3 Layers | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ |

Part B screens are not tracked here. Their optional cosmetic fixes land in Batch G.

---

## 7. Open decisions

Resolve each before the batch that needs it, and record the answer here.

1. **Themes (4.1)** — the theme cards must keep looking like cards. Convert with `Grid` as the
   per-list default (needs decision 2), or leave the page bespoke and move it to Part B?
   *Decision: pending.*
2. **Per-list default layout** — should `AdminListOptions` support a default per list name
   (Themes → Grid) rather than one site-wide default?
   *Decision: pending.*
3. **Sortable lists** — URL Rewriting (1.1), List Part ordering (3.8) and the Layers list (4.3) are
   drag-sortable. The rows container differs per layout (`<ul>`, `<tbody>`, `.admin-list-grid-body`),
   so `sortingListManager.create(selector)` has no stable target today.
   Investigated during the Queries conversion: SortableJS computes `oldIndex`/`newIndex` by counting
   only siblings matching its `draggable` selector (`.item`), and the toolbar row never carries
   `.item`, so **the indices stay correct in all three layouts** — the only missing piece is a stable
   container hook. Proposal: add a `RowsAttributes` (or `RowsCssClass`) property to the `AdminList`
   shape, rendered on the rows container in all three layouts, so the page can mark it sortable.
   Land that as its own commit before 1.1.
   *Decision: pending.*
4. **Toolbar vs Header** — screens with an options editor use `Header`, the rest use
   `AdminListToolbar`. Confirm we are not consolidating these into one before Batch D starts.
   *Decision: pending.*
5. **Features bulk-action input name (4.2)** — the Features page posts `featureIds`, every other list
   posts `itemIds`. Keep `featureIds` and parameterise the `list-management` script, or rename and
   update the controller?
   *Decision: pending.*

---

## 8. Cross-cutting work (Batch G)

- [ ] Sweep for any remaining `row gx-3` inside an `.action-bar` and change it to `gx-2`.
- [ ] Sweep every `*.Fields.SummaryAdmin.cshtml` for `<h5>`/heading titles and replace with a plain
      link, per the row template conventions in section 1. Known: Indexes, URL Rewriting.
- [ ] Sweep every row template for `<span class="hint">` used to convey a fact (source, type, state)
      and convert it to a `ta-badge`.
- [ ] Sweep for `has-search` without `input-group` or without a Go button, including the Part B
      screens listed as having a cosmetic fix (B.2, B.4, B.7).
- [ ] Confirm `_admin-list.scss` container queries cover every new column set, and that no layout
      gained a viewport-based `d-md-none`.
- [ ] `src/docs/reference/modules/Admin/README.md` — document every converted list name and its
      default columns, so module authors know the names to target with `IAdminListColumnProvider`.
      Also document the Part B exclusion list so nobody re-opens it.
- [ ] `src/docs/releases/4.0.0.md` — list the converted screens and note the breaking change for
      anyone overriding the old row templates.
- [ ] Unit tests: extend `DefaultAdminListServiceTests` and `AdminListShapeTableProviderTests` with a
      test per new list name asserting the default columns.
- [ ] Note in the release doc that per-type row templates (for example
      `Content-BlogPost.SummaryAdmin.cshtml`) only apply in the `List` layout; customising Table and
      Grid uses `AdminListCell-{List}-{Column}.cshtml`.

---

## 9. Risks

| Risk | Mitigation |
|------|------------|
| Bulk actions silently break when the checkbox moves into a zone | Keep `#select-all` and `name="itemIds"` verbatim; step 10 of the validation protocol |
| Client-side search stops matching | Set `data-filter-value` on the **row shape**, not in the layout template |
| A theme or site overrode a row template that no longer renders | Call it out in the release notes; the old template still works under the `List` layout |
| Sortable lists (List Part, Layers) break under Table/Grid | Open decision 3 |
| A module adds columns twice because its provider is registered twice | `AddAdminListColumnProvider<T>` already uses `TryAddEnumerable` — do not register manually |
| A batched commit makes one screen's regression hard to isolate | One screen per commit, pushed before the next starts — section 0 |

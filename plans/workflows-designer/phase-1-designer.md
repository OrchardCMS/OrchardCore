# Phase 1 — New workflow designer

Replace the jsPlumb editor and the full-page activity editors with a single-page designer: a toolbox on the left, a canvas in the middle, and a properties panel on the right. It autosaves to a draft and publishes explicitly. Engine behavior doesn't change (decisions D1–D8 in [`README.md`](README.md)).

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
┌──────────────────────────────────────────────────────────────────────────────────────┐
│ Workflows › Order approval      ● Draft saved 12:04   [Undo][Redo]  [Discard] [Publish]│
├────────────┬─────────────────────────────────────────────────────────┬───────────────┤
│ Toolbox    │                                                         │ Properties    │
│ [search…]  │     ┌──────────────┐        ┌──────────────┐            │ [Activity|    │
│ ▸ Content  │     │⚡ Content     │ Done ─▶│  Notify      │            │  Workflow|    │
│ ▸ HTTP     │     │  Published   │        │  "Thanks…"   │            │  Issues]      │
│ ▸ Primitive│     └──────────────┘        └──────────────┘            │               │
│ ▸ Timers   │                                                         │ <server-      │
│ …          │                                 [−] 100% [+] [Fit]      │  rendered     │
│            │                                                         │  editor form> │
└────────────┴─────────────────────────────────────────────────────────┴───────────────┘
```

- Drag an activity from the toolbox onto the canvas (or click it to add it at the center of the view). If it has an editor, the panel opens.
- Draw a connection by dragging from an outcome port onto a target activity.
- Select an activity to edit it in the panel. Changes apply in place, and the node summary and outcomes update without navigating away.
- Every change autosaves to the draft. **Publish** makes the draft live; **Discard** reverts to the live definition.
- The instance details page reuses the same canvas in read-only mode.

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 1.0 Spike: validate the two riskiest assumptions (throwaway code, not committed)

Time-box this step and record the findings in **Spike findings** at the bottom of this file. That record is the only thing committed.

1. **Injected editors work.** On a scratch admin page, load each of these editors over AJAX into a container and check that it initializes, posts back and binds:
   - `NotifyTask` (plain)
   - `IfElseTask` (syntax toggle)
   - `ScriptTask` (Monaco)
   - `HttpRequestEvent` (URL generator; needs `data-workflow-type-id` / `data-activity-id` on a wrapper)
   - `EmailTask` (CodeMirror, from `OrchardCore.Email`)
   - `CreateContentTask` (from `OrchardCore.Contents`)

   Use the Flows pattern (`OrchardCore.Flows/Controllers/AdminController.cs` `BuildEditor` and `Views/Admin/Display.cshtml`, then `@orchardcore/bloom/helpers/evalScripts`). Record:
   - which scripts initialize only at top level (they need step 1.3);
   - whether stylesheets have to be collected too;
   - whether loading the same editor twice double-initializes;
   - whether Monaco/CodeMirror instances leak when the container is replaced.
2. **The in-house canvas performs well enough.** In a minimal Vue component, render 200 nodes plus 300 SVG edges in a transformed layer. Dragging a node and panning must stay smooth (no visible stutter on a mid-range laptop). If it fails, record that and switch decision D2 to "jsPlumb inside Vue" before step 1.5.

---

### - [x] 1.1 Server: workflow type drafts

Goal: a server-side draft for each workflow type, which the designer autosaves into. Publishing goes through `IWorkflowTypeStore` so every `IWorkflowTypeEventHandler` runs. This also fixes the store bypass in `ActivityController`.

- **Model** — `M/Models/WorkflowTypeDraft.cs` (module-internal; Phase 2 promotes it into versioning):
  - `long Id`, `string WorkflowTypeId`, `int Revision`, `DateTime CreatedUtc`, `DateTime ModifiedUtc`, `string ModifiedByUserId`, `string ModifiedByUserName`.
  - A copy of the editable type data: `Name`, `IsEnabled`, `IsSingleton`, `LockTimeout`, `LockExpiration`, `DeleteFinishedWorkflows`, `IList<ActivityRecord> Activities`, `IList<Transition> Transitions`.
- **Index and migration** — `M/Indexes/WorkflowTypeDraftIndex.cs` (`WorkflowTypeId`). Register it in `M/Startup.cs`, and add `UpdateFrom4Async` in `M/Migrations.cs` (the schema is at version 4 today) to create the index table.
- **Service** — `M/Services/IWorkflowTypeDraftManager.cs` plus a `sealed` implementation:
  - `GetAsync(workflowTypeId)` returns the draft, or null.
  - `GetOrCreateAsync(workflowType)` creates the draft as a deep copy of the live type (clone the `Properties` JsonObjects; `InstantiateActivity` shares the instance today).
  - `SaveGraphAsync(workflowTypeId, expectedRevision, positions/start flags/transitions/removed ids)` returns a result: `Saved(newRevision)`, `Conflict(currentRevision, modifiedBy, modifiedUtc)` or `NotFound`.
  - `AddActivityAsync(workflowTypeId, expectedRevision, activityName, x, y)` generates the id with `IActivityIdGenerator`, applies the existing rule `IsStart = isEvent && !HasStartActivity()` (`A/Helpers/WorkflowTypeExtensions.cs`), and returns the record plus the new revision.
  - `UpdateActivityAsync(workflowTypeId, expectedRevision, activityId, JsonObject properties)` replaces properties and drops transitions whose `SourceOutcomeName` the activity no longer produces. It reports which transitions it removed.
  - `UpdateSettingsAsync(...)` covers the `EditProperties` fields.
  - `PublishAsync(workflowTypeId, expectedRevision)` copies the draft into the live `WorkflowType`, calls `IWorkflowTypeStore.SaveAsync` and deletes the draft. It returns the validation issues (below).
  - `DiscardAsync(workflowTypeId)`.
  - `ValidateAsync(draftOrType)` returns `IList<WorkflowDesignIssue {Severity (Error|Warning), Code, Message, ActivityId?, TransitionKey?}>`:
    - missing start activity (warning; today's rule);
    - activity type not registered (`MissingActivity`, warning);
    - transition whose source or destination activity doesn't exist (error; removed on save);
    - outcome with more than one outgoing transition (warning: "only the first is followed", because the engine uses `FirstOrDefault`);
    - activity unreachable from any start activity (warning).
- **Lifecycle**:
  - Delete the draft when its type is deleted. Implement `IWorkflowTypeEventHandler.DeletedAsync` (look at how `M/Http/Handlers/WorkflowTypeRoutesHandler.cs` registers).
  - Delete the draft when the recipe step `WorkflowType` (`M/Recipes/WorkflowTypeStep.cs`) replaces the type.
  - `Duplicate` creates no draft.
  - Export and deployment (`M/Deployment/AllWorkflowTypeDeploymentSource.cs`) export the **live** type only. Formats don't change (D7).
- **Tests** (`test/OrchardCore.Tests/Modules/OrchardCore.Workflows/Designer/WorkflowTypeDraftManagerTests.cs`): use Moq'd `IWorkflowTypeStore` and YesSql where needed (or a `SiteContext` scope).
  - creating a draft deep-copies (editing the draft doesn't mutate the live type);
  - revision increments;
  - a stale revision conflicts;
  - adding the first event sets start;
  - updating properties removes transitions for vanished outcomes (use `ForkTask` with `Forks` changed);
  - publish calls `IWorkflowTypeStore.SaveAsync` once and deletes the draft;
  - discard;
  - each validation rule;
  - deleting a type deletes its draft;
  - a recipe import replacing a type deletes its draft.

### - [x] 1.2 Server: designer JSON API

Goal: everything the designer needs, as an `[Admin]` MVC controller returning JSON, with shapes rendered on the server.

- **Shared mapping**: extract the graph-to-JSON and outcome computation from `WorkflowTypeController.Edit` (`M/Controllers/WorkflowTypeController.cs:382-459`) into a `sealed` service, `M/Services/WorkflowDesignerModelBuilder.cs`. It builds the throwaway `WorkflowExecutionContext` and calls `GetPossibleOutcomesAsync` per activity. It also builds `Activity_Design` / `Activity_Thumbnail` shapes through `IActivityDisplayManager` and renders them to HTML strings.
- **Controller**: `M/Controllers/WorkflowDesignerController.cs`, with `[Admin("Workflows/Types/{workflowTypeId}/Designer/{action}", "WorkflowDesigner{action}")]` (check for route clashes with `ActivityController`'s `Workflows/Types/{workflowTypeId}/Activity/...`). Every action checks `WorkflowsPermissions.ManageWorkflows` and returns a 403 ProblemDetails (`ProblemDetailsApiControllerExtensions`). Responses use camelCase JSON. Unsafe verbs are POST, so the global `AutoValidateAntiforgeryTokenAttribute` applies; don't opt out.

  | Action | Request | Response |
  |---|---|---|
  | `GET Definition` | — | `{ workflowTypeId, revision, hasDraft, draftModifiedBy, draftModifiedUtc, settings{…}, nodes:[{id, name, x, y, isStart, isEvent, hasEditor, title, designHtml, outcomes:[{name, displayName}]}], transitions:[{sourceActivityId, sourceOutcomeName, destinationActivityId}], issues:[…], runningInstanceCount }`. Reads the draft if one exists, otherwise the live type. |
  | `GET Library` | — | `{ categories:[{name, activities:[{name, displayText, category, isEvent, hasEditor, thumbnailHtml, icon}]}] }` |
  | `POST Save` | `{ revision, nodes:[{id, x, y, isStart}], transitions:[…], removedActivityIds:[…] }` | `{ revision, issues }`, or 409 ProblemDetails with `{ currentRevision, modifiedBy, modifiedUtc }` |
  | `POST AddActivity` | `{ revision, name, x, y }` | `{ revision, node }` |
  | `GET Editor?activityId=` | — | `{ content, scripts, styles }` |
  | `POST Editor?activityId=&revision=` | the editor's form (`FormData`, multipart like `EditActivity.cshtml`) | valid: `{ valid:true, revision, node, removedTransitions }`; invalid: `{ valid:false, content, scripts, styles }` (rendered with ModelState errors) |
  | `GET Settings` / `POST Settings` | the `EditProperties` fields | same pattern as `Editor` |
  | `POST Publish` | `{ revision }` | `{ issues, publishedUtc }`, or 409 |
  | `POST Discard` | — | `{ }` |

- **Fragment view**: `M/Views/WorkflowDesigner/Fragment.cshtml` (`Layout = null`). It works like `OrchardCore.Flows/Views/Admin/Display.cshtml`, but **also collects registered stylesheets**. It wraps editors in `<div data-workflow-type-id=… data-activity-id=…>` so `workflow-url-generator.ts` keeps working. Use `htmlPrefix ""` (the panel shows one editor at a time, and the field ids fixed in #19961 assume it).
- **No-editor activities**: `AddActivity` just adds them; nothing writes during a GET (unlike today's `ActivityController.Create`).
- **Tests** (`test/OrchardCore.Tests/Modules/OrchardCore.Workflows/Designer/WorkflowDesignerControllerTests.cs`, with `SiteContext` and `EnableFeaturesAsync(context, "OrchardCore.Workflows", "OrchardCore.Workflows.Http")`):
  - 403 without `ManageWorkflows` (through the `PermissionsContext` header);
  - `Definition` returns the live graph when there's no draft, and the draft graph after a save;
  - a stale revision returns 409;
  - `AddActivity` adds an activity that has no editor;
  - `Editor` GET returns content and a non-empty `scripts` for `ScriptTask`;
  - `Editor` POST with an invalid Liquid `IfElseTask` returns `valid:false` with the error in `content`;
  - `Editor` POST changes `ForkTask.Forks` and the outcomes update;
  - `Publish` registers the `HttpRequestEvent` route (assert through the HTTP route entries service) and the draft disappears;
  - `Discard` works;
  - a POST without an antiforgery token is rejected.
- **Notes from implementing this step:**
  - `Activity.Design.cshtml` and `Activity.Thumbnail.cshtml` wrap the activity's own shape with legacy canvas/picker markup (absolute positions, links to `ActivityController`). The model builder still builds `Activity_Design` / `Activity_Thumbnail` through `IActivityDisplayManager`, but renders only their `Content` zone, which holds `{Name}_Fields_Design` / `{Name}_Fields_Thumbnail`. Overrides of the activity's own shapes keep working; overrides of the legacy wrappers don't apply in the designer.
  - Route entries are only registered for `HttpRequestFilterEvent` start activities. `HttpRequestEvent` is invoked through its signed token URL (`workflows/invoke/{token}`), which resolves the activity on the live type. The publish test asserts both: the filter event's route entry appears, and the token URL returns 404 before publishing and responds after.
  - Publishing a draft with `Error` issues returns 400 ProblemDetails with an `issues` extension.

### - [x] 1.3 Make every activity editor safe to inject

Goal: every editor initializes correctly when injected into the panel, more than once, without leaks. This is the same class of problem `plans/es-module-migration.md` addressed for Flows and Widgets.

- **Convert top-level initialization to `observeAndInit`** (`.scripts/bloom/helpers/observeAndInit.ts`) and make it idempotent: mark initialized elements, for example with `data-oc-initialized`.
  - In `M/Assets/Scripts`: `correlate-task.ts`, `liquid-task.ts`, `http-request-task.ts`, `http-response-task.ts`.
  - In other modules: the editor scripts of `OrchardCore.Email` (EmailTask), `OrchardCore.Contents` (Create/UpdateContentTask) and `OrchardCore.Tenants` (Create/SetupTenantTask).
  - Plus anything else the spike found.
  - Find them all with: `grep -rl "Fields.Edit.cshtml"` under each module's `Views/Items`, then inspect their `<script>` tags.
- `workflow-syntax-toggle.ts` and `workflow-monaco-text-editor.ts` already use `observeAndInit`. Verify them anyway.
- **Disposal**: before the panel replaces editor content, the designer dispatches a `oc:editor-unmounting` DOM event on the container. Monaco and CodeMirror initializers in Workflows listen for it and dispose their instances (expose this from bloom if it doesn't exist yet). Document the event for third-party activity authors in the module docs (step 1.12).
- **Keep the full-page editors working** until step 1.11 removes them. Both paths must work in this step.
- **Tests**: covered end to end in step 1.13. Add Vitest specs for any bloom helper you add.
- **Notes from implementing this step:**
  - The new bloom helper `helpers/editorLifecycle.ts` provides `EDITOR_UNMOUNTING_EVENT`, `dispatchEditorUnmounting`, `onEditorUnmounting` and `bindCodeMirrorToTextArea`. The binding keeps the textarea in sync, fires a `change` on the textarea when the editor loses focus after an edit (like a native field), and calls `toTextArea()` on unmount. `components/monaco-text-editor.ts` does the same for Monaco and disposes the editor and its model.
  - Once an editor script has run in the designer, its `observeAndInit` registration stays active for the rest of the page. Broad selectors such as `textarea[id$='Body']` (Sms) or `textarea[id$='HtmlBody']` (Notifications) would then also match other editors (EmailTask). So each module editor now has a `data-task-editor` wrapper (`email`, `sms`, `notification`, `content`, `create-tenant`, `setup-tenant`), and its script only looks inside it. Overrides of those `*.Fields.Edit.cshtml` views must keep the wrapper (release notes, step 1.12).
  - `content-type-check-all.js` is now `Assets/ts/content-type-check-all.ts`, built by Parcel into `Scripts/content-type-check-all/` and loaded as a module.
  - bloom has its own Vitest setup (`.scripts/bloom/vitest.config.ts`, run in `frontend_unit_tests.yml`).
  - Verified in a running CMS, both injected over AJAX (switching between editors and back) and on the legacy full pages. Every editor initialized each time, with no leftover CodeMirror or Monaco instances and no console errors.

### - [x] 1.4 Front-end scaffold

- **Create `M/Assets/designer/`**, mirroring `OrchardCore.Media/Assets/media-gallery`:
  - `package.json`: name `@orchardcore/workflows-designer`, private, scripts `check` (`vue-tsc --noEmit`) and `test:unit` (`vitest`).
  - `vite.config.ts`: lib mode, ES output into `../../wwwroot/Scripts/Workflows/designer/`, `vue` aliased to `vue/dist/vue.esm-bundler.js`, the `@bloom` alias, `@vitejs/plugin-vue`, postcss-rtlcss. Don't set `build.minify`; the assets-manager plugin minifies.
  - `tsconfig.json` extending `.scripts/tsconfig.base.json` and including `.scripts/bloom/ambient.d.ts`.
  - `vitest.config.ts` and `vitest.setup.ts` (jsdom, junit to `testing/vitest-results.xml`).
- **Register the bundle in `M/Assets.json`**: `{ "action": "vite", "name": "workflows-designer", "source": "Assets/designer", "tags": ["admin", "workflows"] }`.
- **Register the resource** `workflows-designer` (script, with `.SetAttribute("type","module")`, plus a style) in `M/ResourceManagementOptionsConfiguration.cs`. Use the Media manifest as the reference.
- **Localization**: add `M/Services/WorkflowsDesignerJSLocalizer.cs : IJSLocalizer` (group `workflows-designer`) and register it in `M/Startup.cs`. The pattern is `OrchardCore.Cors/Services/CorsJSLocalizer.cs`.
- **App skeleton** (`src/`):
  - `main.ts`: mounts on `#workflow-designer` and reads the config JSON from a `data-config` attribute:
    - endpoint URLs built server-side with `Url.Action`, so they're tenant-prefix aware;
    - `workflowTypeId`;
    - `readOnly`;
    - `instancesUrl`, `exportUrl`, `listUrl`;
    - translations.
  - `api/designerApi.ts`: typed client on bloom `createApiService({ authType: "cookie" })`.
  - `state/designerStore.ts`: module-level reactive store (no Pinia; follow Media's `Globals.ts`) holding the graph, selection, viewport, revision, save status and issues.
  - `state/history.ts`: command-pattern undo/redo, capped at 100 entries. Commands: add, remove, move (coalesce drags), connect, disconnect, setStart. Editor applies are recorded as a non-undoable boundary, because server-side properties can't be reverted locally.
  - `App.vue`: layout shell with the toolbar, toolbox, canvas and panel.
- **Mount in a new view**: `M/Views/WorkflowType/Designer.cshtml`. The existing `Edit.cshtml` stays the default until step 1.11. Until then the new designer is reachable at `WorkflowDesigner/Index` (add that GET action to the controller from step 1.2), so both can be compared.
- **Tests**: Vitest specs for history (undo/redo, drag coalescing, cap) and for the store's serialization to the `Save` payload. Add a `M/Assets/designer` step to `.github/workflows/frontend_unit_tests.yml`.
- **Notes from implementing this step:**
  - `vue` is **not** aliased to `vue/dist/vue.esm-bundler.js`. The designer only uses precompiled SFCs, and the runtime-only build halves the bundle (152 kB instead of 302 kB minified).
  - **Undo across autosave.** Once a removal is autosaved, the server no longer has the activity, so undoing it locally would lose its properties. The draft therefore keeps removed activities in `WorkflowTypeDraft.RemovedActivities` (the last 100; never published), and `Save` takes `restoredActivityIds`. The store compares the graph with the activity ids the server last had: missing ones go in `removedActivityIds`, re-appeared ones in `restoredActivityIds`. This is draft-only state, so the persisted formats (D7) don't change.
  - Editor applies replace the node and call `History.markBoundary()`, which clears the undo/redo stacks.
  - **For step 1.8:** every mutating request (Save, AddActivity, Editor and Settings posts, Publish) takes the current revision and returns the next one, so autosave must send them one at a time through a single queue.

### - [x] 1.5 Canvas

Files: `src/canvas/` — `DesignerCanvas.vue`, `ActivityNode.vue`, `OutcomePort.vue`, `TransitionEdge.vue`, `geometry.ts`, `viewport.ts`, `useDrag.ts`, `useConnect.ts`.

- **Coordinate system**: node `x`/`y` are canvas pixels at zoom 1, the same meaning as `ActivityRecord.X/Y` today, so existing workflows lay out identically. The viewport is `{ panX, panY, zoom }`, applied as one CSS `transform` on the layer that holds both the HTML nodes and the SVG edges.
- **Nodes**:
  - Header: icon, display text and a start badge (events with `isStart`). Body: `designHtml` (trusted server output, rendered with `v-html`).
  - Event and task styling, a selected state, a `MissingActivity` warning style, and an issue badge (from `issues`).
  - Output ports: one per outcome, labelled with the outcome display name.
  - The whole node is the input target, as today.
- **Edges**: SVG cubic Bézier from the port to the closest side of the target node, with an arrow marker and the outcome label. They have hover and selected states plus a delete affordance. Self-loops aren't allowed (the engine skips them).
- **Interactions**:
  - Drag nodes with a 10px grid snap and multi-select (shift-click, marquee).
  - Pan by dragging the background, with the middle mouse button, or with space + drag.
  - Zoom with Ctrl+wheel and toolbar buttons, from 25% to 200%. Toolbar also has "fit to content".
  - Connect by dragging from a port onto a node.
  - Delete removes the selection; undo restores it, with a toast "Deleted N activities — Undo" (this replaces the commented-out confirm prompt, `workflow-editor.ts:271`).
  - Keyboard: Delete, Ctrl+Z / Ctrl+Y, Ctrl+A, arrow keys to nudge, Esc.
  - A context menu on a node offers: Edit, Set as start (events), Delete. On an edge: Delete.
- **One transition per outcome**: connecting an outcome that already has a transition **replaces** it, with a toast. Legacy graphs that already have duplicates show the warning issue from step 1.1.
- **Accessibility**: nodes are focusable with `role="group"` and an accessible name. Ports are buttons. Every action is reachable from the keyboard through the context menu, including "Connect outcome to…", which lists target activities.
- **Hooks for tests**: put `data-cy` attributes on nodes (`data-cy="activity-{id}"`), ports (`port-{activityId}-{outcome}`), edges, toolbar buttons and the panel.
- **Tests (Vitest)**:
  - `geometry.ts`: port anchor positions, Bézier control points, closest-side selection, hit testing;
  - `viewport.ts`: zoom around the cursor, clamping, fit to content;
  - connecting an outcome replaces the old edge; self-loops are rejected;
  - deleting removes attached edges;
  - component tests for node and edge rendering.
- **Notes from implementing this step:**
  - **Node size.** Nodes are 220 px wide, and the outcome ports sit in a column beside the body, not below it. That keeps nodes about as tall as in the legacy editor (60–125 px on the seeded ComingSoon "User Registration" workflow, with no overlaps at its stored positions).
  - **Gestures.** Dragging the background pans; **Shift+drag** on the background draws the marquee. The mouse wheel pans, and Ctrl/⌘+wheel zooms around the cursor. Activities are ordered by z-index in document order, newest last.
  - **Spike rules applied.**
    - `will-change: transform` is set on the viewport layer only while panning or zooming.
    - Dragged activities (up to 10) and their edges are lifted onto their own layers during a drag.
    - Pointer moves are coalesced into one update per animation frame (`useDrag.ts`).
    - The dot grid is a separate layer, translated by the pan modulo one cell, so panning never repaints it.
  - **Keyboard access.** The node context menu (context-menu key or Shift+F10) offers Edit, Set/Unset start, "Connect an outcome to…" (a dialog listing outcomes and target activities), one "Remove connection {outcome} → {target}" entry per connection, and Delete. Enter on a port opens the same dialog. Tabbing to an activity selects it.
  - **RTL.** The canvas rules are wrapped in `/* rtl:begin:ignore */ … /* rtl:end:ignore */` and the canvas uses `direction: ltr`, so `postcss-rtlcss` never mirrors the coordinate space.
  - **Verified** in a running CMS with synthetic pointer events on the seeded workflow: grid-snapped drag, connecting a port to a node (replacing the existing transition, with a toast), undo with Ctrl+Z, panning (`will-change` only during the pan) and Ctrl+wheel zoom. No console errors.

### - [x] 1.6 Toolbox

- `src/toolbox/ActivityToolbox.vue` replaces the `#activity-picker` modal and `activity-picker.ts`. Data comes from `GET Library`.
- It has a search box (display text and category), collapsible categories, and an Events/Tasks filter. Cards render `thumbnailHtml` plus an icon. Drag a card onto the canvas to add it at the drop point, or click to add it at the center of the view.
- **Icons**: use a default per category; activities can override it. Add an optional `Icon` (a Font Awesome class) to `ActivityRegistration` (`A/Options/ActivityRegistration.cs`) and an `AddActivity<TActivity, TDriver>(…, configure)` overload, so third parties can opt in without breaking. Set icons for the built-in Workflows activities only.
- Adding an activity calls `AddActivity`. If `hasEditor`, the panel then opens on the new activity.
- **Tests**: Vitest specs for filtering and search; a component test that dropping a card at a point calls the API with canvas coordinates (correct under pan and zoom).
- **Notes from implementing this step:**
  - **Icon registration.** `ActivityRegistration.Icon`, `WorkflowOptions.RegisterActivity(type, driver, configure)`, `WorkflowOptions.GetActivityRegistration(type)` and `services.AddActivity<TActivity, TDriver>(activity => activity.Icon = "fa-solid fa-star")`. All are additive.
  - **Icon resolution.** The icon is resolved on the server (`WorkflowDesignerIcons`): the registration's icon, else a default keyed by the category's resource name (`LocalizedString.Name`, so it doesn't depend on the culture), else a generic event or task icon. Missing activities get a warning icon. Both the toolbox (`Library`) and the nodes (`Definition`) carry it.
  - **Toolbox behavior.** Categories start collapsed and expand while a search or kind filter is active. Search splits the query into terms that must all match the display text or category, ignoring case and accents. Cards are buttons (Enter/Space adds) and use HTML drag and drop (`application/x-orchard-workflow-activity`).
  - **Placement.** A dropped activity is placed with its header centered on the pointer; a clicked one is centered in the visible area. Both positions snap to the grid.
  - **After adding.** The new activity is selected and focused. The add is undoable: undo removes it from the draft on the next save, and redo restores it from the draft trash. Since step 1.7, the panel opens on activities that have an editor.
  - **Verified** in a running CMS: icons, search, click-to-add and a real `DataTransfer` drop (position and icon), with the draft updated on the server.

### - [x] 1.7 Properties panel

Files: `src/panel/` — `PropertiesPanel.vue` (resizable and collapsible, with tabs **Activity**, **Workflow** and **Issues**), `ServerFormHost.vue`, `IssuesList.vue`.

- **`ServerFormHost.vue`** hosts server-rendered forms (activity editors and workflow settings):
  - Loads `{content, scripts, styles}`, injects the styles once per href, renders the content inside a `<form>`, and runs the scripts with bloom `evalScripts`.
  - Before replacing content, it dispatches `oc:editor-unmounting` (step 1.3).
  - **Applying**: on the `change` event (debounced 600 ms), on Apply, or when the selection changes or the panel closes, it posts the form as `FormData` to `POST Editor`.
    - If the result is invalid, it re-renders the returned content with errors and keeps the selection on that activity, asking before switching away.
    - If it's valid, it updates the node (`designHtml`, `title`, `outcomes`), removes `removedTransitions` from the graph with a toast, and stores the new `revision`.
- **Workflow tab**: hosts the `EditProperties` fields through `GET/POST Settings`.
- **Issues tab**: lists the `issues`. Clicking one selects and centers the activity.
- Double-clicking a node opens the Activity tab.
- **Tests (Vitest)**:
  - debounce and apply-on-blur behavior;
  - an invalid result keeps the selection;
  - removed transitions are applied;
  - styles are injected once;
  - `oc:editor-unmounting` is dispatched before replacing content.
- **Notes from implementing this step:**
  - **Ordered script loading.** Per the spike, the host doesn't use `evalScripts`. bloom gains `loadScripts(html, { target, waitFor })` and `loadStyles(html)` (`helpers/loadAssets.ts`):
    - scripts run one at a time in document order, and each external classic script is awaited (`async = false` plus its `load` event);
    - external scripts already on the page are skipped (by `src`), and inline scripts run every time;
    - stylesheets are added once (by `href`, or by text for inline `<style>`).
  - **Applying.** `input` marks the form dirty, and `change` applies it after 600 ms. Apply, a selection change, a tab switch or collapsing the panel applies pending input at once. A form with nothing pending isn't posted. Before building the `FormData`, the host dispatches a synthetic `submit` on the form, so Monaco and CodeMirror copy their values into their textareas.
  - **Invalid result.** The returned content is rendered with its errors and a status message. Leaving the activity or the tab asks through the admin `confirmDialog` ("Discard changes?", with Discard and Keep editing). Keep editing selects the activity again.
  - **Valid result.**
    - The node is replaced with the server's node, keeping its canvas position.
    - Transitions from removed outcomes are dropped from the graph, including unsaved local ones, with a warning toast.
    - The revision and issues are updated.
    - Property changes are applied on the server, so undo can't go back past them: applying clears the history (`History.markBoundary`, step 1.4). Undo therefore can't bring back a transition from an outcome that was removed.
  - **Panel.**
    - It is collapsible and resizable from 288 to 720 px, with the pointer or the arrow keys on its separator. The width is kept per browser in `localStorage`.
    - Missing activities and multiple selections show a message instead of a form.
    - In read-only mode, the Activity tab shows a summary of the selected activity (for step 1.9).
  - **Conflicts.** A 409 from `POST Editor` or `POST Settings` is reported to the app with a `conflict` event. A toast stands in until step 1.8 adds the conflict dialog.
  - **Server fix: shapes rendered outside a view.** The designer endpoints return JSON, so there is no view context. The first shape template of each request was therefore rendered as a main page and wrapped in the theme layout: a full admin page in the `designHtml` of the first node, the `thumbnailHtml` of the first toolbox card, and the node returned by `POST Editor`. `WorkflowDesignerModelBuilder` now renders shapes in a view context without output, as Liquid templates do, and restores the previous context afterwards. `RenderedShapes_JsonEndpoints_AreNotWrappedInTheLayout` covers it and fails without the fix.
  - **Verified** in a running CMS:
    - Monaco (Script Task) is created in the panel and disposed (0 editors and 0 models) when switching to the Liquid Task, whose CodeMirror editor loads.
    - The If/Else syntax toggle works. An invalid Liquid condition shows its error, and the confirm dialog works with both answers: Keep editing keeps the activity selected with its value, and Discard leaves the stored activity unchanged.
    - Editing Fork branches updates its ports and body.
    - Renaming the workflow on the Workflow tab updates the toolbar.
    - The Issues tab lists the warnings.
    - No new console errors on fresh pages, in either editor order.

### - [x] 1.8 Draft autosave, publish and discard

- **Autosave**:
  - `src/services/autosave.ts` debounces graph changes (800 ms) and keeps a single request in flight, coalescing pending changes.
  - It retries with backoff on network errors.
  - On 409 it stops autosaving and shows a conflict dialog: **Reload** discards local changes and reloads the draft; **Overwrite** re-saves with the server's revision.
- **Toolbar status**: Saved • Saving… • Unsaved changes • Offline/retrying • Conflict. The **Publish** button is enabled when there's a draft. **Discard** uses `window.confirmDialog`.
- **Publish flow**:
  - Errors block publishing. Warnings appear in a dialog with Publish anyway / Cancel.
  - If `runningInstanceCount > 0`, show: "N running instances will continue on the new definition." That's today's behavior; Phase 2 adds version pinning.
  - After publishing, show a toast and clear the draft state.
- **Draft banner**: when a draft exists and its last editor is someone else, show a banner: "Draft last edited by {user} at {time}."
- **Leave guard**: a `beforeunload` prompt while a save is pending or failed.
- **Tests (Vitest)**: coalescing, a single in-flight request, the retry schedule, conflict handling for both choices, and the publish dialog decision logic.
- **Notes from implementing this step:**
  - **One queue for every draft change.** `src/services/revisionQueue.ts` runs Save, AddActivity, the Editor and Settings posts, Publish, Discard and the reloads one at a time.
    - Each request gets the revision that is current when it starts.
    - A revision it returns becomes current inside the queue, before the next request starts. Otherwise a queued Save could read a stale revision while the panel was still handling an apply.
  - **Autosave** (`src/services/autosave.ts`):
    - Saves 800 ms after the last graph change, with one Save in flight at a time. Changes made meanwhile are saved together after it.
    - Network and 5xx errors are retried after 1, 2, 5 and 10 s, then every 30 s ("Not saved. Retrying…"). Other errors stop with "Not saved." and a Retry button.
    - `flush` saves pending changes right away (used by Publish).
    - `pause`/`resume` hold saves during Discard, keeping the changes if it fails.
  - **Conflicts.** A 409 from any request stops autosave and opens the conflict dialog, which "Resolve…" next to the toolbar status reopens.
    - **Reload** loads the draft again and drops the local changes and unapplied form edits.
    - **Overwrite** saves the graph with the draft's current revision, applies the unapplied form edits, then loads the result. Save replaces positions, start flags and connections but keeps the activities it doesn't list, so the other person's added activities and property changes stay, and are then shown.
  - **Publish.**
    - It applies the open form and saves pending changes first, then decides with `src/draft/publishDecision.ts`:
      - errors block it, with a dialog listing them (clicking one selects the activity);
      - warnings or running instances ask first, with Publish anyway (or Publish) and "N running instance(s) will continue on the new definition.";
      - otherwise it publishes right away.
    - A 400 with issues from the server opens the blocked dialog.
    - Publishing clears the draft state (revision 0) and the undo history, since the draft's trash is gone.
  - **Discard** asks through the admin `confirmDialog`, then loads the live definition.
  - **Draft banner.** It compares the draft's `draftModifiedByUserId` with the new `currentUserId` config value. Both come from the `NameIdentifier` claim. The banner hides once the user changes the draft and can be dismissed.
  - **Leave guard.** `beforeunload` prompts while a save is pending, retrying, failed or in conflict, while a request is queued, or while a form has unapplied changes.
  - **Dialogs.** `src/ui/ModalDialog.vue` uses Bootstrap modal markup with a focus trap, Esc to close, and focus restored on close. It is teleported to `<body>`, like the admin `confirmDialog`, so the backdrop also covers the admin navigation bar.
  - **Fix: stylesheet order.** The `workflows-designer` stylesheet now depends on `bootstrap`, so it loads after it. Before, Bootstrap won over the designer's single-class rules on the same elements; for example, issue rows rendered as underlined link buttons.
  - **Verified** in a running CMS:
    - Autosave after a keyboard move (one Save).
    - A second writer's save causing the conflict dialog.
    - Overwrite: one Save with the current revision, then positions are ours and the other writer's added activity appears.
    - Reload: the local move is dropped and their position is shown.
    - Publish with 15 warnings through the dialog: the draft is gone and both buttons are disabled.
    - A change after publishing starts a new draft at revision 1, and Discard works through the admin dialog.
    - Saves failing at the network level show "Not saved. Retrying…" with the leave guard on, then save on the retry.

### - [x] 1.9 Read-only instance viewer

- `M/Views/Workflow/Details.cshtml` mounts the same app with `readOnly: true` and an instance payload: the blocking activity ids and the status. Add a `GET` JSON action to `WorkflowController`, or embed the payload in `data-config`.
- Read-only mode hides the toolbox and the editing tools. Clicking a node shows a read-only summary: title, type and whether it's blocking.
- Highlight blocking activities as `workflow-viewer.ts` does today. **Executed-path highlighting waits for Phase 5**: `ExecutedActivities` isn't recorded yet, so add a visible TODO hook only, no engine change.
- Keep the existing State tab. Remove the stale `<script asp-name="bootstrap" version="4">` at `Details.cshtml:115`.
- **Tests**: covered in step 1.13.
- **Notes from implementing this step:**
  - **Endpoint.** `GET WorkflowDesigner/Instance?instanceId=` returns a `WorkflowDesignerDefinition` of the **live** workflow type, which the instance runs on, so a draft doesn't show. It also returns `instance`: id, `WorkflowId`, status name and `blockingActivityIds`. An instance of another type returns 404. `WorkflowController.Details` renders the config with `readOnly: true` and this URL as `urls.definition`.
  - **Shared config.** `WorkflowDesignerConfigBuilder` builds the `data-config` of both the designer and the viewer. The viewer only gets the definition URL.
  - **Viewer.**
    - No toolbox, save status, undo/redo, Publish or Discard.
    - The panel has the Activity tab only. It shows the activity's type, whether the instance waits on it, and its design body.
    - The canvas is read-only and fits the whole workflow on load.
    - Blocking activities get an info-colored ring and header and a "Blocking" badge, and are announced in their accessible name. The toolbar shows a legend.
    - The executed-path TODO hooks are on `WorkflowDesignerInstance`, the client `DesignerInstance` type and `DesignerCanvas` (Phase 5).
  - **Details page.** It keeps the details card, the State tab and the Back/Restart/Delete buttons. The stale Bootstrap 4 script, the jsPlumb stylesheet and the `workflow-viewer` script and stylesheet are no longer referenced; step 1.11 removes those assets. The Workflow tab label is now localized.
  - **For the release notes (1.12): breaking change for template overrides.**
    - `WorkflowViewModel.ActivityDesignShapes` and `WorkflowViewModel.WorkflowTypeJson` are replaced by `DesignerConfigJson`.
    - The `Activity_DesignReadOnly` shape (`Activity.DesignReadonly.cshtml`), which only the jsPlumb viewer used, is removed.
  - **Verified** in a running CMS:
    - An HTTP-started instance halted on a Signal event shows as Halted, with the Signal event highlighted.
    - Selecting it shows "Waiting on this activity".
    - The view is fitted to all 17 activities.
    - The State tab works, and there are no console errors.

### - [x] 1.10 Theming, RTL, accessibility and localization

- **Styling**: only `--oc-*` / `--bs-*` variables (`src/OrchardCore.Themes/TheAdmin/Assets/scss/_variables.scss`) and Bootstrap classes. Check light, dark and auto `data-bs-theme`. The canvas grid and edges must stay legible in both themes.
- **RTL**: the panel and toolbox mirror. The canvas coordinate space does **not** mirror; workflow layouts stay the same for every user. Document this.
- **Localization**: every client string goes through the `workflows-designer` JS localizer from step 1.4, and server messages use `IStringLocalizer`.
- **Accessibility**: keyboard-only walkthrough (add, connect, edit, delete, publish). Visible focus. `aria-live` for save status and toasts.
- **Notes from implementing this step:**
  - **Styling.**
    - Every color in the designer stylesheet and components is a `--bs-*` variable or a Bootstrap class.
    - In the dark theme, the grid dots, edges, labels and node text stay legible.
    - The blocking (info), event (warning), start (success), error (danger) and selection (primary) colors stay distinct.
    - The filled inactive panel tabs come from the admin theme's own `.nav-tabs` style, so they are kept for consistency with the other admin tabs.
  - **RTL.**
    - The toolbox and panel swap sides, their borders flip, and the panel's resize handle stays on the edge facing the canvas.
    - The canvas keeps `direction: ltr` and the same coordinates, so a workflow looks the same for every user. Step 1.12 documents this.
    - Directional icons outside the canvas (panel collapse, category carets, undo/redo) get `wfd-mirror-rtl` and flip in RTL.
  - **Localization.**
    - A Vitest spec checks that every `t("…")` key the sources use is defined in `WorkflowsDesignerJSLocalizer`, and that every defined key is used.
    - Server messages already go through `IStringLocalizer`. The instance page's Correlation ID fallback ("None") and Workflow tab label are now localized.
  - **Accessibility.**
    - The canvas region is a labeled `<section>` instead of `<main>`, since the admin layout has no main landmark of its own.
    - The panel tabs follow the ARIA tabs pattern: ids with `aria-controls` and `aria-labelledby`, one tab stop, arrow keys (mirrored in RTL), Home and End.
    - Editing an activity (Enter or double-click) moves the focus to its first field. Escape in the panel returns it to the activity, except inside Monaco or CodeMirror.
    - Error toasts use `role="alert"`, and the others `role="status"` inside a polite live region. The close button stays dark on warning toasts.
    - When a dialog closes on a control that is now disabled (Publish) or replaced (Reload, Discard), the focus goes to the canvas instead of being lost.
  - **Keyboard walkthrough (verified in a running CMS, real key presses):**
    - Add: Enter on a toolbox card adds the activity and focuses its node.
    - Connect: Shift+F10, then "Connect an outcome to…", choosing the target, then Connect. The connection is drawn and autosaved, and the focus is back on the node.
    - Edit: Enter focuses the Title field; typing then Escape returns to the node, and the change is applied on blur.
    - Delete: Delete removes the node and shows the undo toast.
    - Publish: Enter on Publish opens the dialog focused on its confirm button, Tab stays inside the dialog, Enter publishes, and the focus goes to the canvas.

### - [x] 1.11 Make the new designer the default and retire the legacy editor

- `WorkflowTypeController.Edit` (GET) renders the new designer. Remove the POST `Edit` that saves the canvas state, and `WorkflowTypeUpdateModel`; `WorkflowDesigner/Index` from step 1.4 can then redirect to `Edit`.
- **Old activity URLs**:
  - Replace `ActivityController.Create` / `ActivityController.Edit` with redirects to the designer that select the activity (`?activityId=`), so existing bookmarks still work.
  - Remove `M/Views/Activity/Create.cshtml` and `M/Views/Activity/EditActivity.cshtml`, plus the `sessionStorage` local-state handling (`localId`, `LoadLocalState`).
- **Remove the old editor code**:
  - Remove `workflow-editor.ts`, `activity-picker.ts`, `workflow-canvas.ts` and `workflow-viewer.ts`, with their `Assets.json` entries and resources (`workflow-editor`, `workflow-viewer`).
  - Remove the unused `M/Views/WorkflowType/RenderActivity.cshtml`.
  - Keep the `jsplumb` resource registered for third parties, with an "obsolete" comment and a note in the release notes (D7).
- **Keep these server shapes unchanged**: `Activity_Design`, `Activity_Thumbnail` and `Activity_Edit` (the designer renders them).
- Run `yarn build` and commit the `wwwroot` changes, including removal of obsolete output files.
- **Notes from implementing this step:**
  - **Edit.** `WorkflowType/Edit/{id}` renders the designer. The designer view moved from `Views/WorkflowType/Designer.cshtml` to `Edit.cshtml` (model `WorkflowDesignerViewModel`).
    - `?activityId=` selects that activity, centers it and opens its editor (the `initialActivityId` config value).
    - `WorkflowDesigner/Index` redirects to `Edit`, keeping `activityId`.
  - **Toolbar.** It gets the old editor's Instances link (with the running instance count) and Export. Export posts through the admin theme's `data-url-af="UnsafeUrl"` handler and exports the published workflow, not the draft, as its tooltip says. Properties moved to the Workflow tab in step 1.7.
  - **Old activity URLs.** `ActivityController` only redirects now:
    - `…/Activity/{activityId}/Edit` goes to `Edit/{id}?activityId=…`;
    - `…/Activity/{activityName}/Add` goes to `Edit/{id}`.
    - Their views, the POST actions and their `sessionStorage` local state (`localId`) are gone.
  - **Removed.**
    - Server: the POST `Edit`, `WorkflowTypeViewModel`, `WorkflowTypeUpdateModel`, `ActivityEditViewModel`, `RenderActivity.cshtml`, and the services `WorkflowTypeController` no longer uses.
    - Client: `workflow-editor.ts`, `activity-picker.ts`, `workflow-canvas.ts`, `workflow-viewer.ts` and `workflow-models.ts`, which only those used. Also the editor and viewer stylesheets (`orchard.workflows-editor.scss`, `orchard.workflows-viewer.scss`, `workflow-canvas.scss`), with their `Assets.json` entries, `workflow-editor`/`workflow-viewer` resources and `wwwroot` output.
    - Dependencies: the module's `bootstrap`, `@popperjs/core` and `@types/bootstrap`, which only those scripts used (`yarn.lock` updated).
  - **Kept.** The `jsplumb` script and `jsplumbtoolkit-defaults` style resources stay registered for third parties, with an obsolete comment; their `Assets.json` entries stay so the files are still built. The `Activity_Design`, `Activity_Thumbnail` and `Activity_Edit` shapes are unchanged.
  - **For the release notes (1.12).** These public types and views are removed or changed:
    - removed: `WorkflowTypeViewModel`, `WorkflowTypeUpdateModel`, `ActivityEditViewModel`;
    - `WorkflowType/Edit.cshtml` now uses `WorkflowDesignerViewModel`;
    - `Activity/Create` and `Activity/EditActivity` are removed;
    - the `workflow-editor` and `workflow-viewer` resources are removed.
  - **Verified** in a running CMS:
    - `Edit/10` shows the designer with Instances (1) and Export, and no jsPlumb.
    - `Designer/Index`, `Activity/{id}/Edit` and `Activity/{name}/Add` redirect to it.
    - The activity edit URL opens with that activity selected and its editor loaded.

### - [x] 1.12 Documentation and release notes

- **Module docs**: update `src/docs/reference/modules/Workflows/README.md`:
  - designer tour: toolbox, canvas, panel, issues, drafts and publish, keyboard shortcuts;
  - new screenshots in `src/docs/reference/modules/Workflows/docs/`;
  - **for activity authors**: the editor must be safe to inject (`observeAndInit`, idempotent, dispose on `oc:editor-unmounting`), and the optional `Icon` registration.

  Editing an existing page needs no `mkdocs.yml` change. A **new** page must go into `nav`, or `mkdocs build --strict` fails.
- **Release notes**: add to `src/docs/releases/4.0.0.md` under `### Workflows`:
  - **New features**: the designer, drafts, autosave and publish.
  - **Breaking changes**: the activity editor full pages are replaced by redirects; the `workflow-editor` and `workflow-viewer` resources are removed; the `jsplumb` resource is obsolete; third-party editor scripts must be safe to inject.
- **Notes from implementing this step:**
  - **Module docs.** `reference/modules/Workflows/README.md` gets a **Workflow Designer** section covering:
    - the three areas, the tabs and the collapsible rails;
    - drafts and publishing: save status, Publish and Discard, conflicts, the draft banner;
    - keyboard shortcuts;
    - right-to-left languages: the canvas isn't mirrored;
    - the instance viewer.
  - **Updated sections.** The Vocabulary (Workflow Designer, Activity Editor, Activities Pane, Transition) and "Developing Custom Activities" (what the display types render in the designer) are updated. Two sections are new: **Activity Icons** and **Activity Editors in the Designer**, the injectable-editor contract, with the bloom helpers and an example.
  - **Screenshots.** `docs/workflow-designer.png` and `docs/workflow-instance-viewer.png` replace `docs/workflow-editor.png`, which only the replaced section used. They were captured at 1600 × 1000 from a sample "Article approval" workflow, with a throwaway Playwright script that isn't committed.
  - **Release notes.** `releases/4.0.0.md` gets:
    - **Breaking changes › Workflows Module:** drafts and when `IWorkflowTypeEventHandler`s run, the removed pages, view models, views, shapes and resources, the obsolete `jsplumb` resources, template overrides, and the editor contract, with links to the docs;
    - **New features › Workflow Designer.**
  - **Verified** with `mkdocs build --strict`. No `mkdocs.yml` change was needed, since no page was added.

### - [x] 1.13 End-to-end tests (Playwright for .NET)

- **Setup**:
  - Fixture `WorkflowsDesignerTestsFixture : CmsRecipeFixture` in `test/OrchardCore.Tests.Functional/Tests/Cms/CmsRecipeFixture.cs`, with `RecipeName => "WorkflowsDesignerTests"`.
  - Recipe `test/OrchardCore.Tests.Functional/Fixtures/workflows-designer-tests.recipe.json`: `issetuprecipe: true`; enables `TheAdmin`, `OrchardCore.Workflows`, `OrchardCore.Workflows.Http`, `OrchardCore.Workflows.Timers` and `OrchardCore.Contents`; plus a `WorkflowType` step with one seeded workflow for the viewer and redirect tests.
  - Test class `Tests/Cms/WorkflowsDesignerTests.cs : CmsTestBase<WorkflowsDesignerTestsFixture>, IClassFixture<…>`.
  - Use `data-cy` selectors and `ConsoleHelper.CollectConsoleErrors()`; every test asserts there are no console errors.
- **Scenarios** (one test each, named `{Action}_{Condition}_{ExpectedResult}`):
  1. Open the designer for a new type: the toolbox, canvas and panel render, and the seeded layout positions match the stored `X`/`Y`.
  2. Drag "Content Published" from the toolbox: the node appears, is the start activity, and the panel opens.
  3. Add "Notify", connect `Done` to it, edit its message in the panel: the node summary updates without navigating away.
  4. Reload the page: the draft is still there (autosave). Publish, then reload: the published graph matches and the draft banner is gone.
  5. Discard reverts to the published definition.
  6. Delete with undo restores the node and its edges.
  7. Connecting an outcome that already has a transition replaces it (only one edge remains).
  8. Change `Fork` branches in the panel: the outcomes and ports update, and edges for removed branches go away.
  9. The `HttpRequestEvent` editor generates a URL in the panel. After publishing, the URL responds (the route was registered by the store handlers).
  10. Invalid `IfElse` Liquid shows the validation error in the panel; the selection stays.
  11. Two pages on the same draft: the second save gets the conflict dialog.
  12. The instance details page renders read-only with the blocking activity highlighted and no toolbox.
  13. The old URL `…/Activity/{id}/Edit` redirects to the designer with that activity selected.
  14. Dark mode (`data-bs-theme="dark"`) renders with no console errors.
  15. A user without `ManageWorkflows` gets 403 from the designer endpoints.
- **Canvas drags** use `DragDropHelper.DragAsync`. Add a port-to-node helper to `Helpers/` if needed.
- **Run locally** with the commands in `README.md`. Fix every server warning surfaced by `CmsTestBase.AssertNoLoggedIssues()`.
- **Notes from implementing this step:**
  - **Recipe.** `workflows-designer-tests.recipe.json`:
    - enables the features listed above (plus the base admin features);
    - adds a `WorkflowViewer` role that has `AccessAdminPanel` but not `ManageWorkflows`;
    - seeds "Seeded approval", an HTTP request → Notify → Signal "approve" → Notify workflow with known ids and positions.
  - **Helper.** `Helpers/WorkflowDesignerHelper.cs` covers:
    - creating a workflow type and finding one by name;
    - opening the designer;
    - adding an activity by dragging its toolbox card (an HTML drag, `DragToAsync`) to a point on the canvas;
    - connecting a port to an activity (`DragDropHelper.DragAsync`);
    - opening an activity's editor;
    - waiting for autosave;
    - publishing, confirming the dialog when there is one;
    - generating an HTTP event's URL.
  - **Setup.** Tests that change a workflow create their own; the seeded one is only read. Pages use a 1600 × 1000 viewport, since 1280 × 720 leaves about 350 px of canvas between the admin menu, the toolbox and the panel.
  - **The 15 scenarios** are one test each, and every test asserts no console errors. The conflict test allows only the browser's own "409" message for the rejected request. The test for a user without `ManageWorkflows` creates a user in the `WorkflowViewer` role and calls the JSON endpoints, which return 403.
  - **Gotcha.** The `data-cy` element of a dialog (`.wfd-modal-host`) has no box of its own, because the modal inside is fixed, so Playwright considers it hidden. Tests check the dialog's buttons instead.
  - **Results.** 15/15, twice in a row (about 1 minute), with no server warnings reported by `AssertNoLoggedIssues()`.

## Definition of done (Phase 1)

- [ ] Steps 1.1–1.13 are checked.
- [ ] The CI-flag build is green, `OrchardCore.Tests` passes, Vitest passes, and the functional `*Cms*` tests pass, including the new `WorkflowsDesignerTests`.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Existing recipes and deployment packages with `WorkflowType` steps import and render unchanged (covered by the seeded fixture).
- [ ] Docs and release notes are updated.

## Review feedback

Requested by the maintainer while reviewing the running designer after step 1.11.

- [x] **R1. Explain the panel tabs.** The Activity, Workflow and Issues tabs stay. Each tab and its rail button gets a tooltip that says what it is for, and the Workflow and Issues tabs start with a one-line explanation.
- [x] **R2. Collapsed panel rail.** A collapsed properties panel is a rail showing the three tabs as icons, with the issue count on Issues, like the admin menu in its compact mode.
  - Clicking an icon expands the panel on that tab.
  - Hovering the rail opens the panel over the canvas on that tab. It closes when the pointer leaves, and stays open while the focus is in it; Escape closes it.
  - Hovering only switches tabs when the open form has nothing to apply.
- [x] **R3. Collapsible toolbox.** The activities toolbox can also be collapsed to a rail, to give the canvas more room. Hovering the rail opens it over the canvas; it stays open while an activity is dragged from it and closes after the drop.
  - Adding an activity while the panel is collapsed doesn't expand it; editing one (Enter, double-click) does.
- **Collapsed state.** Both collapsed states are remembered per browser (`localStorage`), like the panel width. Nothing is stored with the workflow (D7).
- [ ] **F1. Collapse a branch and zoom to an activity (after Phase 1).** To be done once steps 1.12–1.13 and the Definition of done are complete:
  - Collapsing an activity (for example the third one) hides the activities that come after it.
  - The collapsed activity shows an indicator with the number of hidden activities, and a control to expand them again.
  - "Zoom to activity" centers and zooms the canvas on one activity, so the user can focus on part of a large workflow.
  - Decided: the collapsed branches are remembered **per browser**, per workflow type, like the other view preferences. No persisted format changes (D7).
  - To settle when implementing:
    - which activities count as "after" (all activities reachable from it, or only those that aren't reachable some other way);
    - how hidden activities behave with selection, search, issues and autosave (they stay in the graph and are saved as usual).

## Open questions

Record questions here while executing. Decide them with the maintainer before working around them.

- One shared draft per workflow type (planned) or one per user? Shared is simpler and matches the Phase 2 versioning model.
- Does publishing need a separate permission (for example `PublishWorkflows`), or does `ManageWorkflows` cover it? The plan uses `ManageWorkflows` for Phase 1.
- Should the legacy editor stay available behind a setting for one release? The plan removes it (step 1.11).

## Spike findings

Recorded on 2026-10-02 against `df4ae1a93a`. The throwaway code (a scratch `[Admin]` controller and fragment view in the Workflows module, a standalone canvas page and a jsPlumb comparison page) was not committed. **Both assumptions hold: decision D2 (in-house canvas) and D3 (injected server-rendered editors) stand**, with the constraints below.

### Injected editors

Method: a scratch admin page loaded each editor over AJAX with `BuildEditorAsync(activity, updater, isNew: true, "", "")` into a `<form>` inside a `<div data-workflow-type-id data-activity-id>` wrapper, ran the scripts, posted the form as `FormData` with the `RequestVerificationToken` header, and bound it with `UpdateEditorAsync`. Every one of the 59 activities registered in a ComingSoon tenant (Workflows, Http, Contents, Email, Users, Tenants, Forms, ReCaptcha) was loaded twice in a row. The six named in step 1.0 were also posted back.

- **The Flows fragment pattern misses most scripts.** `IResourceManager.GetRegisteredHeadScripts()` / `GetRegisteredFootScripts()` only return raw blocks registered with `RegisterHeadScript` / `RegisterFootScript`. Resources required through `<script asp-name>` / `<script asp-src … at="Foot">` (all the editors in this list) are not in them, so `IfElseTask` returned an empty `scripts`. The fragment (step 1.2) must render with `ResourceManager.RenderHeadScript(writer)` + `RenderFootScript(writer)`. That includes dependencies, for example the seven CodeMirror files plus `liquid.js` for `CorrelateTask`.
- **Stylesheets must be collected too**, with `RenderStylesheet(writer)`: `codemirror.css` (every CodeMirror editor) and `monaco-loader.css` (Monaco) are not on the admin page otherwise.
- **Script execution order matters, so bloom `evalScripts` is not enough as is.** Script-inserted external classic scripts are async. `workflow-monaco-text-editor.js` (module) ran before `monaco-loader.js` (classic) had set `window.__orchardCoreMonacoReady`, and logged "Monaco is not loaded on this page" with no editor created. Running the scripts one at a time, awaiting `load` for each external classic script, fixed it (no console errors across all 59 editors). `ServerFormHost` (step 1.7) needs an ordered runner. Add it to bloom as an option or a sibling helper, with a Vitest spec.
- **Classic scripts must be deduplicated by `src`.** Re-appending the same tags (naive `evalScripts`) inserted `workflow-url-generator.js` and `crossbrowserclipboardcopy.js` twice, and would re-run `codemirror.js`, which replaces the global `CodeMirror` and its registered modes. Module scripts are executed only once per page by the browser regardless.
- **Values in rich editors only reach their `<textarea>` on `submit`.** CodeMirror (`fromTextArea`) saves on the form's `submit`, and `bloom/components/monaco-text-editor` saves on a window `submit` listener. Posting `new FormData(form)` without a submit lost the edits (EmailTask `TextBody`/`HtmlBody`, CreateContentTask `ContentProperties` and ScriptTask `Script` were posted with their old values). Dispatching a synthetic `new Event("submit", { bubbles: true, cancelable: true })` on the panel form before building `FormData` synced all of them and does not navigate. `ServerFormHost` must do this before every apply.
- **Top-level initializers** (they run once per page, so the second editor shown gets no CodeMirror):
  - `OrchardCore.Workflows` `task-editors`: `correlate-task.ts`, `liquid-task.ts`, `http-request-task.ts`, `http-response-task.ts`.
  - `OrchardCore.Contents` `content-task-editor.ts` (Create/UpdateContentTask: `UpdateContentTask` got no editor even on its first load once `CreateContentTask` had been shown, because they share the module).
  - `OrchardCore.Email` `email-task.ts`. It also calls `document.querySelector("select")` and adds a `change` listener to **every** `<select>` on the page; it must be scoped to its editor.
  - `OrchardCore.Tenants` `create-tenant-task.ts`, `setup-tenant-task.ts`. These look for `textarea[id$=…]`, but both views render `<input type="text">`, so they never initialize, even on today's full page. Converting them keeps behavior identical; fixing the selector is out of scope.
  - **Not in the plan's list:** `OrchardCore.Notifications` `notification-task-field-editor.ts` (NotifyContentOwnerTask, NotifyUserTaskActivity), `OrchardCore.Sms` `sms-task.ts` (SmsTask), and `OrchardCore.Contents` `Assets/js/content-type-check-all.js` (the `SelectContentTypes` view component used by the seven `Content*Event` editors; it waits for `DOMContentLoaded`, so "check all" never works when injected).
- **`workflow-url-generator.ts` never initializes when injected**: it waits for `DOMContentLoaded`, so the URL stayed empty and "Regenerate" did nothing. It also reads the *first* `[data-workflow-type-id]` / `[data-activity-id]` on the page and fixed ids (`#workflow-url-text`, `#generate-url-button`, `#token-lifespan`). Step 1.3 must convert it to `observeAndInit` and resolve everything from the closest wrapper. The fragment wrapper from step 1.2 is found correctly.
- **Already safe:** `workflow-syntax-toggle` (IfElse, ForEach, ForLoop, SetOutput, SetProperty, WhileLoop) initializes on every load, after an invalid re-render, and toggles once per change (no double initialization, thanks to the `WeakSet` in `observeAndInit`). `workflow-monaco-text-editor` re-initializes too (once the ordering issue is fixed).
- **Leaks:** Monaco leaks. After loading `ScriptTask` twice and switching to another editor, `monaco.editor.getEditors()` / `getModels()` still held 2 editors and 2 models, and each instance keeps its window `submit` listener. The `oc:editor-unmounting` disposal in step 1.3 is needed. CodeMirror 5 instances live inside the replaced container and left nothing behind in the document.
- **Binding and validation work as planned.** NotifyTask, IfElseTask, ScriptTask, HttpRequestEvent, EmailTask and CreateContentTask bound from `FormData`. An invalid IfElse Liquid condition returned the re-rendered editor with the error ("Condition doesn't contain a valid Liquid expression…") and the Liquid group still visible. Field names carry the driver prefix (`NotifyTask.Message`, `IActivity.ActivityMetadata.Title`), so tests and the panel must not assume bare names.
- No editor contains inline `<script>` blocks. The only inline handler is HttpRequestEvent's `onclick="select_all_and_copy(…)"`, which works once `crossbrowserclipboardcopy.js` has loaded.

### Canvas performance

Method: one Vue 3.5 app rendering 200 HTML nodes (header, summary, one to three outcome ports) and 300 SVG cubic Bézier edges with arrow markers inside a single `translate() scale()` layer. Each benchmark mutated state once per animation frame for 3 seconds: dragging the most connected node (degree 8), dragging 50 nodes at once, and panning, at zoom 100% and 35%. The same graph was drawn with jsPlumb 2.15.6 (the D2 fallback) for comparison. Runs used Playwright for .NET (1600×900) with CDP CPU throttling (1× and 4×, 4× standing in for a mid-range laptop), both headless (SwiftShader) and headed on an Intel UHD integrated GPU. Main-thread cost per frame comes from CDP `Performance.getMetrics`.

| Scenario (headed iGPU, final technique) | In-house 1× | In-house 4× | jsPlumb 1× | jsPlumb 4× |
|---|---|---|---|---|
| Drag one node, zoom 100% | 59–60 fps | 57–58 fps | 36–48 fps | 16–24 fps |
| Drag one node, zoom 35% | 50–60 fps | 38–43 fps | 34–59 fps | 7–9 fps |
| Pan, zoom 100% | 58–59 fps | 53–57 fps | 35–46 fps | 25–29 fps |
| Drag 50 nodes, zoom 100% | 17–23 fps | 7–8 fps | 10–12 fps | 1 fps |
| Main-thread script + layout + style per frame (one-node drag, 4×) | | ≈ 3 ms | | ≈ 50–65 ms |

- **Vue reactivity is not the bottleneck.** The update cost of a one-node drag was 0.3 ms per frame (≈ 3 ms at 4×), and about 2 ms for 50 nodes (≈ 15 ms at 4×). jsPlumb spent 20–100× more main-thread time, mostly in forced layout (50 nodes at 4×: about 1.7 s per frame).
- **The remaining cost is raster, and two rendering rules remove it.** Without them, a one-node drag at 100% ran at only 17–28 fps on the iGPU (any change re-rasterized the large static layer). Shadows, markers, the dot-grid background, hit-testing, node `contain` and one-SVG-per-edge made no measurable difference. What worked:
  1. Set `will-change: transform` on the viewport layer **only while panning or zooming** (pan went from 23–32 fps to 58–60 fps). Leaving it on permanently is harmful: dragging at 35% zoom fell to 1–17 fps because the oversized layer was re-rasterized.
  2. **Lift what is being dragged:** while dragging, give the dragged node `will-change: transform` and render the edges attached to it in a second SVG overlay with `will-change: transform`, so the static content is never re-rasterized. One-node drag went from about 20 fps to 58–60 fps. Lifting each of 50 nodes into its own layer, or translating them as one group layer, did not help large selections (5–23 fps on the iGPU, GPU-bound by the re-rasterized overlay of about 150 attached edges). That's still better than jsPlumb; lift individually only up to about 10 selected nodes.
- Coalesce `pointermove` into one state update per animation frame (the benchmark already applied one update per frame).
- **Verdict:** dragging a node and panning stay smooth on an integrated GPU with 4× CPU throttling, so the in-house canvas is viable and outperforms the jsPlumb fallback in every scenario measured. Dragging 50 nodes at once degrades on integrated GPUs (jsPlumb stalls completely there). Step 1.5 must implement the two rendering rules above and should show a lighter drag preview if large-selection drags matter.

### Consequences for later steps (no decision changes)

- **1.2 Fragment view:** use `RenderHeadScript` + `RenderFootScript` for `scripts` and `RenderStylesheet` for `styles`, not the Flows `GetRegistered*` calls.
- **1.3:** besides the plan's list, convert `notification-task-field-editor.ts` (Notifications), `sms-task.ts` (Sms) and `content-type-check-all.js` (Contents), scope `email-task.ts` to its editor, and rework `workflow-url-generator.ts`. Add disposal to `bloom/components/monaco-text-editor.ts`, and make it sync its textarea on model changes so it doesn't rely only on `submit`.
- **1.5:** apply the layer rules above, and never set a permanent `will-change` on the zoomed layer.
- **1.7 `ServerFormHost`:** execute scripts in order, deduplicate classic scripts and stylesheets by URL, and dispatch a synthetic `submit` on the panel form before building `FormData`.

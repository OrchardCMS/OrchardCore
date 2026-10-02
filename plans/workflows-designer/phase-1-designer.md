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

### - [ ] 1.0 Spike: validate the two riskiest assumptions (throwaway code, not committed)

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

### - [ ] 1.1 Server: workflow type drafts

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

### - [ ] 1.2 Server: designer JSON API

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

### - [ ] 1.3 Make every activity editor safe to inject

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

### - [ ] 1.4 Front-end scaffold

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

### - [ ] 1.5 Canvas

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

### - [ ] 1.6 Toolbox

- `src/toolbox/ActivityToolbox.vue` replaces the `#activity-picker` modal and `activity-picker.ts`. Data comes from `GET Library`.
- It has a search box (display text and category), collapsible categories, and an Events/Tasks filter. Cards render `thumbnailHtml` plus an icon. Drag a card onto the canvas to add it at the drop point, or click to add it at the center of the view.
- **Icons**: use a default per category; activities can override it. Add an optional `Icon` (a Font Awesome class) to `ActivityRegistration` (`A/Options/ActivityRegistration.cs`) and an `AddActivity<TActivity, TDriver>(…, configure)` overload, so third parties can opt in without breaking. Set icons for the built-in Workflows activities only.
- Adding an activity calls `AddActivity`. If `hasEditor`, the panel then opens on the new activity.
- **Tests**: Vitest specs for filtering and search; a component test that dropping a card at a point calls the API with canvas coordinates (correct under pan and zoom).

### - [ ] 1.7 Properties panel

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

### - [ ] 1.8 Draft autosave, publish and discard

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

### - [ ] 1.9 Read-only instance viewer

- `M/Views/Workflow/Details.cshtml` mounts the same app with `readOnly: true` and an instance payload: the blocking activity ids and the status. Add a `GET` JSON action to `WorkflowController`, or embed the payload in `data-config`.
- Read-only mode hides the toolbox and the editing tools. Clicking a node shows a read-only summary: title, type and whether it's blocking.
- Highlight blocking activities as `workflow-viewer.ts` does today. **Executed-path highlighting waits for Phase 5**: `ExecutedActivities` isn't recorded yet, so add a visible TODO hook only, no engine change.
- Keep the existing State tab. Remove the stale `<script asp-name="bootstrap" version="4">` at `Details.cshtml:115`.
- **Tests**: covered in step 1.13.

### - [ ] 1.10 Theming, RTL, accessibility and localization

- **Styling**: only `--oc-*` / `--bs-*` variables (`src/OrchardCore.Themes/TheAdmin/Assets/scss/_variables.scss`) and Bootstrap classes. Check light, dark and auto `data-bs-theme`. The canvas grid and edges must stay legible in both themes.
- **RTL**: the panel and toolbox mirror. The canvas coordinate space does **not** mirror; workflow layouts stay the same for every user. Document this.
- **Localization**: every client string goes through the `workflows-designer` JS localizer from step 1.4, and server messages use `IStringLocalizer`.
- **Accessibility**: keyboard-only walkthrough (add, connect, edit, delete, publish). Visible focus. `aria-live` for save status and toasts.

### - [ ] 1.11 Make the new designer the default and retire the legacy editor

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

### - [ ] 1.12 Documentation and release notes

- **Module docs**: update `src/docs/reference/modules/Workflows/README.md`:
  - designer tour: toolbox, canvas, panel, issues, drafts and publish, keyboard shortcuts;
  - new screenshots in `src/docs/reference/modules/Workflows/docs/`;
  - **for activity authors**: the editor must be safe to inject (`observeAndInit`, idempotent, dispose on `oc:editor-unmounting`), and the optional `Icon` registration.

  Editing an existing page needs no `mkdocs.yml` change. A **new** page must go into `nav`, or `mkdocs build --strict` fails.
- **Release notes**: add to `src/docs/releases/4.0.0.md` under `### Workflows`:
  - **New features**: the designer, drafts, autosave and publish.
  - **Breaking changes**: the activity editor full pages are replaced by redirects; the `workflow-editor` and `workflow-viewer` resources are removed; the `jsplumb` resource is obsolete; third-party editor scripts must be safe to inject.

### - [ ] 1.13 End-to-end tests (Playwright for .NET)

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

## Definition of done (Phase 1)

- [ ] Steps 1.1–1.13 are checked.
- [ ] The CI-flag build is green, `OrchardCore.Tests` passes, Vitest passes, and the functional `*Cms*` tests pass, including the new `WorkflowsDesignerTests`.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Existing recipes and deployment packages with `WorkflowType` steps import and render unchanged (covered by the seeded fixture).
- [ ] Docs and release notes are updated.

## Open questions

Record questions here while executing. Decide them with the maintainer before working around them.

- One shared draft per workflow type (planned) or one per user? Shared is simpler and matches the Phase 2 versioning model.
- Does publishing need a separate permission (for example `PublishWorkflows`), or does `ManageWorkflows` cover it? The plan uses `ManageWorkflows` for Phase 1.
- Should the legacy editor stay available behind a setting for one release? The plan removes it (step 1.11).

## Spike findings

_Record the results of step 1.0 here._

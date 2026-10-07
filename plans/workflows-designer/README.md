# Workflows designer modernization

Upgrade the Orchard Core Workflows module so authoring feels like Elsa Studio (canvas, toolbox, side-panel property editing, autosave, no full-page round trips). The work happens entirely inside Orchard Core, built only from what the repository already ships, and lays the groundwork for Elsa-style engine features later.

This folder contains these files:

| File | Contents |
|---|---|
| `README.md` (this file) | Context, verified findings, decisions, roadmap, testing strategy, and rules for executing the plan |
| [`phase-1-designer.md`](phase-1-designer.md) | Detailed, step-by-step plan for Phase 1: the new designer |
| [`phase-2-versioning.md`](phase-2-versioning.md) | Detailed, step-by-step plan for Phase 2: workflow versioning |
| [`phase-3-variables.md`](phase-3-variables.md) | Detailed, step-by-step plan for Phase 3: typed variables and data binding |
| [`phase-4-expressions.md`](phase-4-expressions.md) | Detailed, step-by-step plan for Phase 4: per-input expression syntax |
| [`phase-5-journal.md`](phase-5-journal.md) | Detailed, step-by-step plan for Phase 5: execution journal and recovery |
| [`phase-6-realtime.md`](phase-6-realtime.md) | Detailed, step-by-step plan for Phase 6: real time with `OrchardCore.SignalR` |
| [`phase-7-composition.md`](phase-7-composition.md) | Detailed, step-by-step plan for Phase 7: workflows as activities and activity presets |
| [`phase-8-evaluation.md`](phase-8-evaluation.md) | Detailed, step-by-step plan for Phase 8: the branching mode, and the evaluation of composition and state machines |
| [`later-phases.md`](later-phases.md) | The missing engine and UI components, with the original design sketches of phases 2–8 and the backlog. Each phase is detailed, and tracked, in its `phase-N-*.md` file |

## Context

The decision was to keep Orchard Core's own workflow engine and modernize it, rather than embedding Elsa Workflows. Elsa Studio is a Blazor WebAssembly app that needs an extra host project, can't be customized by integrators, and is being replaced by a React dashboard in Elsa 4. Embedding the Elsa runtime also means re-implementing its persistence on YesSql and tracking its interface changes on every release. Orchard Core already has about 66 workflow activities across `OrchardCore.Workflows` and 7 other modules (Contents 16, Users 13, Tenants 6, Forms 5, and more), plus third-party modules. All of them are built on `IActivity` and display drivers, and all of them benefit directly from a better designer and engine.

Elsa's author (who also wrote `OrchardCore.Workflows`) listed the capabilities Elsa adds over Orchard Core: versioning, per-input expression syntax, typed variables and bindings, activity composition, implicit fork/join, workflows as activities, dynamic activity providers, workflows in C#, state machines, execution inspection and retry, graceful shutdown, and BPMN. Phase 1 changes only the UI. The other capabilities are listed in `later-phases.md` and scheduled by value and effort.

## Verified findings (as of `988c29a406`, 2026-10-02)

All paths are relative to `src/OrchardCore.Modules/OrchardCore.Workflows/` (`M/`) and `src/OrchardCore/OrchardCore.Workflows.Abstractions/` (`A/`).

**Today's designer**
- The editor page is `WorkflowTypeController.Edit` (`M/Controllers/WorkflowTypeController.cs:382-525`) with view `M/Views/WorkflowType/Edit.cshtml`. The script is `M/Assets/Scripts/workflow-editor.ts` (jsPlumb 2.15.6 community, imperative DOM, marked `// TODO: Re-implement this using a MVVM approach.`), built with Parcel through `M/Assets.json`.
- The graph JSON is `{id, name, isEnabled, activities:[{id, x, y, name, isStart, isEvent, outcomes:[{name, displayName}]}], transitions:[{id, sourceActivityId, sourceOutcomeName, destinationActivityId}]}`. On save, the canvas posts `WorkflowTypeUpdateModel {Id, State}`. Only `X`, `Y`, `IsStart`, removed activities and transitions are persisted from the canvas. Activity properties never come from the canvas.
- **Adding or editing an activity is a full page navigation.** It goes through `ActivityController.Create` / `ActivityController.Edit` (`M/Controllers/ActivityController.cs`) with a `returnUrl`, and the canvas survives the trip in `sessionStorage[localId]`. These actions call `_session.SaveAsync(workflowType)` **directly, bypassing `IWorkflowTypeStore`**, so `IWorkflowTypeEventHandler` (for example the HTTP route table in `M/Http/Handlers/WorkflowTypeRoutesHandler.cs`) does not run until the canvas itself is saved. If an activity has no editor, the GET writes to the database.
- Node bodies are server-rendered shapes: `Activity_Design` wraps `{Name}_Fields_Design`, and the picker uses `Activity_Thumbnail` with `{Name}_Fields_Thumbnail`. The editor is `Activity_Edit` with `{Name}_Fields_Edit`, built by `IActivityDisplayManager.BuildEditorAsync` and bound by `UpdateEditorAsync`. The form prefix is the activity type name. `ActivityMetadataDisplayDriver` adds the Title editor to every activity.
- Editor scripts register their own resources with `at="Foot"`:
  - CodeMirror family plus `task-editors/*.js`: Correlate, Liquid, HttpRequest, HttpResponse, Email, Create/UpdateContent, Create/SetupTenant.
  - `workflow-syntax-toggle`: IfElse, ForEach, ForLoop, SetOutput, SetProperty, WhileLoop.
  - Monaco plus `workflow-monaco-text-editor`: Script, WorkflowFaultEvent.
  - `workflow-url-generator`: HttpRequestEvent. It reads `[data-workflow-type-id]` and `[data-activity-id]` from the wrapper in `M/Views/Activity/EditActivity.cshtml`.

  Some of these scripts initialize once at module top level instead of using `observeAndInit`, so they won't initialize when injected later.
- Outcomes that depend on the activity's own properties have to be recomputed after an edit: ForkTask `Forks`, ScriptTask `AvailableOutcomes`, UserTaskEvent `Actions`, HttpRequestTask `HttpResponseCodes`.
- A proven "load an editor over AJAX" pattern already exists. `OrchardCore.Flows/Controllers/AdminController.cs` `BuildEditor` and `OrchardCore.Flows/Views/Admin/Display.cshtml` return `{Content, Scripts}`. The client runs the scripts with `.scripts/bloom/helpers/evalScripts.ts`. That view does **not** collect registered styles.

**Today's engine (relevant to later phases)**
- `WorkflowType` has no version, draft or published fields. `Workflow.WorkflowTypeId` is a string, and resume always loads the **current** type (`M/Services/WorkflowManager.cs:243-292`). Edits apply to running instances immediately. Instances keep their snapshotted activity properties (`WorkflowState.ActivityStates`); activities added later get empty properties, and removed ones throw `KeyNotFound`.
- Variables are untyped `Properties` / `Input` / `Output` dictionaries on `WorkflowExecutionContext`.
- `WorkflowExpression<T>` only holds a string. The syntax is chosen per activity, through paired properties plus `WorkflowScriptSyntax {JavaScript, Liquid}`.
- `ExecuteWorkflowAsync` walks a flat graph and follows only **one** transition per outcome (`Transitions.FirstOrDefault`).
- `WorkflowExecutionContext.ExecutedActivities` is never pushed to, so `WorkflowState.ExecutedActivities` is always empty. The instance viewer (`M/Views/Workflow/Details.cshtml`, `workflow-viewer.ts`) can only highlight blocking activities. Its "Log" tab is commented out.
- There are no JSON/API endpoints for workflow types or instances, and no audit trail integration.

**Building blocks Orchard already ships (the only ones this plan uses)**
- **Vue 3.5** is the only Vue; the `vuejs` resource is 3.5.13. **Vite + Vue SFC** builds through `.scripts/assets-manager` (`"action": "vite"`). The reference app is `OrchardCore.Media/Assets/media-gallery` (lib mode, ES output, `@bloom` alias, `vue.esm-bundler`, resource registered with `type="module"`).
- `@orchardcore/bloom` (`.scripts/bloom`) provides:
  - `services/api-service.ts`: axios with the `RequestVerificationToken` header in cookie mode.
  - `helpers/globals.ts`: `getAntiForgeryToken`, `getTenantPathBase`, `getAdminPrefix`.
  - `helpers/localizations.ts`.
  - `services/notifications/notifier.ts`: `notify()`, which understands ProblemDetails.
  - `helpers/observeAndInit.ts` and `helpers/evalScripts.ts`.
  - `services/signalr/*`: `SignalRApp`, which reconnects automatically.
  - `helpers/monaco.ts` and `monacoTheme.ts`.
  - `components/*` initializers.
- Admin UI: Bootstrap 5.3.8, Font Awesome 7, `window.confirmDialog`, `--oc-*`/`--bs-*` CSS variables, `data-bs-theme` dark mode and RTL (`postcss-rtlcss`). Client strings are localized through `IJSLocalizer` plus `Orchard.GetJSLocalizations(group)` (example: `OrchardCore.Cors/Services/CorsJSLocalizer.cs`). There is no global client toast API, so the designer renders its own Bootstrap toasts fed by bloom `notify()`.
- **SignalR**: feature `OrchardCore.SignalR` (with Redis and Azure backplanes), resource `signalr` (`@microsoft/signalr` 10.0.0). Hubs are mapped per tenant with `routes.MapHub<T>()`; the reference is `OrchardCore.Media/Hubs/MediaHub.cs` with policy `MediaHub` and permission-checked groups.
- Graph rendering: the repository has **no diagram library besides jsPlumb 2.15.6**. It has no React Flow, Vue Flow, d3, dagre or elkjs, and no Pinia or Vuex (Media uses module-level `ref` stores and mitt).
- JSON endpoints for admin UIs: `[Admin]` MVC controllers returning JSON (example: `OrchardCore.DataLocalization/Controllers/AdminController.cs`). `AutoValidateAntiforgeryTokenAttribute` is global, so every non-GET MVC action validates the token. ProblemDetails helpers are in `src/OrchardCore/OrchardCore.Abstractions/ProblemDetailsApiControllerExtensions.cs`.

## Decisions

| # | Decision | Why | Rejected alternatives |
|---|---|---|---|
| D1 | The designer is a **Vue 3 SFC app built with Vite** at `M/Assets/designer/`, following the Media gallery setup and using `@orchardcore/bloom` services. | It's the established path for rich admin apps. Vue, Vite, bloom and Vitest are already in the repository. | React or React Flow (new framework, not shipped by Orchard). The global `vuejs` resource with string templates (no SFCs and weak typing for an app this size). Extending `workflow-editor.ts` (imperative jsPlumb code its own TODO says to rewrite). |
| D2 | The **canvas is written in-house**: nodes are absolutely positioned HTML in a CSS-transformed layer, and edges are SVG paths in the same transformed coordinate space. It supports pan, zoom, grid snap and connection by dragging from a port. No new runtime dependency. | It renders declaratively from reactive state, unit-tests in Vitest/jsdom, and supports dark mode and RTL through CSS variables. jsPlumb 2.x community is imperative, unmaintained and conflicts with Vue reactivity. | jsPlumb inside Vue (kept only as a fallback if the canvas spike in step 1.0 fails). Adding Vue Flow (a new third-party dependency). |
| D3 | **Node bodies, toolbox cards and property editors stay server-rendered** from the existing shapes (`{Name}_Fields_Design`, `_Thumbnail`, `_Edit`) and are injected into the Vue app. | All ~66 existing activities and third-party activities work unchanged, and the display-driver extensibility model is preserved. | Re-implementing editors in Vue (breaks every module and third party). |
| D4 | The designer talks to a new **`[Admin]` MVC controller returning JSON** (`WorkflowDesignerController`). Editors are rendered with the Flows `{Content, Scripts, Styles}` pattern. | Shape rendering needs Razor views, which MVC provides. The global antiforgery validation and `[Admin]` routing come for free. | Minimal API endpoints (they would need their own Razor rendering and antiforgery filter). |
| D5 | Autosave writes to a **server-side draft** (`WorkflowTypeDraft` document). **Publish** applies the draft to the live `WorkflowType` through `IWorkflowTypeStore.SaveAsync`. | Partially edited workflows must never execute, and all store event handlers (HTTP routes and others) must run. This also fixes the store bypass in `ActivityController`. The draft becomes the "draft version" in Phase 2. | Autosaving straight into the live type (unsafe). Client-only drafts in sessionStorage (lost across devices and tabs). |
| D6 | Drafts use **optimistic concurrency** (an integer `Revision`). A stale save returns 409 and the UI offers reload or overwrite. | Two tabs or two users must not silently overwrite each other. Real-time presence comes in Phase 6 (SignalR). | Last write wins. |
| D7 | **Persisted formats stay compatible.** `WorkflowType` JSON, recipe step `WorkflowType`, deployment step `AllWorkflowType`, `X`/`Y` pixel coordinates and the shape names are unchanged. The `jsplumb` resource stays registered (deprecated) for third parties. | It's a non-breaking upgrade; existing recipes and exported packages must keep working. | |
| D8 | Phase 1 changes **no engine behavior.** Everything that changes execution (versions, variables, journal, multiple transitions per outcome) is in `later-phases.md`. | It keeps Phase 1 reviewable and shippable on its own. | |

## Roadmap

Update the **Status** column as work lands (`Not started` / `In progress` / `Done (commit)`). Each phase file ticks its steps and its Definition of done, with notes, in the commit that does them.

The work is pushed to the draft pull request [OrchardCMS/OrchardCore#19996](https://github.com/OrchardCMS/OrchardCore/pull/19996) (branch `ma/workflows-designer`), one commit per step.

**Status (2026-10-07):** phases 1–8 are done and await review.
- **Merge.** The branch was brought up to date with `main` (merge `45fb919d29`).
    - The SMS task editor kept `main`'s new From Phone Number field inside the branch's editor wrapper.
    - `main`'s new execution-limit test creates `IfElseTask` with the expression manager it takes since Phase 4.
- **Verification of the merged result:**
    - The CI-flag build has 0 warnings and 0 errors.
    - `OrchardCore.Tests`: 3,656 tests, 3,651 passed and 2 skipped. The 3 `ContentQuickNavigationTests` cases failed when their cleanup hit a Windows file lock; the class passed 11 of 11 when run again.
    - Vitest: 236 of 236. The functional `*Cms*` tests: 165 of 165.
    - `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.

| Phase | Scope | Depends on | Status |
|---|---|---|---|
| **1** | **New designer**: canvas, toolbox, side-panel editors, drafts with autosave and publish, read-only instance viewer, tests. See [`phase-1-designer.md`](phase-1-designer.md). | — | Done (1.0–1.13 and the Definition of done; awaiting review) |
| 2 | Workflow versioning (drafts → versions, instances pinned to a version, history, revert). See [`phase-2-versioning.md`](phase-2-versioning.md). | 1 | Done (2.1–2.8 and the Definition of done; awaiting review) |
| 3 | Typed variables, activity outputs and data binding. See [`phase-3-variables.md`](phase-3-variables.md). | 2 | Done (3.1–3.8 and the Definition of done; awaiting review) |
| 4 | Per-input expression syntax (Literal / Liquid / JavaScript / pluggable providers). See [`phase-4-expressions.md`](phase-4-expressions.md). | 3 (recommended) | Done (4.1–4.6 and the Definition of done; awaiting review) |
| 5 | Execution journal, executed-path highlighting, retry of a faulted activity. See [`phase-5-journal.md`](phase-5-journal.md). | 1 | Done (5.1–5.6 and the Definition of done; awaiting review) |
| 6 | Real time with `OrchardCore.SignalR`: live instance view, presence, draft change notifications. See [`phase-6-realtime.md`](phase-6-realtime.md). | 1, 5 | Done (6.1–6.5 and the Definition of done; awaiting review) |
| 7 | Workflows as activities; dynamic activity providers (as activity presets). See [`phase-7-composition.md`](phase-7-composition.md). | 2, 3 | Done (7.1–7.6 and the Definition of done; awaiting review) |
| 8 | Evaluate: multiple transitions per outcome or implicit fork, composition/containers, state machines. See [`phase-8-evaluation.md`](phase-8-evaluation.md). | 1–5 | Done (8.1–8.4 and the Definition of done; awaiting review) |

## Testing strategy (applies to every phase)

**.NET unit and integration tests**
- Project: `test/OrchardCore.Tests`. Uses xUnit v3 (`xunit.v3.mtp-v2`), Moq and plain `Assert.*`.
- Put new tests under `test/OrchardCore.Tests/Modules/OrchardCore.Workflows/`, named `{Action}_{Condition}_{ExpectedResult}`.
- For engine tests, reuse the helpers in `test/OrchardCore.Tests/Workflows/WorkflowManagerTests.cs` (`CreateWorkflowManager`, fake activities in `test/OrchardCore.Tests/Workflows/Activities/`). Extract them into a shared internal helper when a second test class needs them.
- For HTTP-level tests of admin endpoints, use `SiteContext` (`test/OrchardCore.Tests/Apis/Context/SiteContext.cs`). It creates a fresh SQLite tenant and its `PermissionContextAuthorizationHandler` lets you test both allowed and forbidden cases through the `PermissionsContext` header. Save documents through YesSql `ISession` as `test/OrchardCore.Tests/Navigation/NestedAdminInlineBreadcrumbTests.cs` does.

**Front-end unit tests (Vitest)**
- Add a Vitest setup to `M/Assets/designer/`, copying `OrchardCore.Media/Assets/media-gallery/vitest.config.ts` (jsdom, `@vue/test-utils`, junit output to `testing/vitest-results.xml`).
- Specs go in `src/**/__tests__/*.spec.ts`.
- Add a step for the new folder to `.github/workflows/frontend_unit_tests.yml`. That workflow lists folders by hand.

**End-to-end tests (Playwright for .NET)**
- Project: `test/OrchardCore.Tests.Functional` (`Microsoft.Playwright` 1.62.0, Chromium, xUnit v3). There is no Node Playwright in the repository; don't add one.
- New class: `Tests/Cms/WorkflowsDesignerTests.cs : CmsTestBase<WorkflowsDesignerTestsFixture>`. Add the fixture to `Tests/Cms/CmsRecipeFixture.cs`, with recipe `Fixtures/workflows-designer-tests.recipe.json` (`issetuprecipe: true`; enable `TheAdmin`, `OrchardCore.Workflows`, `OrchardCore.Workflows.Http`, `OrchardCore.Workflows.Timers`, `OrchardCore.Contents`).
- Use the helpers in `Helpers/`: `AuthHelper.LoginAsync`, `FeatureHelper`, `NavigationHelper.GotoAndAssertOkAsync`, `ConsoleHelper.CollectConsoleErrors`, `SelectorHelper.GetByCy` (`data-cy` attributes), `DragDropHelper.DragAsync`.
- `CmsTestBase` fails a test on **any** logged server warning. Treat that as a feature.
- Classes in namespace `OrchardCore.Tests.Functional.Tests.Cms` run in PR CI automatically (`--filter-class "*Cms*"`).

**Asset validation**
- `.github/workflows/assets_validation.yml` runs `yarn lint`, `yarn check` (vue-tsc) and `yarn build`, then fails if `git status --porcelain` is dirty. **Built `wwwroot` output is committed.** Run `yarn build` and commit its output with every front-end change.

## Commands

```bash
# Build exactly as CI does (warnings are errors; analyzers on)
dotnet build OrchardCore.slnx -c Release -p:TreatWarningsAsErrors=true --warnaserror -p:RunAnalyzers=true -p:NuGetAudit=false

# .NET tests (Microsoft.Testing.Platform: always pass --project; use --filter-class / --filter-method, not VSTest --filter)
dotnet test --project ./test/OrchardCore.Tests/OrchardCore.Tests.csproj -c Release --no-build --filter-class "OrchardCore.Tests.Modules.OrchardCore.Workflows.*"
dotnet test --project ./test/OrchardCore.Tests/OrchardCore.Tests.csproj -c Release --no-build --filter-class "OrchardCore.Tests.Workflows.*"

# Front end (Node 24.20.0 from .node-version; Yarn 4 through corepack)
corepack enable
yarn
yarn build -n workflows-designer   # use the asset name from Assets.json
yarn lint
yarn check
cd src/OrchardCore.Modules/OrchardCore.Workflows/Assets/designer && yarn vitest run

# End-to-end
dotnet build -c Release test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj
pwsh test/OrchardCore.Tests.Functional/bin/Release/net10.0/playwright.ps1 install chromium
dotnet test --project test/OrchardCore.Tests.Functional/OrchardCore.Tests.Functional.csproj -c Release --no-build --filter-class "*WorkflowsDesigner*"
```

Note: `AGENTS.md` and `.agents/skills/orchardcore-unit-test/SKILL.md` show `dotnet test <csproj>` and `--filter "FullyQualifiedName~..."`, and refer to `OrchardCore.sln`. In this repository the solution is `OrchardCore.slnx`, and MTP needs `--project` plus `--filter-class` / `--filter-method`.

## Rules for executing this plan

1. Read `AGENTS.md` first and follow it. Key rules:
   - `sealed` classes, file-scoped namespaces, collection expressions, **no primary constructors**.
   - `_camelCase` private fields.
   - XML docs on public APIs.
   - `ocat-*` classes in admin edit views.
   - Docs updated in `src/docs`.
   - Release notes in `src/docs/releases/4.0.0.md`.
2. Use the skills in `.agents/skills/` where they apply: `orchardcore-workflow-activity`, `orchardcore-asset-manager`, `orchardcore-unit-test`, `orchardcore-tester`, `orchardcore-docs-writer`, `orchardcore-admin-edit-views`, `localization`.
3. Work one step at a time, in order. Each step ends with a green build (CI flags), passing tests for what the step touched, committed `yarn build` output when assets changed, and **one commit** describing the step. Tick the step's checkbox in `phase-1-designer.md` in the same commit.
4. Don't change persisted formats or engine behavior in Phase 1 (decisions D7 and D8). If a step seems to require it, stop and record the question under **Open questions** in the phase file.
5. Push each committed step to the draft pull request (the maintainer approved it on 2026-10-07). Never merge, and don't open other pull requests without approval.
6. Phase 1 was reviewed before Phase 2 started. The maintainer then asked for the remaining phases to be done and verified without stopping between them.

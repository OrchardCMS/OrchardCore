# Later phases — missing components

Phase 1 (the designer) changes only the UI. This file lists the engine and UI components Orchard Core Workflows is missing, compared with Elsa Studio and Elsa Workflows, so they can be built later. Each phase has a design sketch, a compatibility plan and its tests. Before starting a phase, turn its sketch into a detailed step list in a new `phase-N-*.md`, in the same format as [`phase-1-designer.md`](phase-1-designer.md).

**Status.** Each phase below is detailed in its own file, which records what was built and why it differs from the sketch; the [roadmap](README.md#roadmap) tracks them. Phases 2–9 are done; Phase 10 is in progress. The backlog isn't scheduled.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Missing components at a glance

| Component | Today (before the plan) | Needed for | Phase | Effort | Status |
|---|---|---|---|---|---|
| Workflow versions (draft / published / history), instances pinned to a version | None; instances resume on the current definition | Safe edits, revert, workflows as activities | 2 | Medium | Done |
| Typed workflow variables | Untyped `Properties` dictionary | Data binding, validation, designer IntelliSense | 3 | Medium | Done |
| Activity output descriptors and output-to-variable binding | Activities write `Properties` / `LastResult` ad hoc | Typed data flow between activities | 3 | Medium | Done |
| Per-input expression syntax (Literal / Liquid / JavaScript / custom) | Per activity, through paired properties plus `WorkflowScriptSyntax` | Consistent editors, custom providers | 4 | Medium | Done |
| Execution journal (per-activity start, end, outcome, error) | `ExecutedActivities` is never recorded | Executed-path view, debugging, Log tab | 5 | Medium | Done |
| Retry a faulted activity in place | Only "Restart" (a new instance with the old input) | Operations and recovery | 5 | Medium | Done |
| Real-time designer and instance updates | None | Live instance view, presence, conflict notices | 6 | Small–Medium | Done |
| Workflow as an activity (sub-workflow with inputs and outputs) | None | Reuse | 7 | Medium | Done |
| Dynamic activity providers (catalog from configuration or a database) | Only types registered with `AddActivity` | Integrations, low-code catalogs | 7 | Medium | Done, as activity presets |
| Multiple transitions per outcome / implicit fork | Engine follows only the first transition | Simpler branching | 8 | Medium–Hard | Done, as an opt-in branching mode (8.1) |
| Composition (containers, nested sequences and flowcharts) | Flat graph only | Large workflows | 8 | Hard (new execution model) | Evaluated; not built (see phase 8) |
| State-machine modeling | None | Approvals and lifecycles | 8 | Hard | Evaluated; documented as a pattern (see phase 8) |
| Audit trail for workflow type changes | None | Governance | Backlog | Small | Not scheduled |
| List of instances across all types, more status filters, bulk actions | Per type only; filters All, Finished, Faulted | Operations | 10 (10.8) | Small | Not started |
| Workflow testing from the designer ("Run with input…") | None | Authoring speed | 10 (10.6) | Small–Medium | Not started |
| Each activity's data in the journal (evaluated expressions, outputs, changed variables, last result) | Status, outcomes, duration and error only | Debugging | 10 (10.1–10.4) | Medium | Done |
| Retry policies per activity, and what a failure does | Manual retry of a faulted instance | Transient failures | 10 (10.5) | Medium | Not started |
| One running instance per correlation id | Single instance per workflow only | Approvals per content item | 10 (10.7) | Small–Medium | Not started |
| Links between parent and child instances | None | Debugging composed workflows | 10 (10.9) | Small | Not started |

---

## Phase 2 — Workflow versioning

**Detailed plan:** [`phase-2-versioning.md`](phase-2-versioning.md). **Status:** done.

**Goal:** separate drafts from published versions. Each instance stays pinned to the version it started on, and there's a history with compare and revert. Phase 1's `WorkflowTypeDraft` becomes "the draft version".

**Design sketch**
- **New document** `WorkflowTypeVersion` (in `A/Models/`): `long Id`, `string WorkflowTypeId`, `string VersionId` (unique), `int Version`, `bool IsPublished`, `bool IsLatest`, `DateTime CreatedUtc`, `string CreatedBy`, plus a snapshot of `Activities`, `Transitions` and the settings.
  - Index: `WorkflowTypeVersionIndex(WorkflowTypeId, VersionId, Version, IsPublished, IsLatest)`.
  - `WorkflowType` stays the head record (name, enabled, the indexes the engine queries today) and holds a copy of the **published** version, so `GetByStartActivityAsync` and the HTTP route handlers keep working unchanged.
- **Instances**: add `Workflow.WorkflowTypeVersionId` and include it in `WorkflowIndex`.
  - `WorkflowManager.ResumeWorkflowAsync` loads the pinned version (`WorkflowManager.cs:243-292`), falling back to the head for instances that have no version id.
  - `NewWorkflow` stamps the published version id.
  - Blocking-activity lookups (`WorkflowBlockingActivitiesIndex`) are unaffected.
- **Store**: add a version service, `IWorkflowTypeVersionStore`, with `GetVersionsAsync`, `GetAsync(versionId)`, `PublishDraftAsync`, `RevertAsync(version)` (creates a new draft from an old version) and `PruneAsync(keep N)`. `PublishDraftAsync` writes the new version and updates the head through `IWorkflowTypeStore.SaveAsync`, so the existing handlers run.
- **Migration**:
  - Create version 1 from every existing type.
  - Leave existing instances unpinned (they resume on the head, which is version 1 at that point).
  - Convert any pending Phase 1 drafts into draft versions.
- **Recipes and deployment**: the `WorkflowType` step stays as it is and imports as a new published version. Exporting with version history stays opt-in and out of scope.
- **Designer**: a version-history panel (list, view read-only, compare graphs side by side, revert to a draft). The publish dialog drops the "running instances move to the new definition" warning.
- **Retention**: a setting under `OrchardCore:Workflows:Versions` (keep the last N versions, never deleting versions that instances are pinned to), enforced by the existing trimming background task (`M/Trimming/`).

**Tests**
- Engine: an instance started on v1 resumes on v1 after v2 is published, including activities that v2 removes or renames; unpinned legacy instances resume on the head; pruning never deletes pinned versions.
- Migration: upgrading from schema 5 creates v1 per type and keeps instances runnable.
- End to end: publish v2 while an instance is halted on a `UserTaskEvent`; resuming follows v1.

## Phase 3 — Typed variables and data binding

**Detailed plan:** [`phase-3-variables.md`](phase-3-variables.md). **Status:** done.

**Goal:** declared, typed variables at workflow scope. Activities have typed outputs that can be bound to variables, and expressions can read the variables. This mirrors Elsa's variables and output binding without changing the flat execution model.

**Design sketch**
- **Declarations**: `WorkflowVariableDefinition { string Name; string TypeName; JsonNode DefaultValue; string Description; }` (in `A/Models/`), stored on the version and the head as `IList<WorkflowVariableDefinition> Variables`.
  - Supported types come from a registry, `IWorkflowVariableTypeProvider`, with built-ins: `string`, `number`, `boolean`, `datetime`, `json object`, `json array`, `content item` (through the existing `IWorkflowValueSerializer` `ContentItemSerializer` in OrchardCore.Contents) and `user`.
  - Names are unique and valid identifiers.
- **Runtime**:
  - `WorkflowExecutionContext.Variables` is a typed store backed by `Properties` for compatibility. A declared variable `x` **is** `Properties["x"]`, coerced to its type on write and validated (with an error on failure). Existing workflows that use `Properties` keep working.
  - Defaults are applied when the workflow starts.
  - Persistence uses the existing `IWorkflowValueSerializer` path in `PersistAsync`.
- **Expressions**: JavaScript gets `variables.x` and keeps `property('x')` for compatibility; register it through `WorkflowMethodsProvider`. Liquid gets `{{ Workflow.Variables.x }}`, added to Fluid member access in `M/Startup.cs:40-46`.
- **Activity outputs**: an optional interface, `IActivityOutputs`, declares `IEnumerable<ActivityOutputDescriptor { Name, TypeName, DisplayName }>`, and activities set values with `context.SetOutput(activity, "Name", value)`.
  - Each activity record has an optional binding map, `ActivityRecord.Properties["OutputBindings"] = { "Name": "variableName" }`, applied after the activity executes (`WorkflowManager.ExecuteWorkflowAsync`).
  - Start with built-ins that already produce values: `HttpRequestTask` (response), `CreateContentTask` (content item), `ScriptTask` (last result) and `SetPropertyTask`.
- **New activity**: "Set Variable", with type-aware editors.
- **Designer**:
  - A **Variables** tab (CRUD, type picker, default value editor).
  - In the Activity tab, an **Outputs** section to bind each output to a variable.
  - Variable-name completion in the Monaco editors (through the existing Liquid/JS IntelliSense hooks; see `MonacoLiquidIntelliSenseTests` in the functional tests for the pattern).
  - Validation issues for unknown variable references and type mismatches where they can be detected.
- **Scopes**: workflow scope only. Activity- and container-scoped variables depend on composition (Phase 8).

**Tests**
- Engine: type coercion, defaults on start, a binding written after execution, existing `Properties`-based workflows unchanged, JS and Liquid access, serialization round trip (including a content item).
- Designer: Vitest for the variables editor state; end to end to declare a variable, bind the `HttpRequestTask` output, and read it in a `Notify` Liquid message.

## Phase 4 — Per-input expression syntax

**Detailed plan:** [`phase-4-expressions.md`](phase-4-expressions.md). **Status:** done.

**Goal:** any expression-capable input can be Literal, Liquid or JavaScript, chosen per input (plus custom providers). This replaces the paired-property pattern (`Condition` / `LiquidCondition` plus `WorkflowScriptSyntax`).

**Design sketch**
- **Model**: extend `WorkflowExpression<T>` with an optional `string Syntax`. Null means the legacy behavior of the call site, so existing stored data deserializes unchanged.
- **Providers**: an `IWorkflowExpressionProvider { Name; DisplayName; Task<T> EvaluateAsync<T>(…) }` registry. The `Literal`, `Liquid` and `JavaScript` providers wrap the existing `IWorkflowExpressionEvaluator` / `IWorkflowScriptEvaluator`. Activity authors can restrict the allowed syntaxes per property (with an attribute or descriptor).
- **Editor**: a reusable shape or tag helper, `WorkflowExpressionEditor`, with a syntax dropdown and Monaco (language per syntax) that's safe to inject (Phase 1, step 1.3). Migrate the editors of `IfElse`, `ForEach`, `ForLoop`, `WhileLoop`, `SetOutput`, `SetProperty` and `Correlate`.
- **Compatibility**: each migrated activity reads its legacy paired properties when `Syntax` is unset. Saving from the new editor writes the new shape. Add no data migration until a later major version.

**Tests**: each provider; legacy stored activities evaluate the same (snapshot tests built from today's `WorkflowManagerTests` scenarios); a custom provider registered in a test module appears in the editor (end to end).

## Phase 5 — Execution journal and recovery

**Detailed plan:** [`phase-5-journal.md`](phase-5-journal.md). **Status:** done.

**Goal:** record what ran, show it, and recover from faults without starting a new instance. This also enables the commented-out "Log" tab (`M/Views/Workflow/Details.cshtml:23,90`).

**Design sketch**
- **Journal**:
  - `WorkflowExecutionRecord` documents (in their own YesSql collection, so `Workflow.State` doesn't grow): `{ WorkflowId, ActivityId, ActivityName, Outcome(s), StartedUtc, CompletedUtc, Status (Completed|Halted|Faulted), Error, Sequence }`, plus an index on `WorkflowId` and `Sequence`.
  - Written from `WorkflowManager.ExecuteWorkflowAsync` through a new `IWorkflowExecutionJournal`. Writes are batched per execution cycle.
  - Optional settings `OrchardCore:Workflows:Journal`: enabled, the maximum number of records per instance, and whether input and output snapshots are included (off by default for privacy and size).
  - Also populate `WorkflowExecutionContext.ExecutedActivities`, capped.
  - Trimming deletes the journal together with its instance (`M/Trimming/`).
- **Viewer**: the designer's read-only mode highlights the executed path (nodes and the edges taken, with a count for loops) and shows a timeline in a **Journal** tab (per-activity duration and error details).
- **Retry**: "Retry from here" on a faulted activity re-runs that activity with the current instance state and continues from it, through a new `IWorkflowManager.RetryActivityAsync(workflow, activityId)`. It's guarded by a lock and the `ExecuteWorkflows` permission.
- **Incident strategy** (optional): a per-type setting for "fault the instance" (today) or "continue on the `Failed` outcome" for activities that declare one.

**Tests**: the journal records the order, outcomes and faults of a branching workflow (unit); the cap is enforced; retry continues after fixing the faulting activity's input (unit plus end to end in the viewer); trimming removes journal records.

## Phase 6 — Real time with `OrchardCore.SignalR`

**Detailed plan:** [`phase-6-realtime.md`](phase-6-realtime.md). **Status:** done.

**Goal:** live updates in the designer and the instance viewer, built on `src/OrchardCore.Modules/OrchardCore.SignalR`. Autosave stays on REST.

**Design sketch**
- **Feature**: `OrchardCore.Workflows.SignalR`, which depends on `OrchardCore.Workflows` and `OrchardCore.SignalR`. Map the hub with `routes.MapHub<WorkflowsHub>("/hubs/workflows")` in that feature's Startup.
  - Policy `WorkflowsHub`: Api plus cookie schemes and `PermissionRequirement(ManageWorkflows)`. Mirror `OrchardCore.Media/Services/MediaApiAuthorizationOptionsConfiguration.cs`.
  - Group-based subscriptions, with permission checks inside `Subscribe*` (mirror `OrchardCore.Media/Hubs/MediaHub.cs`): `workflow-type:{id}` and `workflow:{id}`.
- **Events**:
  - `DraftChanged {workflowTypeId, revision, by}`: other open designers show "updated by X — reload" and get presence avatars.
  - `Published`.
  - `InstanceProgress {workflowId, record}`, raised by the Phase 5 journal through `IHubContext<WorkflowsHub>`: the viewer animates the executed path live.
  - `InstanceStatusChanged`.
- **Client**: bloom `services/signalr/SignalRApp` (reconnects automatically; re-subscribe on reconnect, as `OrchardCore.Media/Assets/media-gallery/src/services/SignalR.ts` does). Build the hub URL on the server with `Url.Content("~/hubs/workflows")` so it's tenant-prefix aware; don't hard-code `/hubs/...` the way Media's embedded mode does. Pass a `signalrEnabled` flag in the designer config, set only when `IHubContext<WorkflowsHub>` resolves (the Media pattern is `MediaSignalRExtensions.IsMediaSignalREnabled`).
- **Multi-node**: works with the existing Redis and Azure backplanes; there's nothing workflow-specific to add.

**Tests**: hub authorization (forbidden without the permission; subscribing to another type is rejected without access); end to end with two pages, where a save in one shows the notice in the other; viewer progress updates while a timer-driven workflow runs.

## Phase 7 — Workflows as activities; dynamic activity providers

**Detailed plan:** [`phase-7-composition.md`](phase-7-composition.md). **Status:** done. The dynamic activity providers were built as activity presets (decision C6).

**Workflow as an activity**
- A type opts in with "Usable as activity" (a setting on the published version) and declares its inputs and outputs, reusing the Phase 3 variable definitions with input and output direction.
- The engine adds an `ExecuteWorkflowTask` activity, generated for each opted-in type through the dynamic provider below. It starts a child instance pinned to a version (Phase 2), maps the inputs, and either waits for completion (it's blocking, resumed by a `WorkflowCompleted` signal from the child) or fires and forgets.
- Track the parent-child relationship on `Workflow` (`ParentWorkflowId`) for the viewer.
- Recursion depth reuses `WorkflowManager.MaxRecursionDepth`.

**Dynamic activity providers**
- `IActivityProvider { Task<IEnumerable<ActivityDescriptor>> GetDescriptorsAsync(); IActivity Create(descriptor, JsonObject properties); }`.
- The descriptor carries name, display text, category, icon, outcomes, inputs (with type and syntax constraints) and outputs.
- `ActivityLibrary` merges providers with the activities registered through `AddActivity`. The designer and the editors render descriptor-driven inputs through a generic driver (`DescriptorActivityDisplayDriver`), so no Razor view is needed per activity.

**Tests**: a child workflow runs and returns outputs to the parent; the parent waits and resumes; recursion is guarded; a provider-defined activity appears in the toolbox, edits and executes.

## Phase 8 — Evaluate: branching model, composition, state machines

**Detailed plan:** [`phase-8-evaluation.md`](phase-8-evaluation.md). **Status:** done. The branching mode was built; composition and state machines were evaluated and not built (see its Evaluation).

Spike and decide; these change the execution model. Pursue them only when there's real demand.

- **Multiple transitions per outcome / implicit fork**: change `Transitions.FirstOrDefault` (`WorkflowManager.ExecuteWorkflowAsync`) to follow all of them, behind a per-type setting (`BranchingMode = FirstOnly | All`, default `FirstOnly` for compatibility). It needs a Join that understands skipped paths before it can be the default.
- **Composition**: container activities (Sequence, nested Flowchart, ForEach with a body) need an execution-context tree and scheduler (in the style of Elsa 3), not a flat stack. That's a new engine. Assess it against the backlog before committing to it.
- **State machines**: named states with entry and exit activities and trigger-driven transitions. They could be modeled as a specialized workflow type with its own designer mode, but only after composition.
- **BPMN**: out of scope.

## Backlog (small, can go into any phase)

- Audit trail events for creating, publishing, deleting and reverting workflow types (`OrchardCore.AuditTrail` provider).
- An auto-layout command ("Tidy up") using a simple layered layout implemented in-house (there's no layout library in the repository).
- Copy and paste of activities between workflow types (clipboard with JSON records; new ids assigned on paste).

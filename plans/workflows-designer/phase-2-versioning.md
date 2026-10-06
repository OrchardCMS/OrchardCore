# Phase 2 — Workflow versioning

Separate what is being edited from what runs. Every publish creates an immutable **version** of the workflow type, each instance stays pinned to the version it started on, and the designer shows the history of versions with compare and restore. This turns the sketch in [`later-phases.md`](later-phases.md#phase-2--workflow-versioning) into steps.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

Unlike Phase 1, this phase changes persisted formats and engine behavior on purpose (decisions D7 and D8 applied to Phase 1). Every change stays additive: documents written before the upgrade keep loading, and instances created before it keep their current behavior.

## Target experience

```
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│ Order approval  [Version 3 · published 12:04]   ● Draft saved   [Versions] [Discard] [Publish]│
├────────────┬──────────────────────────────────────────────────────────┬───────────────────┤
│ Activities │                        canvas                            │ Activity|Workflow|│
│            │                                                          │ Issues            │
└────────────┴──────────────────────────────────────────────────────────┴───────────────────┘

┌ Versions of "Order approval" ──────────────────────────────────────────────┐
│ ● Draft            changed 12:10 by alice                [Compare]         │
│   Version 3  Published  12:04 by alice   2 instances     [View] [Compare] │
│   Version 2             09:31 by bob     5 instances     [View] [Compare] [Restore] │
│   Version 1             Sep 30 by bob                    [View] [Compare] [Restore] │
└────────────────────────────────────────────────────────────────────────────┘
```

- **Publish** creates the next version and makes it the one new instances start on. Running instances keep running on the version they started on.
- **Versions** lists the history. **View** opens a version read-only, **Compare** shows two versions (or a version and the draft) side by side with the differences highlighted, and **Restore** copies a version into the draft, to publish it again as a new version.
- The instance page shows the version the instance runs on.

## Decisions

| # | Decision | Why |
|---|---|---|
| V1 | **A version is an immutable snapshot** (`WorkflowTypeVersion` document) of what affects execution: activities, transitions, `IsSingleton`, `LockTimeout`, `LockExpiration`, `DeleteFinishedWorkflows`. `Name` and `IsEnabled` stay on the head `WorkflowType` only. The snapshot also records the name at the time, for display. | Renaming or disabling a workflow doesn't change how its instances run, so it shouldn't create versions. |
| V2 | **`WorkflowType` stays the head** and always holds the published (latest) version, with its id in a new `WorkflowType.VersionId`. Start lookups, HTTP routes and every `IWorkflowTypeStore` consumer keep reading the head unchanged. | No change for the engine's start paths and third-party code. |
| V3 | **Versions are created by `WorkflowTypeStore.SaveAsync`**, before the handlers run, whenever the snapshot differs from the latest version. Publishing from the designer, recipes, `Duplicate`, the properties page and third-party code that saves a type all create versions the same way. | One gate: the head and its latest version can't drift apart. |
| V4 | **The Phase 1 draft stays the draft** (`WorkflowTypeDraft`, with its revision and conflict handling). Versions are only published snapshots; restoring a version writes it into the draft. There is nothing to convert, since Phase 1 isn't released. | The draft manager already works and is tested; the sketch's "draft version" is the draft document. |
| V5 | **Instances are pinned**: `Workflow.WorkflowTypeVersionId` is set when an instance is created and indexed in `WorkflowIndex`. Resuming runs the pinned version. Instances without one (created before the upgrade) resume on the head, as they do today. | Safe edits; no behavior change for existing instances. |
| V6 | **Restart** starts a new instance on the published version, as it does today. | Restart means "run again", with the current definition. |
| V7 | **Recipes import a new version.** The `WorkflowType` step updates an existing type in place (same document, instances kept and pinned to their versions) instead of deleting and recreating it; it still discards the draft. Export is unchanged and leaves `VersionId` out. | Importing a deployment no longer deletes running instances. |
| V8 | **Retention**: `Workflows:Versions:MaxCount` (0, the default, keeps every version). When set, publishing deletes the oldest versions beyond it, never the published version or a version an instance is pinned to. | Deleting history is opt-in. Pruning at publish time is simpler than in the daily trimming task, and only publishing adds versions. |
| V9 | **Compare** is two read-only canvases side by side, with added, changed and removed activities and transitions highlighted, computed on the server. | Reuses the canvas; the diff rules are tested in .NET. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 2.1 Versions: model, store and migration

- **Model** `A/Models/WorkflowTypeVersion.cs` (`sealed`): `long Id`, `string WorkflowTypeId`, `string VersionId` (26 characters, from `IdGenerator`), `int Version` (1, 2, 3…), `DateTime CreatedUtc`, `string CreatedByUserId`, `string CreatedByUserName`, `string Name`, and the snapshot (V1): `IsSingleton`, `LockTimeout`, `LockExpiration`, `DeleteFinishedWorkflows`, `IList<ActivityRecord> Activities`, `IList<Transition> Transitions`. Deep copies (`ActivityRecord.Properties` is a `JsonObject`).
- **Index** `M/Indexes/WorkflowTypeVersionIndex.cs`: `WorkflowTypeId`, `VersionId`, `Version`, `CreatedUtc`.
- **Head and instance fields** (additive):
  - `WorkflowType.VersionId`: the published version.
  - `Workflow.WorkflowTypeVersionId`, and a `WorkflowTypeVersionId` column on `WorkflowIndex`.
- **Store** `A/Services/IWorkflowTypeVersionStore.cs` and `M/Services/WorkflowTypeVersionStore.cs` (scoped):
  - `GetAsync(string versionId)`, `ListAsync(string workflowTypeId)` (newest first), `GetLatestAsync(string workflowTypeId)`;
  - `CreateIfChangedAsync(WorkflowType workflowType)`: compares the snapshot with the latest version, creates version N+1 when it differs (or when there is none), and sets `workflowType.VersionId`; returns the version;
  - `DeleteAsync(string workflowTypeId)`: deletes every version of a type.
- **`WorkflowTypeStore`**: `SaveAsync` calls `CreateIfChangedAsync` before raising `CreatedAsync`/`UpdatedAsync`; `DeleteAsync` deletes the versions with the type.
- **Export** (`AllWorkflowTypeDeploymentSource`, `WorkflowTypeController.ExportWorkflows`): leave `VersionId` out, like `Id`.
- **Migration** `UpdateFrom5Async` → 6:
  - create the `WorkflowTypeVersionIndex` table and its index;
  - add the `WorkflowTypeVersionId` column to `WorkflowIndex`;
  - in a deferred task, create version 1 of every existing type and set its `VersionId` (through the version store, without raising the type handlers). Existing instances stay unpinned (V5).
- **Tests** (`test/OrchardCore.Tests/Modules/OrchardCore.Workflows/Versioning/`):
  - the first save creates version 1 and sets `VersionId`;
  - saving unchanged, renamed or disabled types creates no version;
  - changing an activity, a transition or an execution setting creates the next version;
  - a version is a deep copy (changing the head afterwards doesn't change it);
  - deleting a type deletes its versions;
  - the migration's deferred work creates version 1 for each existing type, once;
  - export leaves `VersionId` out.
- **Notes from implementing this step:**
  - **Fingerprint.** "Changed" compares a JSON fingerprint of the execution settings, activities (including their positions and properties) and transitions with the latest version. Moving an activity and publishing creates a version, like any other edit; saving the same definition again doesn't.
  - **Migration.** `CreateInitialVersionsAsync` is a `public static` method on `Migrations`, because the test project has no access to internals. It skips types that already have a `VersionId`, so running it again does nothing.
  - **Tests.** `Versioning/WorkflowTypeVersionStoreTests.cs` runs the real migrations (`CreateAsync`, `UpdateFrom4Async`, `UpdateFrom5Async`) against SQLite, so the new tables and columns are covered too. The SiteContext tests (`WorkflowDesignerControllerTests`, `WorkflowTypeDraftLifecycleTests`) set up tenants through the same migrations. Workflows tests: 77/77.

### - [x] 2.2 Engine: pin instances to their version

- `NewWorkflow` stamps `Workflow.WorkflowTypeVersionId = workflowType.VersionId`.
- `IWorkflowTypeVersionStore.GetWorkflowTypeAsync(WorkflowType head, string versionId)` returns the head when `versionId` is empty or is the head's, otherwise a `WorkflowType` built from the head (`Id`, `WorkflowTypeId`, `Name`, `IsEnabled`) and the version's snapshot. A missing version falls back to the head with a warning. Document that the returned object must never be saved.
- `WorkflowManager.ResumeWorkflowAsync` runs the pinned version. It also stops failing with a `NullReferenceException` when the blocking activity isn't in the definition: it logs a warning, faults the instance and returns.
- Pinned definitions for the HTTP module:
  - `WorkflowRoutesHandler` and `WorkflowInstanceRouteEntries` register the route entries of a halted instance from its pinned version;
  - `HttpWorkflowController.Invoke`, resuming an instance through a token, finds the activity in the head first, then in the versions of the type, so an instance pinned to a version whose activity was later removed can still be resumed.
- `RestartWorkflowAsync` is unchanged (V6); document it.
- **Tests** (extend `test/OrchardCore.Tests/Workflows/WorkflowManagerTests.cs`, with a version store fake):
  - an instance started on version 1 resumes on version 1 after version 2 is published, including when version 2 removes or rewires the activities after the blocking one;
  - an unpinned instance resumes on the head;
  - a missing pinned version falls back to the head;
  - a blocking activity missing from the definition faults the instance instead of throwing;
  - route entries of a halted instance come from its pinned version (SiteContext, in `WorkflowDesignerControllerTests` style).
- **Notes from implementing this step:**
  - **Materializing a version.** `WorkflowTypeVersion.ToWorkflowType(head)` (in `A/Helpers/WorkflowTypeVersionExtensions.cs`) builds the transient definition, with deep-cloned activity properties. The version store caches the versions it loads for the scope, since resuming many instances loads the same version repeatedly.
  - **Missing blocking activity.** `ResumeWorkflowAsync` logs a warning, faults the instance with a message naming the activity, and saves it. Before, it threw a `NullReferenceException`.
  - **HTTP.** `HttpWorkflowController.Invoke` looks for an activity removed from the current definition in the versions, newest first, and then only resumes instances waiting on it: it never starts a new instance from it. `WorkflowInstanceRouteEntries` also skips instances whose workflow type no longer exists, instead of throwing.
  - **Existing quirk, unchanged.** `IWorkflowStore.GetAsync(string workflowId)` queries `WorkflowBlockingActivitiesIndex`, so it doesn't find instances that aren't waiting on anything (finished ones). The tests query `WorkflowIndex` instead.
  - **Tests.**
    - `Versioning/WorkflowVersionPinningTests.cs` runs the real engine on the real stores (SQLite, through the shared `VersioningTestDatabase`): a new instance is pinned; a pinned instance resumes on version 1 after version 2 replaces its last activity; an unpinned one runs version 2; a missing blocking activity faults the instance.
    - `WorkflowTypeVersionStoreTests` covers `GetWorkflowTypeAsync`: an earlier version keeps the current identity and name; no, the current or a missing version id returns the workflow type itself.
    - `Versioning/WorkflowVersionRoutesTests.cs` (SiteContext): the route entries of an instance pinned to version 1 come from version 1; an HTTP request token for an activity removed in version 2 still resumes the version 1 instance.
    - Workflows tests: 87/87.

### - [x] 2.3 Recipes import a new version

- `WorkflowTypeStep`: when the type exists, copy the imported definition (name, settings, activities, transitions, entity properties) onto it and save it through `IWorkflowTypeStore.SaveAsync`, which creates the next version; discard its draft. New types are unchanged (regenerated HTTP URLs).
- **Tests**: re-importing a type keeps its document id and its instances, creates version 2, discards the draft, and an instance pinned to version 1 still resumes on version 1.
- Release notes: re-importing no longer deletes instances.
- **Notes from implementing this step:**
  - The step copies the name, settings, activities, transitions and entity properties onto the existing document and discards the draft through `IWorkflowTypeDraftManager` (it used to be deleted by the type's `DeletedAsync` handler).
  - **Test.** `Versioning/WorkflowVersionRecipeTests.cs` (SiteContext): an instance waits on a signal of version 1; a recipe imports a definition without that activity; the type keeps its document id and becomes version 2, and the instance, still pinned to version 1, resumes with the signal and finishes. The existing `WorkflowTypeRecipeStep_ReplacesTypeWithDraft_DeletesDraft` still passes. Workflows tests: 88/88.

### - [x] 2.4 Retention

- `WorkflowVersionOptions` (`MaxCount`, default 0) bound to `Workflows:Versions` (`M/ConfigurationSchema.json`).
- After creating a version, delete the oldest versions beyond `MaxCount`, skipping the published one and those referenced by `WorkflowIndex.WorkflowTypeVersionId`.
- **Tests**: keeps everything by default; prunes beyond the limit; never deletes a pinned version or the published one.
- **Notes from implementing this step:**
  - **Rule.** `MaxCount` keeps the N most recent versions (the published one is always among them); each older version is deleted unless a `WorkflowIndex` row references it. Versions kept for instances don't count toward the limit, so with `MaxCount = 2` and an instance on version 1, publishing version 4 keeps versions 4, 3 and 1.
  - **Where.** `WorkflowTypeVersionStore.CreateIfChangedAsync` prunes right after it creates a version, so nothing changes until the next publish after the limit is set. Unpinned instances (created before versions existed) don't reference a version and keep nothing.
  - **Configuration.** `WorkflowVersionOptions` (`A/Services`) is bound to `Workflows:Versions` and described in `M/ConfigurationSchema.json`. It's a new section, so it has no legacy name.
  - **Tests.** `Versioning/WorkflowVersionRetentionTests.cs`: no limit keeps 5 of 5; a limit of 2 keeps versions 4 and 3; a version an instance runs on is kept. Workflows tests: 91/91.

### - [x] 2.5 Designer API

- `Definition` and the publish result include the published version (`versionId`, `version`, `createdUtc`, `createdBy`).
- `GET Versions`: the versions of the type, newest first, with the number of instances pinned to each.
- `GET Version?versionId=`: a version as a read-only designer definition (nodes rendered from the snapshot).
- `GET Compare?from=&to=`: two definitions (a version id or `draft`) plus their differences: activities and transitions added, removed and changed, and changed settings. The diff lives in a tested service (`M/Services/WorkflowTypeDiff.cs`).
- `POST Restore` (`versionId`, `revision`): writes the version into the draft through the draft manager (conflict-checked like every draft change).
- `Instance` renders the instance's pinned version and returns its number.
- **Tests**: each endpoint (SiteContext), the diff rules, restore conflicts, 403 without `ManageWorkflows`.
- **Notes from implementing this step:**
  - **Shapes.** Versions are `WorkflowDesignerVersion` (`versionId`, `version`, `name`, `createdUtc`, `createdBy`, `isPublished`, `instanceCount`). `WorkflowDesignerDefinition` gains `publishedVersion` and `version` (the version a version page or an instance shows). `Publish` returns `version`. `Versions` returns `{ versions, draft: { revision, modifiedUtc, modifiedBy } | null }`; `Compare` returns `{ from, to, changes }`; `Restore` returns `{ revision, issues }`, or the usual 409 problem.
  - **Diff rules** (`WorkflowTypeDiff`, public so the test project can reach it): activities are added, removed, changed (type, start flag or properties) or only moved (a changed activity isn't also reported as moved); transitions are compared by their `{source}:{outcome}:{destination}` keys, so a rewired one is a removed key plus an added one; settings list the changed names among `Name` and the execution settings.
  - **Names.** A version shown on its own (version page, compare) carries the name it had then; an instance shows the current name.
  - **Instance counts** load the `WorkflowIndex` rows of the type and group them by version. Instances created before versions existed aren't counted.
  - **Restore** keeps the draft's name and enabled state (V1), and creates the draft when there's none.
  - **Config.** The designer config has `versions`, `version`, `compare` and `restore` URLs (null in the instance viewer). The pages that use them come in step 2.6.
  - **Tests.** `Versioning/WorkflowTypeDiffTests.cs` (5) and `WorkflowDesignerControllerTests` (6 more: versions with instance counts and the publish result, an earlier version's graph, a version of another type, compare with the draft, restore and its conflict, a pinned instance's version; the forbidden test now covers the new endpoints). Workflows tests: 102/102.

### - [ ] 2.6 Designer UI

- Toolbar: the published version ("Version 3"), and a **Versions** button.
- **Versions dialog**: list with dates, authors, published badge and pinned instance counts; **View** (read-only page), **Compare** (with the published version, or the draft when there is one), **Restore** (confirms when the draft has changes, then reloads the definition).
- **Version page** `Admin/Workflows/Types/{id}/Versions/{versionId}`: the read-only designer for a version.
- **Compare page**: two read-only canvases side by side, sharing the zoom; added activities highlighted on the right, removed ones on the left, changed ones on both; a list of the changes below.
- The publish dialog no longer warns that running instances move to the new definition; it says they keep their version.
- The instance page shows "Version N" (and "not the published version" when it isn't).
- **Tests** (Vitest): toolbar version, dialog actions, restore confirmation, compare highlighting, publish dialog text, instance version label.

### - [ ] 2.7 Instances list, docs and release notes

- The instances list shows each instance's version.
- Docs: a **Versions** section in `src/docs/reference/modules/Workflows/README.md` (publishing, pinning, restart, history, retention setting, recipes).
- Release notes: versioning, pinned instances, recipe import change, `WorkflowType.VersionId`, `Workflow.WorkflowTypeVersionId`, `IWorkflowTypeVersionStore`, the new `WorkflowManager` dependency.

### - [ ] 2.8 End-to-end tests

- Publish version 2 while an instance is halted on the seeded workflow's signal; the instance page shows version 1, and triggering the signal finishes the instance on version 1.
- The versions dialog lists both versions; compare highlights the change; restoring version 1 creates a draft that matches it.

## Definition of done (Phase 2)

- [ ] Steps 2.1–2.8 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Upgrading a site with existing workflow types and halted instances creates version 1 per type, and the instances still resume.
- [ ] Docs and release notes are updated.

## Open questions

Decided with the defaults above unless the maintainer says otherwise:

- V7 changes what re-importing a recipe does to running instances (kept instead of deleted). Is that wanted for every recipe, or only behind an option?
- V8 keeps every version by default. Should there be a default limit?
- V6: should restarting a faulted instance offer "on its own version" too?

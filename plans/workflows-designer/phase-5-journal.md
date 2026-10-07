# Phase 5 — Execution journal and recovery

Record what each instance ran, show it in the instance viewer, and retry a faulted activity in place instead of restarting the instance. This turns the sketch in [`later-phases.md`](later-phases.md#phase-5--execution-journal-and-recovery) into steps.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
Instance #42 (Faulted)                         ┌ Properties ───────────────────────┐
                                               │ Activity | Variables | Journal    │
 [Request]══1══▶[Fork]══1══▶[Send email ✖]      │ #1 Request     Done      0 ms     │
                  ║                            │ #2 Fork        A, B      1 ms     │
                  ╚══1══▶[Notify ✔]            │ #3 Notify      Done      2 ms     │
                                               │ #4 Send email  Faulted   40 ms    │
 executed path highlighted, with counts        │    SMTP server not reachable      │
                                               │ [↻ Retry from Send email]          │
                                               └───────────────────────────────────┘
```

## Decisions

| # | Decision | Why |
|---|---|---|
| J1 | **Journal records** are `WorkflowExecutionRecord` documents in their own YesSql collection (`WorkflowJournal`), with an index on the instance and the sequence. A record holds the instance, the activity (id, name, title), its status (`Completed`, `Halted`, `Faulted`), its outcomes, its start and end, the error message, and a sequence number per instance. No input or output snapshots. | `Workflow.State` doesn't grow, and the journal can be read and trimmed on its own. Snapshots can hold personal data; they are left for later. |
| J2 | **Writing**: `WorkflowManager.ExecuteWorkflowAsync` adds an entry per activity it runs (and per resume) to a buffer of the execution context; the entries are saved with the instance, in `PersistAsync`, through `IWorkflowExecutionJournal`. An instance that isn't saved (deleted when finished) saves no journal. | One write per execution cycle; the journal matches what was saved. |
| J3 | **Settings** `OrchardCore:Workflows:Journal`: `Enabled` (default `true`) and `MaxRecordsPerInstance` (default `1000`; the oldest records go first). | Bounded storage for long-running loops. |
| J4 | `WorkflowExecutionContext.ExecutedActivities` is filled too (one entry per outcome taken, the last 100), so `WorkflowState.ExecutedActivities` finally holds something. | It was declared but never written. |
| J5 | **Deleting an instance deletes its journal**: `IWorkflowStore.DeleteAsync` (through a workflow handler), the trimming service, and deleting a workflow type. | No orphaned records. |
| J6 | **Viewer**: the `Instance` endpoint returns the journal (the most recent 500 records) and, from it, how many times each activity ran and each transition was taken. The read-only canvas highlights the executed activities and transitions, with a count when it's more than one, and marks the faulted activity. A **Journal** tab lists the records. | The executed path is the first thing to look at when an instance misbehaves. |
| J7 | **Retry**: `IWorkflowManager.RetryActivityAsync(workflow, activityId)` runs a faulted instance again from an activity of its definition, with the instance's current state, under the instance's lock. The viewer offers **Retry from here** on the faulted activity (and any other activity) of a faulted instance, to users with `ExecuteWorkflows`. | Recover without losing the state, after fixing what made the activity fail. |
| J8 | **No incident strategy** in this phase (continuing on a `Failed` outcome instead of faulting): activities that can fail already declare a `Failed` outcome. | Recorded as an open question. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 5.1 Journal records and store

- `A/Models/WorkflowExecutionRecord.cs`, `WorkflowExecutionRecordStatus`; `A/Services/IWorkflowExecutionJournal.cs` (`SaveAsync(entries)`, `ListAsync(workflowId, count)`, `DeleteAsync(workflowIds)`); `M/Indexes/WorkflowExecutionRecordIndex.cs`; `M/Services/WorkflowExecutionJournal.cs` with the cap (J3); options `WorkflowJournalOptions`; migration creating the index table in the `WorkflowJournal` collection.
- Deleting instances deletes their journal (J5).
- **Tests**: save and list in order, the cap, delete; a deleted instance (store, trimming, type deletion) loses its records.
- **Notes from implementing this step:**
  - **Collection.** `WorkflowExecutionRecord.Collection` (`WorkflowJournal`) is registered in `StoreCollectionOptions`, as AuditTrail does, so the shell initializes it. `UpdateFrom6Async` creates `WorkflowExecutionRecordIndex` (`WorkflowId`, `Sequence`) in it.
  - **Journal.**
    - `SaveAsync(workflowId, records)` saves records with the sequence numbers they have (step 5.2 numbers them). It then deletes the oldest records beyond `MaxRecordsPerInstance`.
    - `ListAsync(workflowId, count)` returns the most recent records, oldest first.
    - `DeleteAsync(workflowIds)` deletes by batches of 100 instances.
    - `IsEnabled` reflects `Enabled`.
  - **Settings.** `WorkflowJournalOptions` is bound to `Workflows:Journal` and described in `ConfigurationSchema.json`.
  - **Deletions.**
    - `WorkflowJournalHandler` (a workflow handler) handles `IWorkflowStore.DeleteAsync`.
    - `WorkflowTrimmingService` and `WorkflowTypeStore.DeleteAsync` delete instances through the session, so they delete the journal themselves; both take an `IWorkflowExecutionJournal` (release notes).
  - **Tests.**
    - `Journal/WorkflowExecutionJournalTests.cs` (7): save and list, the most recent records, the cap, delete, workflow type deletion, trimming, and the handler.
    - The SQLite test database initializes the collection and runs the new migration.
    - Workflows tests: 229/229.

### - [x] 5.2 Recording executions

- `WorkflowExecutionContext` buffers journal entries and fills `ExecutedActivities` (J4); `WorkflowManager` adds an entry per executed or resumed activity (completed with its outcomes, halted, faulted with the error) and saves them in `PersistAsync` when the journal is enabled.
- **Tests**: a branching workflow records the order and outcomes; a halted then resumed workflow continues the sequence; a fault records the error; a disabled journal records nothing; `ExecutedActivities` is filled.
- **Notes from implementing this step:**
  - **Context.**
    - `WorkflowExecutionContext.RecordExecution(activityContext, status, outcomes, startedUtc, completedUtc, isResume, error)` adds a record to `JournalRecords`, numbered from `ExecutionSequence`, with the activity's title.
    - It also pushes one `ExecutedActivity` per outcome (or one without an outcome) and keeps the most recent `MaxExecutedActivities` (100).
    - `WorkflowState.ExecutionSequence` keeps the last number between runs.
  - **Engine.** `ExecuteWorkflowAsync` records each activity:
    - halted (waiting on an event) without outcomes, and completed with its outcomes, after its output bindings;
    - faulted with the exception's message, before the fault handler runs;
    - an event resumed by the instance with `IsResume`.
  - **Saving.** `PersistAsync` saves the records through `IWorkflowExecutionJournal` when it is enabled, then clears them. An instance deleted when it finishes saves no journal.
  - **Order fix.** `ExecutedActivities` was saved from its stack top first, so every save and reload reversed it. That was harmless while it was never filled. It's now saved oldest first.
  - **API change.** `WorkflowManager` takes an `IWorkflowExecutionJournal` (release notes).
  - **Tests.**
    - In `WorkflowManagerTests` (5): the records and the saved state of a finished run; a halt then a resume continuing the sequence; a fault's error; a disabled journal; the cap on executed activities.
    - Workflows tests: 234/234.

### - [x] 5.3 Retry a faulted activity

- `IWorkflowManager.RetryActivityAsync(workflow, activityId)`: only for a faulted instance and an activity of its definition; clears the fault, runs from that activity with the instance's state, saves; under the instance lock.
- `POST Designer/Retry` (`instanceId`, `activityId`), requiring `ExecuteWorkflows`.
- **Tests**: retry continues after the input of the faulting activity is fixed; a non-faulted instance or an unknown activity is rejected; 403 without the permission.
- **Notes from implementing this step:**
  - **Engine.** `RetryActivityAsync` reads the definition the instance runs on (its version), then:
    - throws `InvalidOperationException` for an instance that isn't faulted or whose workflow type is gone, and `ArgumentException` for an activity the definition doesn't have;
    - takes the instance's lock when it is atomic, and returns `null` when the lock is held;
    - clears `FaultMessage`, runs from the activity with the instance's state (properties, variables and activity states), and saves the instance (or deletes it, when it finishes and its type deletes finished instances).
    The journal goes on numbering.
  - **Endpoint.** `Retry` requires `ExecuteWorkflows`. It answers 404 for an instance of another type, 400 for one that isn't faulted or an unknown activity, 409 when the lock is held, and otherwise the new `status` and `faultMessage`.
  - **Configuration.** The instance page passes `canRetry` (the user's `ExecuteWorkflows` permission) to the viewer's configuration, which then has the `retry` URL.
  - **Tests.**
    - In `WorkflowManagerTests` (2): retrying after a transient failure finishes the instance and continues the journal (with a `FlakyTask`); a finished instance and an unknown activity are rejected.
    - In `WorkflowDesignerControllerTests` (2, plus checks in 2 others): retrying a faulted instance finishes it, saves it and records the activity; a finished instance, an unknown activity and an instance of another type are rejected; the endpoint is forbidden without the permission; the instance page's configuration has the URL.
    - The test instances are created with `IWorkflowManager.NewWorkflow`, so their state holds their activities, as real instances' does.
    - Workflows tests: 238/238.

### - [x] 5.4 Instance viewer

- The `Instance` endpoint returns `journal`, `executedActivityCounts`, `executedTransitionCounts` and `faultedActivityId`; the designer's read-only canvas highlights the executed path with counts and the faulted activity; a **Journal** tab; **Retry from here** in the activity summary of a faulted instance.
- **Tests**: endpoint (a branching instance's counts); Vitest for the highlighting, the tab and the retry action.
- **Notes from implementing this step:**
  - **Endpoint.** `Instance` adds to the instance:
    - `faultMessage`;
    - `faultedActivityId`, the last faulted record of a faulted instance;
    - `journal`, the most recent 500 records (`WorkflowDesignerJournalRecord`);
    - `executedActivityCounts`, and `executedTransitionCounts` by transition key, following the first transition of each outcome as the engine does.
  - **Durations.** Orchard Core writes dates to the second (`DateTimeJsonConverter`), so `WorkflowExecutionRecord.DurationMilliseconds` stores the duration on its own.
  - **Canvas.**
    - Executed activities have a green edge and, when they ran more than once, a `×n` badge.
    - The faulted activity has a red ring and badge.
    - Taken transitions are green, with `×n` on their label when taken more than once.
    - The legend explains the colors.
  - **Journal tab** (`panel/JournalTab.vue`, viewer of an instance only). It lists each record's sequence, title, status, `Resumed`, outcomes, duration and error. Selecting a record selects and centers its activity.
  - **Retry.** With `urls.retry` (the user can execute workflows), the activity summary of a faulted instance shows the fault message on the faulted activity and **Retry from here** on every activity. After a confirmation, it posts `Retry`, reloads the instance and shows the new status.
  - **Strings.** 18 new strings; `Retry` was already defined.
  - **Tests.**
    - Controller: `Instance_Journal_ReturnsTheRecordsTheExecutedPathAndTheFault`.
    - Vitest: `JournalTab.spec.ts` (3), the node and edge highlighting, and the viewer's highlighting, Journal tab and retry flow, and no retry without the permission. 223/223.
    - Workflows tests: 239/239.

### - [x] 5.5 Docs and release notes

- `src/docs/reference/modules/Workflows/README.md`: the journal, its settings, the executed path, retrying.
- Release notes.
- **Notes from implementing this step:**
  - **Workflows README.** A new **Execution Journal** section (after Versions):
    - what's recorded and when it's deleted, and the executed path and Journal tab of the instance page;
    - **Retrying a Faulted Instance**, which requires Execute workflows;
    - **Journal Settings**, with an `appsettings.json` example and a table;
    - **Journal for Developers**.
    The Workflow Instances paragraph links to it.
  - **Release notes.** A **Workflow Execution Journal** feature section, and a **Journal** breaking-change bullet:
    - the constructors of `WorkflowManager`, `WorkflowTypeStore` and `WorkflowTrimmingService`;
    - `IWorkflowManager.RetryActivityAsync`;
    - `WorkflowState.ExecutedActivities` being filled.

### - [x] 5.6 End-to-end test

- A seeded workflow faults on its first run (a Script that throws unless a property is set). The test opens the faulted instance, sees the executed path and the faulted activity in the journal, retries after the condition is fixed, and the instance finishes.
- **Notes from implementing this step:**
  - **Why not a Script.** A script error doesn't fault a workflow: the JavaScript evaluator logs it and returns `null` (see Phase 3, 3.2).
  - **Transient failure.** The `WorkflowsSample` test module adds a **Transient failure** activity (`TransientFailureTask`). It throws `TransientFailureException` the first time it runs in an instance, and remembers that in its activity state, which is saved with the faulted instance. A retry then succeeds, like a call to a service that was briefly down.
    - The module now builds with the Razor SDK, for the activity's Design and Thumbnail views.
    - The functional host ignores the error the engine logs for this exception, which would otherwise fail the test (`CmsTestBase` fails on any logged error).
  - **Seed.** "Transient failure" (`wfdtransientfailure`): an HTTP request, then the activity, then a Set Property with a Literal value.
  - **Test.** `Journal_FaultedInstance_ShowsWhatRanAndIsRetried`:
    - calls the URL, which faults the instance;
    - checks the instance page: the request and its connection are executed, the activity is faulted, the Set Property didn't run, and the Journal tab shows the error;
    - selects the record, retries from the activity, confirms, and sees "Finished", the Set Property executed and no retry left.
  - **Results.** `WorkflowsDesignerTests` 21/21.

## Definition of done (Phase 5)

- [ ] Steps 5.1–5.6 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Docs and release notes are updated.

## Open questions

- J1: should the journal optionally hold input and output snapshots, with a privacy setting?
- J8: should a workflow type be able to continue on a `Failed` outcome instead of faulting?

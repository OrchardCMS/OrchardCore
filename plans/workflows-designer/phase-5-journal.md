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

### - [ ] 5.1 Journal records and store

- `A/Models/WorkflowExecutionRecord.cs`, `WorkflowExecutionRecordStatus`; `A/Services/IWorkflowExecutionJournal.cs` (`SaveAsync(entries)`, `ListAsync(workflowId, count)`, `DeleteAsync(workflowIds)`); `M/Indexes/WorkflowExecutionRecordIndex.cs`; `M/Services/WorkflowExecutionJournal.cs` with the cap (J3); options `WorkflowJournalOptions`; migration creating the index table in the `WorkflowJournal` collection.
- Deleting instances deletes their journal (J5).
- **Tests**: save and list in order, the cap, delete; a deleted instance (store, trimming, type deletion) loses its records.

### - [ ] 5.2 Recording executions

- `WorkflowExecutionContext` buffers journal entries and fills `ExecutedActivities` (J4); `WorkflowManager` adds an entry per executed or resumed activity (completed with its outcomes, halted, faulted with the error) and saves them in `PersistAsync` when the journal is enabled.
- **Tests**: a branching workflow records the order and outcomes; a halted then resumed workflow continues the sequence; a fault records the error; a disabled journal records nothing; `ExecutedActivities` is filled.

### - [ ] 5.3 Retry a faulted activity

- `IWorkflowManager.RetryActivityAsync(workflow, activityId)`: only for a faulted instance and an activity of its definition; clears the fault, runs from that activity with the instance's state, saves; under the instance lock.
- `POST Designer/Retry` (`instanceId`, `activityId`), requiring `ExecuteWorkflows`.
- **Tests**: retry continues after the input of the faulting activity is fixed; a non-faulted instance or an unknown activity is rejected; 403 without the permission.

### - [ ] 5.4 Instance viewer

- The `Instance` endpoint returns `journal`, `executedActivityCounts`, `executedTransitionCounts` and `faultedActivityId`; the designer's read-only canvas highlights the executed path with counts and the faulted activity; a **Journal** tab; **Retry from here** in the activity summary of a faulted instance.
- **Tests**: endpoint (a branching instance's counts); Vitest for the highlighting, the tab and the retry action.

### - [ ] 5.5 Docs and release notes

- `src/docs/reference/modules/Workflows/README.md`: the journal, its settings, the executed path, retrying.
- Release notes.

### - [ ] 5.6 End-to-end test

- A seeded workflow faults on its first run (a Script that throws unless a property is set). The test opens the faulted instance, sees the executed path and the faulted activity in the journal, retries after the condition is fixed, and the instance finishes.

## Definition of done (Phase 5)

- [ ] Steps 5.1–5.6 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Docs and release notes are updated.

## Open questions

- J1: should the journal optionally hold input and output snapshots, with a privacy setting?
- J8: should a workflow type be able to continue on a `Failed` outcome instead of faulting?

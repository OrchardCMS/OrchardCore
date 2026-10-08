# Phase 10 — Inspecting, testing and operating workflows

Make it possible to see what each step of a run did, to test a workflow from the designer, and to operate instances at scale. This phase comes from comparing the built features with Elsa Workflows 3.8 (October 2026): the authoring and engine features match, but Elsa shows what each activity received and produced, retries transient failures by itself, runs a workflow from its designer, starts one instance per correlation id, and operates instances across workflows. These are the six items worth adding; the others (composition, state machines, BPMN, C# expressions) stay out, see [Phase 8](phase-8-evaluation.md#evaluation).

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## The six items

| # | Item | Steps | Status |
|---|---|---|---|
| 1 | Each activity's data in the journal: the expressions it evaluated, the outputs it set, the variables it changed, its last result | 10.1–10.4 | In progress (10.1 done) |
| 2 | Retry policies per activity, and what a failure does | 10.5 | Not started |
| 3 | Run a workflow from the designer, with an input | 10.6 | Not started |
| 4 | One running instance per correlation id | 10.7 | Not started |
| 5 | Instances across workflows: filters, bulk actions | 10.8 | Not started |
| 6 | Links between a parent instance and the child instances it ran | 10.9 | Not started |

## Target experience (item 1)

```
Instance page                                   Activity panel of "Big order?" (Runs tab)
  ... (Price the line) ─▶ (Big order?) ─▶ ...     #5  Completed  2 ms  True                       [▾]
                                                      Expressions
  Journal                                               Condition  JavaScript  variable('lineTotal') >= 50  →  true
   #4 Price the line  Completed  Done                Last result  —
   #5 Big order?      Completed  True  ◀ selected  #9  Completed  1 ms  False                      [▸]
```

## Decisions

| # | Decision | Why |
|---|---|---|
| O1 | A journal record can carry the **data of the activity's execution**: each expression it evaluated (its syntax, its text, its result, and the property it belongs to when known), the outputs it set, the variables it changed (their new values), and its last result when it set one. | It answers "with what did this step run, and what did it produce?", which the status, outcomes and error don't. |
| O2 | Data is recorded only when the workflow's setting **Record activity data** is on. It's part of the workflow's versions; existing workflows have it off, and the page that creates a workflow checks it. | Values can be large and sensitive (personal data, keys), so recording them is a choice. New workflows get it for debugging. |
| O3 | Values are stored as JSON text, each **capped** at 2,000 characters and a record's data at 32,000; what's cut is marked, never silently dropped. | The journal stays bounded; the trimming of instances deletes it with them. |
| O4 | The JavaScript and Liquid evaluators and the Literal syntax **report the evaluations** to the execution context, as they report script errors (Phase 9). A custom syntax can report its own with `WorkflowExecutionContext.ReportEvaluation`. Outputs are reported by `SetActivityOutput`. Variables are compared before and after the activity. | Activities that call the evaluators directly, as most built-in ones do, are covered without changing them. Comparing the variables catches every way of writing them (`setVariable`, `setProperty`, Set Variable, output bindings). |
| O5 | The instance's definition only says which records have data; the viewer **loads a record's data when it's opened**, from an endpoint that checks the instance's permission like the instance page. | 500 records with data would make the instance page heavy. |
| O6 | In the instance viewer, the activity panel gets a **Runs** tab: the selected activity's executions, newest last, each with its status, duration and outcomes, which opens on its data. Selecting a journal record opens the panel there, on that record. | The journal says what ran; the activity panel, which is about the selected activity, says what it did (the split of the workflow and activity panels). |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 10.1 Record the activity data

- `A/`: `WorkflowType.RecordActivityData`; `WorkflowExecutionRecord.Data` (`WorkflowExecutionData`: evaluations, outputs, variables, last result, whether something was cut); `WorkflowExecutionContext.ReportEvaluation` and the outputs it already keeps.
- The JavaScript and Liquid evaluators and the Literal provider report their evaluations; the engine collects them, the outputs, the changed variables and the last result around each activity when the setting is on, with the caps of O3.
- The setting is part of the draft, the versions, their comparison, the recipe step and the settings form (the designer's Workflow tab, and the create and clone pages).
- **Tests**: a run with the setting records the evaluations (with the property name), outputs, changed variables and last result; without it, nothing; a large value is cut and marked.
- **Notes from implementing this step:**
  - **API (`A/`).**
    - `WorkflowType.RecordActivityData`, and the same on `WorkflowTypeVersion`.
    - `WorkflowExecutionRecord.Data` is a `WorkflowExecutionData`: `Evaluations` (`WorkflowExpressionEvaluation`: `Property`, `Syntax`, `Expression`, `Result`), `Outputs`, `Variables`, `Properties` (the workflow properties that aren't declared variables) and `LastResult`, as JSON text, with `IsTruncated`. A record without anything has no `Data`.
    - `WorkflowExecutionContext.ReportEvaluation(syntax, expression, result)` (ignored unless the workflow records activity data), `RecordsActivityData`, and `TakeReportedData()`, which the engine calls around each activity. `SetActivityOutput` reports the output too. `RecordExecution` takes the data.
    - `WorkflowExecutionData.FormatValue` serializes a value with `JOptions.Default` (as text when it can't be serialized) and cuts it at 2,000 characters with "…".
  - **Reporting.** The JavaScript evaluator, the Liquid evaluator (both its value and template paths) and the Literal syntax report their evaluations, formatted when they're reported, since an object can change later in the run. A failed script reports its error, not an evaluation.
  - **Engine.** `ActivityDataRecorder` (`M/Services/`) starts before each activity: it drops the reports made before, and snapshots the workflow properties and the last result. Once the activity completed, halted or faulted, it collects the reports, finds the property of each expression by matching its text in the activity's properties (`Condition`, `Script`, `Inputs.amount`), compares the properties (declared variables apart from the others) and the last result, and keeps the record within 32,000 characters (last result, outputs, variables, evaluations, then properties).
  - **Setting.** The checkbox **Record activity data** is on the designer's Workflow tab and the create and clone pages; the create page checks it, so new workflows record their activities' data and existing ones don't (O2). It's part of the draft, the versions (and their fingerprint), the comparison and the recipe step.
  - **Tests.**
    - `WorkflowManagerTests`: the outputs and the changed variable are recorded only with the setting; a script's evaluation is recorded with its property, syntax and result, its last result and its output; a 3,000-character value is cut and the record marked.
    - The setting's existing tests now cover it too: the Workflow tab, the draft, publishing, the comparison and the version fingerprint. `Create_NewWorkflow_RecordsActivityDataByDefault`, and Clone copies it.
    - Workflows tests: 291/291.

### - [x] 10.2 Read a record's data

- The instance's journal records say whether they have data (`hasData`); `GET …/Designer/JournalData?instanceId={id}&sequence={n}` returns a record's data, with the instance page's permission.
- **Tests**: the endpoint returns the data, 404 for an unknown record, and 403 without the permission.
- **Notes from implementing this step:**
  - **Route.** The action follows the designer controller's other actions (`Designer/{action}` with query string parameters) rather than the path sketched above. The viewer's configuration has its URL (`urls.journalData`), without the sequence.
  - **API (`A/`).** `IWorkflowExecutionJournal.GetAsync(workflowId, sequence)` returns one record, through the index on the workflow id and the sequence.
  - **Instance.** `WorkflowDesignerJournalRecord.HasData`; the records still don't carry their data.
  - **Tests.** `JournalData_RecordOfTheInstance_ReturnsItsDataWhenItHasSome` (the flags, the data, and 404 for a record without data or that doesn't exist); the viewer's configuration has the URL; the functional test of the designer endpoints for a user without Manage Workflows includes it (403). Workflows tests: 292/292.

### - [ ] 10.3 The Runs tab

- The viewer's activity panel has a **Runs** tab (when the instance ran the activity): its executions, each opening on its data (expressions, outputs, changed variables, last result), loaded when opened.
- Selecting a journal record opens the panel on the Runs tab, on that record; a record without data says how to turn the setting on.
- **Tests**: Vitest for the tab, the loading and the selection from the journal.

### - [ ] 10.4 Docs and an end-to-end test (item 1)

- Docs: the setting, the Runs tab, what's recorded and its caps; release notes.
- **Functional test**: a run of a seeded workflow with the setting shows the condition's value of an If/Else in the Runs tab.

### - [ ] 10.5 Retry policies (item 2)

To detail when it starts. Sketch: a task can retry when it faults, with a number of attempts and a delay (fixed or growing), set on its activity panel; between attempts the instance waits like on a timer, so a restart doesn't lose it. A workflow setting chooses what a fault does once the attempts are spent: fault the instance (today) or go on with a **Failed** outcome when the activity has one. The journal records each attempt.

### - [ ] 10.6 Run from the designer (item 3)

To detail when it starts. Sketch: **Run…** in the designer's toolbar for a workflow started by an HTTP request or usable as an activity: it asks for the inputs (the input variables, or a query string and a body), runs the published version or the draft, and opens the instance.

### - [ ] 10.7 One instance per correlation id (item 4)

To detail when it starts. Sketch: the workflow's setting **Single instance** becomes a choice: any number of instances, one at a time (today's singleton), or one per correlation id, which the events that correlate (content, users) honor.

### - [ ] 10.8 Instances across workflows (item 5)

To detail when it starts. Sketch: an **Instances** page under Workflows that lists the instances of every workflow, filtered by workflow, status (Executing, Halted, Faulted, Finished, Aborted) and date, with bulk Retry, Cancel and Delete, and counts by status.

### - [ ] 10.9 Parent and child instances (item 6)

To detail when it starts. Sketch: an Execute Workflow task records the instance it started, which its Runs tab links to; a child instance links back to its parent.

### - [ ] 10.10 Definition of done

## Definition of done (Phase 10)

- [ ] Steps 10.1–10.9 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Workflows run as before when the new settings are off.
- [ ] Docs and release notes are updated.

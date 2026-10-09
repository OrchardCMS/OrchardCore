# Phase 10 — Inspecting, testing and operating workflows

Make it possible to see what each step of a run did, to test a workflow from the designer, and to operate instances at scale. This phase comes from comparing the built features with Elsa Workflows 3.8 (October 2026): the authoring and engine features match, but Elsa shows what each activity received and produced, retries transient failures by itself, runs a workflow from its designer, starts one instance per correlation id, and operates instances across workflows. These are the six items worth adding; the others (composition, state machines, BPMN, C# expressions) stay out, see [Phase 8](phase-8-evaluation.md#evaluation).

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## The six items

| # | Item | Steps | Status |
|---|---|---|---|
| 1 | Each activity's data in the journal: the expressions it evaluated, the outputs it set, the variables it changed, its last result | 10.1–10.4 | Done |
| 2 | Retry policies per activity, and what a failure does | 10.5 | Done |
| 3 | Run a workflow from the designer, with an input | 10.6 | Done |
| 4 | One running instance per correlation id | 10.7 | Done |
| 5 | Instances across workflows: filters, bulk actions | 10.8 | Done |
| 6 | Links between a parent instance and the child instances it ran | 10.9 | Done |

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

### - [x] 10.3 The Runs tab

- The viewer's activity panel has a **Runs** tab (when the instance ran the activity): its executions, each opening on its data (expressions, outputs, changed variables, last result), loaded when opened.
- Selecting a journal record opens the panel on the Runs tab, on that record; a record without data says how to turn the setting on.
- **Tests**: Vitest for the tab, the loading and the selection from the journal.
- **Notes from implementing this step:**
  - **Tab.** `panel/RunsTab.vue`, in the viewer's activity panel between Details and Outputs, when the journal has a record of the activity. Each run shows its sequence, status, outcomes, start time and duration, and opens (an `aria-expanded` button) on its error and its data: the expressions with their property and syntax and an arrow to their result, the outputs, the variables, the other properties and the last result. JSON objects are shown indented; a record whose values were cut says so.
  - **Loading.** A run's data is loaded the first time it's opened (`api.getJournalData(sequence)`, with the URL from the configuration), and kept while the panel shows the activity. A run without data says the activity changed nothing or the workflow doesn't record activity data, and where to turn it on.
  - **From the journal.** `JournalTab` emits the record's sequence with its activity; the workflow panel stores it in `state.focusedRunSequence` and selects the activity, whose panel opens on the Runs tab with that run open and scrolled into view. Selecting another activity on the canvas opens on its Details.
  - **Tests.** `RunsTab.spec.ts` (3): the runs of the activity and the loading of a run's data (once), a run without data, and a journal record opening its run in the activity panel. `JournalTab.spec.ts` checks the sequence. Vitest 279/279.

### - [x] 10.4 Docs and an end-to-end test (item 1)

- Docs: the setting, the Runs tab, what's recorded and its caps; release notes.
- **Functional test**: a run of a seeded workflow with the setting shows the condition's value of an If/Else in the Runs tab.
- **Notes from implementing this step:**
  - **Docs.** "Recording What Each Activity Did" in the Execution Journal section: the setting, what a record keeps (expressions with their setting, syntax and result; outputs; changed variables and properties; last result), the caps, the default (checked for workflows created in the admin), the sensitive-data warning, and `ReportEvaluation` for custom syntaxes. The Journal and Runs tabs are described with the instance page. Release notes, under the execution journal.
  - **Functional test.** `RecordActivityData_RunOfAnIfElse_ShowsItsConditionAndItsValueInTheRunsTab`, with the seeded "Recorded condition" workflow (HTTP request, If/Else `6 * 7 > 40`, response): its journal record opens the Runs tab on that run, with `Condition`, the expression and `true`. The designer functional class passes 28/28.

### - [x] 10.5 Retry policies (item 2)

Decisions:

| # | Decision | Why |
|---|---|---|
| R1 | A **task** has a retry policy among its settings (`ActivityRetryPolicy`, kept in its properties like its title): how many times it's retried when it faults (0 to 10, none by default), the delay before the first retry, and whether the delay is the same each time or doubles (capped at a day). Events aren't retried: they wait for something rather than doing it. | Transient failures (a service that's down for a minute, a lock) shouldn't need someone to press Retry. |
| R2 | A retry with no delay runs at once, in the same run. With a delay, the instance is **faulted with a retry due**: `Workflow.PendingRetry` (the activity, the failed attempts, when the next one is due), indexed by its due time; a background task, every minute, runs the due retries as the Retry button does, from that activity, on the instance's version, under its lock. | Waiting as a stored instance survives a restart and holds nothing in memory. A faulted instance is what Retry already runs again, so the rest of the engine, the lists and the counts don't change. |
| R3 | The fault handlers (the Workflow Fault event) run only once the attempts are spent. Retrying the instance by hand starts the attempts again. The run that completes the activity clears the pending retry. | The fault event is for failures someone should look at, not for each attempt. |
| R4 | Once the attempts are spent, the policy chooses what the failure does: **fault the instance** (as today, the default) or **follow a Failed outcome**, which the task then shows on the canvas and which the run takes with the error in the journal. | Some failures have a fallback path (notify someone, try another service) that the workflow should take rather than stop. A built-in outcome works for every task, without each task declaring one. |
| R5 | The journal records each failed attempt that will be retried with the status **Retrying** and its error; the viewer shows the instance's next attempt, and the card shows that the task retries. | Each attempt and why it failed is what someone debugging a flaky step needs. |

Steps:

- `A/`: `ActivityRetryPolicy` (`MaxRetries`, `DelaySeconds`, `Backoff`, `OnFailure`) and its delays; `Workflow.PendingRetry` (`WorkflowPendingRetry`); `WorkflowExecutionRecordStatus.Retrying`; `IWorkflowManager.RunDueRetryAsync`.
- The engine retries a faulted task by its policy (R2–R4); the background task runs the due retries; `WorkflowIndex.RetryDueUtc` and its migration.
- The policy's editor on the task's settings (a display driver); the designer adds the Failed outcome to the node when the policy follows it, and validates its transitions like the others; the card's retry indicator; the journal's and Runs tab's Retrying status; the viewer's next attempt.
- **Tests**: retries with no delay until it succeeds; spent attempts fault, or follow Failed; a delayed retry is pending and runs when due, not before; a manual retry starts the attempts again; the fault event fires only once the attempts are spent; the delays (fixed, doubling, capped); Vitest for the status and the indicator; a functional test of a task that follows Failed.
- **Notes from implementing this step:**
  - **API (`A/`).** `ActivityRetryPolicy` (`MaxRetries` up to `MaxRetriesLimit` 10, `DelaySeconds`, `Backoff` `Fixed`/`Exponential`, `OnFailure` `FaultWorkflow`/`FollowFailedOutcome`, `GetDelay(retry)` capped at `MaxDelay`, a day) and `activity.GetRetryPolicy()`, which returns the policy of a task when it retries or follows Failed, clamped, and nothing for events. `Workflow.PendingRetry` (`WorkflowPendingRetry`: `ActivityId`, `FailedAttempts`, `MaxRetries`, `DueUtc`). `WorkflowExecutionRecordStatus.Retrying` and `Failed`. `IWorkflowManager.RunDueRetryAsync(workflow)` and `IWorkflowStore.ListDueRetriesAsync(utcNow, take)`.
  - **Engine.** In the activity's `catch`: while failed attempts ≤ retries, the record is `Retrying` and the pending retry is set; without a delay the task is pushed back on the stack (executed again, not resumed, as a retry by hand), otherwise the instance is faulted without running the fault handlers. Once spent, the pending retry is cleared, and the task either faults the instance (the handlers run) or produces `Failed`, with the error as the last result and a `Failed` record. A completed or halted run of the task clears its pending retry. `RetryActivityAsync` clears it (the attempts start again); `RunDueRetryAsync` keeps it and runs only when due.
  - **Children.** An Execute Workflow task waits on a child that's faulted with a retry pending, like a halted one, and the child resumes its parent only once it ends (finished, or faulted without a pending retry).
  - **Scheduling.** `WorkflowIndex.RetryDueUtc` (set only for a faulted instance) and migration step 8 with its index. `WorkflowRetryBackgroundTask` ("Workflow Retries", every minute, in the Workflows feature) runs up to 50 due retries per run, earliest first, each on its own, logging a failure.
  - **Designer.** `ActivityRetryPolicyDisplayDriver` adds **When it fails** to the settings of every task (`Content:after`); a policy that doesn't retry nor follow Failed is removed from the properties. `WorkflowDesignerModelBuilder.GetOutcomesAsync` adds the `Failed` outcome when the policy follows it, which the draft manager uses too, so the transitions of `Failed` stay while it's followed and are removed when it's not. The node has `retries`, shown as a badge; the journal's and Runs tab's statuses come from `panel/journalStatus.ts` (Retrying in yellow, Failed in red, neither counted as a script error); the viewer's activity panel shows the next retry of the task (`pendingRetry` of the instance), and `faultedActivityId` is the task of the pending retry.
  - **Docs.** "Retrying Failed Tasks Automatically" in the Execution Journal section, the API for developers, release notes and breaking changes (the interface members and statuses).
  - **Tests.** `WorkflowManagerTests` (7): retries without a delay until success, spent retries fault with the handlers run once, spent retries follow Failed, a delayed retry runs when due and doubles, a retry by hand starts the attempts again, the delays, the policy of tasks and events. `WorkflowStoreRetryTests` lists the due retries on SQLite with the real migration. `WorkflowTypeDraftManagerTests` keeps the `Failed` transition while followed. Vitest (JournalTab, ActivityNode, instance viewer): 299/299. Functional `RetryPolicy_TaskThatKeepsFailing_IsRetriedThenFollowsItsFailedOutcome` with the seeded "Retried call" (the sample `TransientFailureTask` now fails `Failures` times): the badge, the Failed port, the settings, the response from the Failed branch, and the journal; the designer class passes 29/29. Workflows tests: 526/526.

### - [x] 10.6 Run from the designer (item 3)

Decisions:

| # | Decision | Why |
|---|---|---|
| RN1 | **Run…** in the designer's toolbar runs the **published version**, never the draft; when the draft has changes, the dialog says to publish them first. | Instances run on a version so they can wait and resume (Phase 5); a draft isn't one, and a run has real effects (emails, content). |
| RN2 | A workflow that starts with **Started by Workflow** runs with its **input variables**, one field per variable, typed by the variable's type, on the server (`Designer/Run`), which returns the instance, its status, its error and its output variables. | That's what Execute Workflow passes it, so it runs as it would from another workflow. |
| RN3 | A workflow that starts with an **HTTP Request** event is run by sending the request from the browser to its generated URL, with the event's method, a query string and a body, as a client would. The dialog shows the response, and finds the instance the request started (the newest of the workflow, created after the request was sent). | The HTTP event reads the request (its method, query string, body, headers), which only a real request has. |
| RN4 | Running requires **Execute workflows**, like retrying. A workflow that's disabled, or whose published version starts with another event, doesn't offer Run. | Running from the designer is executing the workflow. Other events (content, timers, users) are triggered by what they wait for. |

Steps:

- `Designer/Run` (inputs) and `Designer/LatestInstance`; the definition says how the published version runs (`run`: inputs or HTTP, its start activity, its method, its input variables); the configuration has the URLs when the user can execute workflows.
- `RunDialog.vue`: the inputs or the request, the result (status, error, outputs, or the response), and a link to the instance.
- **Tests**: the endpoint runs with typed inputs and returns the outputs, refuses without the permission, a disabled workflow or another start; Vitest for both modes; a functional test of each mode.
- **Notes from implementing this step:**
  - **Server.** `WorkflowDesignerDefinition.Run` (`WorkflowDesignerRun`: `Mode` `inputs`/`http`/null, `IsEnabled`, `ActivityId`, `HttpMethod`, `Inputs`) is computed from the published workflow type: Started by Workflow first, then HTTP Request. `POST Designer/Run` checks Execute workflows, the mode, that the workflow is enabled, a singleton that's running (409), and that each input coerces to its variable's type (400 naming those that don't), then starts the published version and returns a `WorkflowDesignerRunResult` (instance id and page URL unless it was deleted once finished, status, error, outputs as JSON). `GET Designer/LatestInstance` returns the newest instance of the type (by document id).
  - **Configuration.** With Execute workflows, the designer's configuration has `urls.run`, `urls.latestInstance` and `urls.generateHttpUrl` (the HTTP feature's `GenerateUrl`, null when it's disabled); the toolbar shows **Run…** then.
  - **Dialog.** `draft/RunDialog.vue` loads the definition when it opens, since a publish may have changed how the workflow runs. Inputs: a field per input variable by type (number, checkbox, JSON for object, array and any, text otherwise); an empty field is left out. HTTP: it notes the newest instance, generates a URL valid for a day, sends the request with `fetch` (same-origin credentials), shows the status and the body (cut at 5,000 characters), then takes the newest instance if it's a new one. Notes for a draft with changes, a workflow that can't run here, and a disabled one.
  - **Tests.** `WorkflowDesignerControllerTests`: `Run_WorkflowStartedByWorkflow_RunsThePublishedVersionWithTheInputsAndReturnsTheOutputs` (the definition's run, the outputs, the instance URL, the newest instance, an input of the wrong type), `Run_HttpOrDisabledWorkflow_IsRefused`, the URLs in the configuration, and Run in the endpoints refused without the permission. `RunDialog.spec.ts` (6): typed inputs, invalid JSON, the request and its instance, a request that started none, the notes, the toolbar button. Vitest 305/305. Functional `Run_WorkflowWithInputsOrStartedByARequest_RunsItFromTheDesignerAndOpensTheInstance` (the seeded Doubler with 21 gives 42 and opens its instance; Recorded condition answers "big"); the designer class passes 30/30.
  - **Also.** The instance viewer's endpoint had lost its doc comment to the journal data's; it's back above `Instance`.
  - **From the review.** A query string to type was too technical. The run options now list the query string parameters and form fields the published version reads (`WorkflowRequestNames` finds `queryString('…')`, `requestForm('…')`, `Request.QueryString[…]`/`.…` and `Request.Form[…]`/`.…` in the activities' properties), and the dialog asks for them as name and value rows (`draft/NameValueRows.vue`): the names the workflow reads are fixed and sent only when filled, others can be added and removed. The body of a POST is form fields (the default when the workflow reads some) or JSON or text. Tests: the definition lists the names (JavaScript and Liquid, once each), and `run_ValuesTheWorkflowReads_AreListedAndSentWithTheOnesAdded`; Vitest 308/308.

### - [x] 10.7 One instance per correlation id (item 4)

Decisions:

| # | Decision | Why |
|---|---|---|
| C1 | `WorkflowType.IsSingletonPerCorrelation` sits next to `IsSingleton` rather than replacing it with an enum; the forms show both as one choice, **Instances at a time**: any number, one at a time, one per correlated item. | `IsSingleton` is public API, stored in every workflow type, version, recipe and deployment; a second flag keeps them valid, and the choice keeps the forms simple. |
| C2 | With one per correlated item, an event that starts the workflow with a correlation id doesn't start an instance while an instance of the workflow with that correlation id waits (has blocking activities). Starts without a correlation id aren't limited. The lock taken before starting is per workflow type and correlation id. | The content and user events and signals start with a correlation id: that's "one approval per content item". HTTP requests and timers have none at start, so there's nothing to compare. |

- **Notes from implementing this step:**
  - **API.** `WorkflowType.IsSingletonPerCorrelation` and the same on `WorkflowTypeVersion`, the draft and its settings; it's part of the versions (and their fingerprint), the comparison, the recipe step, the clone and the create page. `IWorkflowStore.HasHaltedInstanceAsync(workflowTypeId, correlationId)` (through `WorkflowBlockingActivitiesIndex`). `TryAcquireWorkflowTypeLockAsync` takes an optional correlation id, for a lock per type and correlation id.
  - **Engine.** `TriggerEventAsync` skips starting a workflow that runs one instance per correlation id when one with the event's correlation id waits; **Restart** does the same with the instance's correlation id.
  - **Forms.** `WorkflowTypePropertiesViewModel.InstanceLimit` (`WorkflowInstanceLimit`: `Any`, `One`, `OnePerCorrelation`) maps the two flags; the designer's Workflow tab, the create and clone pages show it as a select (`data-cy=instance-limit`) instead of the Single instance checkbox.
  - **Docs.** "Instances at a Time" after Correlation, the Workflow tab and versions lists, release notes and the store's breaking change.
  - **Tests.** `TriggerEventAsync_OneInstancePerCorrelationId_StartsNoInstanceWhileOneWithTheSameIdWaits` (the test workflow manager's lock now succeeds); the settings form posts `InstanceLimit` and the draft gets the flags. Functional `InstanceLimit_OnePerCorrelatedItem_IsSavedInTheDraft`; the designer class passes 31/31. Workflows tests: 529/529.

### - [x] 10.8 Instances across workflows (item 5)

Decisions:

| # | Decision | Why |
|---|---|---|
| L1 | One list serves both pages: the instances of a workflow (`Workflows/Types/{id}/Instances`) and of every workflow (`Workflows/Instances`), which adds a workflow filter and the workflow of each instance. The workflows list links to it; the admin menu doesn't change. | Same filters, actions and look in both places; operating instances starts from the workflows. |
| L2 | Filters: the status (all, halted, running, faulted, finished, aborted), when it was created (any time, the last 24 hours, 7 or 30 days), and the order. The counts by status of the filtered instances head the list, each a link to its filter. | Finding the faulted or stuck instances is the first thing someone operating workflows does. |
| L3 | Bulk actions: **Retry** the faulted instances from the activity that faulted (the pending retry's, or the journal's last faulted record; those without one are skipped and counted), **Cancel** the instances that haven't ended (aborted: they stop waiting and their pending retry is dropped), and **Delete**. Retrying requires Execute workflows, like the Retry button. | The actions an operator takes after a fix, an outage or a mistaken run, on many instances at once. |

Steps:

- `WorkflowController.All` and the shared list; the options (`WorkflowTypeId`, `Created`, the new statuses and bulk actions); the counts; Retry and Cancel; the view; a link on the workflows list.
- **Tests**: the list filters by workflow, status and date, and counts; Retry, Cancel and Delete in bulk; a functional test of the page.
- **Notes from implementing this step:**
  - **Controller.** `WorkflowController.All` (`Workflows/Instances`) and `Index` share `BuildListAsync`: a query per status filter (counted for the counts, the selected one paged), the creation date from `IClock`, the workflow from `Options.WorkflowTypeId` on the list of every workflow, and the version numbers of the instances' workflow types. The filter and bulk posts of both lists keep the filters. `ApplyBulkActionAsync` does Retry (Execute workflows; the activity from `PendingRetry` or the journal's last faulted record; `ArgumentException`/`InvalidOperationException` and a lock held count as skipped), Cancel (`Aborted`, no blocking activities, no pending retry; finished and aborted instances are left) and Delete.
  - **Options.** `WorkflowFilter` gets `Halted`, `Running` and `Aborted`; `WorkflowCreatedFilter`; `WorkflowBulkAction` gets `Retry` and `Cancel`; `WorkflowIndexOptions.WorkflowTypeId` and `Created`; `WorkflowIndexViewModel.StatusCounts`; `WorkflowEntry.WorkflowType`.
  - **View.** `Views/Workflow/Index.cshtml` serves both: the counts as links (`data-cy=instance-count-{filter}`), the workflow filter on the list of every workflow, the creation date filter, the workflow of each instance, and a faulted instance's retry due. The workflows list has an **Instances** button.
  - **Fix.** The bulk **Actions** menu of this list and of the workflows list never showed: the column had `d-none`, which the list script's inline `display` can't override. It's hidden with an inline style now.
  - **Tests.** `AllInstances_Filters_ListAndCountTheInstancesOfTheWorkflowStatusAndDate` and `AllInstances_BulkActions_RetryCancelAndDeleteTheCheckedInstances` (the post needs a non-empty `submit.BulkAction`, as a browser sends). Functional `AllInstances_InstancesOfAWorkflow_AreCountedFilteredAndDeletedInBulk`; the designer class passes 32/32.

### - [x] 10.9 Parent and child instances (item 6)

Decisions:

| # | Decision | Why |
|---|---|---|
| P1 | A journal record can say which instance its activity started or got the result of (`WorkflowExecutionRecord.ChildWorkflowId`), which the activity reports with `WorkflowExecutionContext.ReportChildWorkflow`; Execute Workflow reports the child it starts, and the child whose result it resumes with. | The record is what the Runs tab shows for each run, and a custom activity that starts workflows can report its children the same way. |
| P2 | The instance viewer resolves the ids to the instances' pages: a run of Execute Workflow links to its child, and the page of a child links to its parent (`Workflow.ParentWorkflowId`, which exists). A deleted instance is named by its id, without a link. | Following a composed workflow from one instance to the next is what debugging it needs. |

Steps:

- `A/`: `WorkflowExecutionRecord.ChildWorkflowId`, `WorkflowExecutionContext.ReportChildWorkflow`; Execute Workflow reports it.
- The instance endpoint resolves the child and parent instances to their pages; the Runs tab and the toolbar link to them.
- **Tests**: the record of an Execute Workflow run has the child's id; the endpoint links the child and the parent; Vitest for the links; the functional test of composition follows the links.
- **Notes from implementing this step:**
  - **API (`A/`).** `WorkflowExecutionRecord.ChildWorkflowId`; `WorkflowExecutionContext.ReportChildWorkflow(workflowId)` keeps it for the next record, which `RecordExecution` takes. Execute Workflow reports the child it starts (the halted record of a waiting task has it too) and the child whose result it resumes with.
  - **Instance endpoint.** It looks up the child ids of the journal and the parent id in `WorkflowIndex` and gives the pages of those that exist: `WorkflowDesignerJournalRecord.ChildWorkflowId`/`ChildInstanceUrl`, `WorkflowDesignerInstance.ParentWorkflowId`/`ParentInstanceUrl`.
  - **Viewer.** A run with a child shows "Ran the instance" with a link (`data-cy=run-child-link`), or the id of a deleted one; the toolbar of a child shows "Run by the instance" with a link to its parent (`data-cy=instance-parent-link`).
  - **Tests.** `ExecuteWorkflowTaskTests` (now with a journal that keeps the records): the task's record has the child's id, others don't, and a waiting task's halted record has it. `Instance_ParentAndChild_LinkToEachOtherWhenTheyExist` (and a deleted child without a link). Vitest (RunsTab, instance viewer): 307/307. The composition functional test opens the parent's run, follows the link to the child and back; the designer class passes 32/32. Workflows tests: 533/533.

### - [x] 10.10 Definition of done

- **Notes from checking it (2026-10-08):**
  - The CI-flag build (`-c Release -p:TreatWarningsAsErrors=true --warnaserror -p:RunAnalyzers=true`) of `OrchardCore.Tests` is clean.
  - `OrchardCore.Tests`: 3,694 of 3,696 pass, 2 skipped on Windows (Unix permissions); two cases of `ContentQuickNavigationTests` failed once on a temporary file still in use when the test cleaned up, and pass when run again. The Workflows tests: 533/533.
  - Vitest: 307/307. The functional `Cms` tests: 172/172.
  - `yarn lint` (0 errors; the 2 warnings are in `OrchardCore.Cors`), `yarn check` and `yarn build -n workflows-designer` leave a clean `git status`.
  - With the new settings off (no retry policy, any number of instances, Record activity data unchecked), the engine takes the paths it took before; the existing engine, designer and functional tests run unchanged.

## Definition of done (Phase 10)

- [x] Steps 10.1–10.9 are checked.
- [x] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [x] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [x] Workflows run as before when the new settings are off.
- [x] Docs and release notes are updated.

# Phase 7 — Workflows as activities; activity presets

A workflow can run another workflow as one of its activities, passing it inputs and getting its outputs back, and modules can add entries to the activities pane from configuration or data. This turns the sketch in [`later-phases.md`](later-phases.md#phase-7--workflows-as-activities-dynamic-activity-providers) into steps.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
Activities pane                         Execute Workflow task (Approval)
  Workflows                               Workflow   [ Approval ▾ ]
    ▸ Approval            ← preset        Inputs     amount  [ JavaScript ▾ ] input("Amount")
    ▸ Send invoice                                   reason  [ Literal ▾ ]    Rush order
  Primitives                              Wait for it to finish  [x]
    ▸ Execute Workflow                    Outputs    approved (Yes or no) → [ approved ▾ ]
```

## Decisions

| # | Decision | Why |
|---|---|---|
| C1 | A workflow's **inputs and outputs** are its variables marked `IsInput` and `IsOutput` (Phase 3 declarations). A workflow can be run as an activity when its **Usable as an activity** setting is on (part of its versions). | Reuses the typed declarations; the setting keeps the list of runnable workflows short. |
| C2 | A generic **Execute Workflow** task (`ExecuteWorkflowTask`) has the workflow to run, an expression per input (with its syntax, Phase 4), and whether to **wait** for it. It starts a child instance on the child's published version, on its **Started By Workflow** event (else its first start activity), with the inputs as its input values, and records `ParentWorkflowId`/`ParentActivityId` on the child. | One activity type covers every workflow, and works with the existing editors, storage and versions. The event marks how the workflow starts, as HTTP events do. |
| C3 | **Waiting**: when the child finishes in the same run, the task takes its `Done` outcome with the child's outputs. When the child halts, the parent halts on the task; when the child finishes later, the engine resumes the parent's task with the outputs. A faulted child gives the `Failed` outcome. Without waiting, the task takes `Done` right away. | The flat engine supports it with the existing blocking and resume mechanism. |
| C4 | The task's **outputs** are the child's output variables, which the driver stores on the task when it's saved, so `IActivityOutputs` doesn't load the child. | Output bindings (Phase 3) work as for any activity. |
| C5 | **Recursion**: workflows can run each other 16 levels deep in a run (`WorkflowManager.MaxChildWorkflowDepth`); a task that runs its own workflow is a design warning. | Prevents runaway nesting, well before the engine's recursion limit of 100. |
| C6 | **Activity presets** instead of runtime-generated activity types: an `IActivityPresetProvider` lists entries for the activities pane (name, category, icon, description) that add an existing activity with preset properties. The workflows usable as activities are presets of Execute Workflow. Presets can also come from configuration or a database, like a catalog of HTTP calls. | Presets need no new activity type, editor or view, and stored workflows keep referring to registered activities. The sketch's generic descriptor-driven activity type would need synchronous access to dynamic data from `IActivityLibrary`, which loads activities synchronously. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [x] 7.1 Workflow inputs and outputs

- `WorkflowVariableDefinition.IsInput` and `IsOutput`; `WorkflowType.IsActivity` ("Usable as an activity"), in the draft settings, the settings form, versions and the diff; `Workflow.ParentWorkflowId` and `ParentActivityId`, on the document (the parent is found by its id; listing the children of an instance is left for later).
- Starting an instance with inputs sets its input variables; the Variables tab marks inputs and outputs.
- **Tests**: copies (draft, version, diff); input values set the variables; the settings form.
- **Notes from implementing this step:**
  - **Variables.**
    - `WorkflowVariableDefinition.IsInput` and `IsOutput`, copied by `Clone`.
    - `WorkflowVariables.ApplyInputs` sets the input variables from the values of the same name (ignoring case), converted to their types, and returns the names of those that don't convert.
    - `GetOutputs` returns the output variables that have a value.
  - **Engine.**
    - `StartWorkflowAsync` and `RestartWorkflowAsync` apply the input values to the input variables after their defaults.
    - A value that doesn't convert is logged as a warning, and the variable keeps its default value.
  - **Setting.** `IsActivity` ("Usable as an activity") follows `DeleteFinishedWorkflows` through every copy:
    - the draft and its settings;
    - the designer's settings form, the properties and duplicate pages (a copy keeps the setting);
    - versions, their fingerprint (so changing it makes a new version), the diff, and the recipe step.
  - **Instances.** `Workflow.ParentWorkflowId` and `ParentActivityId` are stored on the document, without index columns: the parent is found by its id.
  - **Variables tab.**
    - Each variable has **Input** and **Output** checkboxes; they're only sent when checked.
    - The read-only table marks inputs and outputs with badges.
  - **Tests.**
    - `ApplyInputs` and `GetOutputs`, `Clone`, and starting an instance with inputs.
    - The diff, a new version when the setting changes, publishing the setting from the draft, and the designer's settings form.
    - Vitest: checking **Input** saves it, and the read-only badges.
    - Workflows tests: 252/252. Vitest: 232/232.

### - [x] 7.2 Execute Workflow task

- `ExecuteWorkflowTask` (C2–C5), the **Started By Workflow** event, `IWorkflowManager.StartChildWorkflowAsync`, the resume of a waiting parent when its child finishes, and the validation warnings (no workflow selected, a workflow that runs itself).
- **Tests**: a child that finishes returns its outputs; a child that halts makes the parent wait, and its end resumes the parent; a faulted child gives `Failed`; fire and forget; the recursion guard.
- **Notes from implementing this step:**
  - **Starting a child.** `IWorkflowManager.StartChildWorkflowAsync(workflowType, parentContext, parentActivityId, input)`:
    - starts the child on its **Started By Workflow** event (`StartedByWorkflowEvent`, a new event without an editor, in Primitives), else on its first start activity;
    - records the parent on the child, and sets the child's input variables from the input values (7.1);
    - fails with an exception once workflows run each other `WorkflowManager.MaxChildWorkflowDepth` (16) levels deep in a run.
    - `StartWorkflowAsync` and the child start share `StartWorkflowCoreAsync`.
  - **Resuming the parent.**
    - When a child finishes or faults in a later run (a resume or a retry), the engine resumes its parent's activity, if the parent waits on it, with a `ChildWorkflowResult` input (status, outputs, fault message).
    - A parent that runs in the same scope reads the result itself, as the engine's recursion counters show.
    - The parent's lock is taken when it's atomic.
  - **Execute Workflow task** (`ExecuteWorkflowTask`):
    - Properties: `WorkflowTypeId`, `Inputs` (an expression per input variable, JavaScript when no syntax is set), `WaitForCompletion` (default on), `Outputs` (stored output descriptors, its `IActivityOutputs`), and `ChildWorkflowId` while it waits.
    - It faults when the workflow doesn't exist, isn't usable as an activity, or is disabled.
    - A child that finishes gives `Done`, with the child's output variables as the task's outputs and as `LastResult`. A faulted child gives `Failed`, with its fault message as `LastResult`. A child that halts makes the task halt until the child ends. Without waiting, the task takes `Done` at once.
    - The task resolves `IWorkflowManager` when it runs, since the manager creates the activities.
    - The result input is removed once read, so it isn't kept in the instance.
    - It's registered with its editor in 7.3.
  - **Validation.** Two design warnings: `MissingWorkflowToExecute` (no workflow selected) and `RecursiveWorkflowExecution` (the task runs the workflow it belongs to).
  - **Tests** (`Composition/ExecuteWorkflowTaskTests`, over in-memory stores with the site's JSON options):
    - a child that finishes returns its outputs to a bound variable, and records its parent;
    - a child that waits makes the parent wait, and its end resumes the parent;
    - a faulted child, now or later, gives `Failed`;
    - without waiting, the parent goes on and the child's end doesn't resume it;
    - a workflow that runs itself stops at the depth limit, and one not usable as an activity faults the parent;
    - the stored outputs are the task's outputs, and the draft manager reports the two warnings.
    - Workflows tests: 261/261.

### - [ ] 7.3 Editor

- The task's driver, view model and views: the workflow select (workflows usable as activities), an expression editor per input, the wait checkbox, and the stored output descriptors.
- The designer loads an editor form again once a change of a field marked `data-wfd-reload` is applied, so the inputs follow the selected workflow.
- **Tests**: controller (the inputs follow the selected workflow; saving stores the outputs).

### - [ ] 7.4 Activity presets

- `IActivityPresetProvider` and `ActivityPreset`; the activities pane lists presets; `AddActivity` takes a preset; the workflows usable as activities are presets.
- **Tests**: the library lists presets; adding one creates the activity with its properties; Vitest for the pane.

### - [ ] 7.5 Docs and release notes

### - [ ] 7.6 End-to-end test

- A seeded parent workflow runs a seeded child with an input, and responds with the child's output; the child appears as a preset in the activities pane.

## Definition of done (Phase 7)

- [ ] Steps 7.1–7.6 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Docs and release notes are updated.

## Open questions

- C6: should presets also be able to hide the underlying activity from the pane?
- Should a parent instance's page link to its children?

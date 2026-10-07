# Phase 9 — Available data, and script errors

Make it visible what an activity can use, and where a run went wrong when a script failed. This phase comes from the review of phases 1–8: authors had to look up in the docs or the code which values the activities put in the workflow's Input, Output and Properties, and a script error only went to the server log, so a run that stopped because of one looked like it finished normally.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
Activity tab of "Notify Owner"                       Instance page
  [editor of the activity]                             (Content Created) ─▶ (Read the query) ─▶ (Notify) ...
  Outputs ...                                                                ⚠ script error
  Available data                                        Journal: 2 Read the query  Completed ⚠ "The value 'lots'
    Variables                                                    isn't a valid number for the variable 'quantity'."
      total (number)        variable('total')   {{ Workflow.Variables.total }}
    From "Content Created"
      ContentItem (content item, input)   input('ContentItem')   {{ Workflow.Input.ContentItem }}
    From "Retrieve Owner"
      Owner (user, property)   property('Owner')   {{ Workflow.Properties.Owner }}
    Workflow
      Last result   lastResult()   {{ Workflow.LastResult }}
  Clicking a snippet inserts it where the cursor was in the editor above, or copies it.
```

## Decisions

| # | Decision | Why |
|---|---|---|
| A1 | Activities **declare the values they provide**: what an event starts or resumes the workflow with (Input), and what a task writes to Input, Output or Properties. An activity implements `IActivityProvidedValues`, or a module declares the values when it registers the activity (`ActivityRegistration.Provides(...)`), without changing its code. A value has a name, a source (Input, Output, Properties), a variable type name and a description. | The values become discoverable without changing how activities pass them, so every existing workflow and script keeps working. Declaring at registration covers third-party activities. |
| A2 | The built-in events and tasks declare their values. | The list is useful from the start. |
| A3 | The **Activity tab** lists the **available data** of the edited activity: the workflow's variables, the values provided by the activities on a path to it (nearest first), and the workflow's last result and correlation id. Each value shows its JavaScript and Liquid expressions. | It answers "what can I use here?" where the expression is written. Values from activities that can't run before this one aren't listed, so the list isn't misleading. |
| A4 | Clicking an expression **inserts** it where the cursor was in the activity's editor: a Monaco or CodeMirror editor, an input or a text area. Without one, it's copied. The script and Liquid editors also **suggest** the available values. | No typing from memory. Monaco exposes its editors, so no editor script has to change. |
| A5 | **Script errors** are reported by the JavaScript evaluator to the execution context, and recorded in the journal with the activity that ran the script. The instance page marks that activity with a warning, and its journal record shows the error. | The evaluator falls back to a default value and the run goes on (or ends when a script didn't choose an outcome), which is how existing workflows behave, but it now shows. |
| A6 | A workflow setting, **Fault the workflow on script errors** (off by default, part of the versions), faults the instance at that activity instead, so it's shown in red and can be retried. | Opt-in, so existing workflows don't change. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [ ] 9.1 Provided values

- `A/`: `IActivityProvidedValues`, `ActivityProvidedValue` and `WorkflowValueSource`; `ActivityRegistration.ProvidedValues` and `Provides(...)`.
- The designer's nodes carry the values their activity provides (its own declaration and its registration's).
- **Tests**: the model builder merges both; the definition endpoint returns them.

### - [ ] 9.2 Built-in declarations

- The events and tasks of the built-in modules declare their values (see the inventory in the notes).
- **Tests**: the declarations of a few activities, including one whose name comes from a setting.

### - [ ] 9.3 Available data in the designer

- `AvailableData.vue` in the Activity tab: variables, values of the activities on a path to the edited one, and the workflow's last result and correlation id, each with its JavaScript and Liquid expressions.
- Insert at the cursor of the last focused field of the activity's editor, or copy.
- The Monaco completions suggest the available values (`input('…')`, `property('…')`, `Workflow.Input.…`, `Workflow.Properties.…`, `Workflow.Output.…`).
- **Tests** (Vitest): the upstream values, insertion in an input, a text area and Monaco, copying, and the completions.

### - [ ] 9.4 Script errors in the engine

- `WorkflowExecutionContext.ReportScriptError`; the JavaScript evaluator reports the errors it falls back from; the engine records them on the activity's journal record, and faults the instance when the workflow faults on script errors.
- `WorkflowType.FaultOnScriptErrors`, in the draft settings, the settings forms, versions, the diff and recipes.
- **Tests**: an error is recorded on the activity that ran the script; with the setting, the instance faults there; the copies of the setting.

### - [ ] 9.5 Script errors in the designer

- The instance page marks the activities whose journal records have errors with a warning badge and the message; the legend explains it.
- **Tests** (Vitest): the warning badge.

### - [ ] 9.6 Docs and release notes

### - [ ] 9.7 End-to-end test

- The available data of an activity after a content event lists its content item, and clicking an expression inserts it into the editor.
- A script error shows a warning on the instance page; with the setting, the instance is faulted at that activity.

## Definition of done (Phase 9)

- [ ] Steps 9.1–9.7 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Workflows run as before when the new setting is off.
- [ ] Docs and release notes are updated.

## Open questions

- Should the Issues tab warn about a script that reads a value no activity on a path provides?
- Should Liquid errors be recorded too? Liquid renders what it can, so it rarely fails at run time.

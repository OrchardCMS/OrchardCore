# Phase 3 — Typed variables and data binding

Declared, typed variables at workflow scope; activities with typed outputs that can be bound to variables; expressions that read the variables. This turns the sketch in [`later-phases.md`](later-phases.md#phase-3--typed-variables-and-data-binding) into steps. The flat execution model doesn't change.

Paths: `M/` = `src/OrchardCore.Modules/OrchardCore.Workflows/`, `A/` = `src/OrchardCore/OrchardCore.Workflows.Abstractions/`.

## Target experience

```
┌ Properties ──────────────────────────────────┐
│ Activity | Variables | Workflow | Issues      │
│                                               │
│ Variables of this workflow, read with         │
│ {{ Workflow.Variables.name }} or variable().  │
│ ┌──────────┬────────────┬──────────────┬───┐  │
│ │ Name     │ Type       │ Default      │   │  │
│ │ greeting │ Text     ▾ │ Hello        │ 🗑 │  │
│ │ attempts │ Number   ▾ │ 0            │ 🗑 │  │
│ └──────────┴────────────┴──────────────┴───┘  │
│ [+ Add a variable]                            │
└───────────────────────────────────────────────┘

Activity tab of a Script task, below its editor:
  Outputs
  Result (any)      → [ greeting ▾ ]
```

## Decisions

| # | Decision | Why |
|---|---|---|
| T1 | A **variable is a workflow property**: declared variable `x` is `Properties["x"]`. Reading it through `Variables` returns it coerced to its type; writing it through `Variables` (Set Variable, output bindings, `setVariable()`) coerces it, and a value that can't be coerced faults the workflow with a message naming the variable. Writing `Properties` directly still works and isn't checked. | Existing workflows and activities that use `Properties` keep working, and a variable is visible to them. |
| T2 | **Declarations are versioned**: `IList<WorkflowVariableDefinition> Variables` on `WorkflowType`, `WorkflowTypeVersion` and the draft; part of the version fingerprint and of the compare changes. Defaults are applied when an instance's context is created and the variable has no value yet. | A variable's type and default affect how an instance runs. |
| T3 | **Types are services** (`IWorkflowVariableType`: name, display name, editor kind, coercion). Built in: `string`, `number` (`double`), `boolean`, `datetime` (UTC `DateTime`), `object` and `array` (JSON), and `any` (no coercion). `contentItem` is registered by OrchardCore.Contents, whose `ContentItemSerializer` persists it. | Modules can add types; persistence keeps using `IWorkflowValueSerializer`. |
| T4 | **No `user` type in this phase**: there is no workflow value serializer for users. | It needs a serializer first; recorded in the backlog. |
| T5 | **JavaScript** gets `variable(name)` and `setVariable(name, value)` next to `property()`/`setProperty()`, not a `variables.x` object: `GlobalMethod` only exposes functions, and changing OrchardCore.Scripting is out of scope. **Liquid** gets `{{ Workflow.Variables.x }}`. | Same shape as the existing workflow methods. |
| T6 | **Outputs** are declared by activities implementing `IActivityOutputs`; an activity sets an output with `workflowContext.SetActivityOutput(activityContext, name, value)`. Bindings live in the activity's properties (`Properties["OutputBindings"] = { "Output": "variable" }`), and the engine applies them after the activity executes (not when it halts or faults). | Bindings are versioned with the activity, survive editor updates (drivers only set their own keys), and need no new storage. |
| T7 | The **designer edits variables and bindings client-side** (a Variables tab, an Outputs section under the activity editor), saved through new draft endpoints with the usual revision checks. | Variables and bindings aren't part of any activity's server-rendered editor. |

## Steps

Do the steps in order. Each step is one commit; tick its box in that commit. Every step needs a green CI-flag build, passing tests for what it touches, and committed `yarn build` output if assets changed.

---

### - [ ] 3.1 Declarations and types

- `A/Models/WorkflowVariableDefinition.cs`: `Name`, `TypeName`, `JsonNode DefaultValue`, `Description`.
- `A/Services/IWorkflowVariableType.cs` (`Name`, `DisplayName`, `Editor` = `text`/`number`/`boolean`/`datetime`/`json`, `TryCoerce(object value, out object result)`); built-in types in `M/Variables/`, registered in `M/Startup.cs`; a lookup service `IWorkflowVariableTypeProvider`.
- `Variables` on `WorkflowType`, `WorkflowTypeVersion`, `WorkflowTypeDraft`, copied everywhere the definition is copied: `CreateDraft`/`ApplyTo`, `ToWorkflowType`, version creation and fingerprint, `RestoreAsync`, `WorkflowTypeDiff` ("Variables" in the changed settings), the recipe update, `Duplicate`.
- Validation of declarations (`WorkflowVariableValidator`): names are unique (case-insensitive) identifiers, types exist, defaults coerce. Used by the draft manager.
- **Tests**: each built-in type's coercions; validation rules; a variable change creates a version and shows in the diff; drafts, versions and restores carry the variables.

### - [ ] 3.2 Runtime and expressions

- `WorkflowVariables` (`A/Models/`), `WorkflowExecutionContext.Variables`: typed get/set over `Properties` (T1), `WorkflowVariableException` on a failed coercion.
- `WorkflowManager`: builds `Variables` from the definition it runs and the registered types, and applies missing defaults when it creates an execution context.
- JavaScript `variable()`/`setVariable()` in `WorkflowMethodsProvider`; Liquid `Workflow.Variables` in `M/Startup.cs`.
- `contentItem` type in OrchardCore.Contents.
- **Tests**: defaults on start; coercion on write and read (numbers that come back as `int` after a save); a failed coercion; JS and Liquid access; a workflow that only uses `Properties` is unchanged; a content item survives a save and reload.

### - [ ] 3.3 Activity outputs and bindings

- `IActivityOutputs`, `ActivityOutputDescriptor`, `WorkflowExecutionContext.SetActivityOutput`; bindings applied in `WorkflowManager.ExecuteWorkflowAsync` after `OnActivityExecutedAsync`, before outcomes are scheduled; a failed coercion faults the workflow.
- Outputs: `ScriptTask` (Result, any), `LiquidTask` (Result, string), `SetPropertyTask` (Value, any), `HttpRequestTask` (Body, string; StatusCode, number; Response, object), `CreateContentTask`, `RetrieveContentTask` and `UpdateContentTask` (ContentItem, contentItem).
- **Tests**: a binding writes the variable after execution; no binding changes nothing; a halted activity applies nothing; a failed coercion faults; each activity's outputs.

### - [ ] 3.4 Set Variable activity

- `SetVariableTask` (Primitives): `VariableName`, a JavaScript or Liquid value (same syntax toggle as Set Property), outcome Done; writes through `Variables` (T1). Driver, view model, views, icon.
- **Tests**: sets a declared variable with coercion; an undeclared name stores the raw value; Liquid and JavaScript values; the editor validates the Liquid.

### - [ ] 3.5 Designer API

- The definition carries `variables`, the available `variableTypes`, and for each node its `outputs` and `outputBindings`.
- `POST Variables` (`revision`, `variables`) and `POST OutputBindings` (`activityId`, `revision`, `bindings`), through new draft manager methods with the usual conflict handling; invalid declarations return 400 with a message per variable.
- Validation issues: a Set Variable or binding naming an undeclared variable (warning); a binding whose output type can't be assigned to the variable's type (warning).
- **Tests**: endpoints, conflicts, validation, issues, 403.

### - [ ] 3.6 Designer UI

- **Variables tab** (between Activity and Workflow): a table of name, type, default (type-aware input) and description; add, edit, delete; saved on blur or change through the revision queue; errors shown per row. Read-only in the viewer, which shows the instance's values too.
- **Outputs section** under the activity editor: one row per output with a variable picker (a type mismatch is flagged).
- A `<datalist>` of the variable names for the Set Variable editor's name field.
- **Monaco**: completions for `variable("…")`/`setVariable("…", )` in the JavaScript editors and `Workflow.Variables.…` in Liquid, registered while the designer is open.
- **Tests** (Vitest): the tab's add/edit/delete and errors, bindings, the datalist, the completion items.

### - [ ] 3.7 Docs and release notes

- A **Variables** section in `src/docs/reference/modules/Workflows/README.md` (declaring, types, defaults, reading and writing from JavaScript and Liquid, Set Variable, outputs and bindings, the relationship with `Properties`), and the Set Variable activity in the activities list.
- Release notes: the new feature and the API additions.

### - [ ] 3.8 End-to-end test

- Seeded workflow: HTTP request → Script (`'hello ' + 21 * 2`) → HTTP response (`{{ Workflow.Variables.greeting }}`). The test declares `greeting` in the Variables tab, binds the script's Result to it, publishes, and calls the request URL: the response is `hello 42`.

## Definition of done (Phase 3)

- [ ] Steps 3.1–3.8 are checked.
- [ ] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [ ] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [ ] Workflows without variables behave as before.
- [ ] Docs and release notes are updated.

## Open questions

Decided with the defaults above unless the maintainer says otherwise:

- T1: should writing a declared variable through `Properties` (Set Property, `setProperty()`) be coerced too? It isn't, for compatibility.
- T5: is `variable()`/`setVariable()` enough, or should `OrchardCore.Scripting` support object globals for `variables.x`?
- T6: a failed coercion faults the workflow. Would a "Failed" outcome on Set Variable be better?

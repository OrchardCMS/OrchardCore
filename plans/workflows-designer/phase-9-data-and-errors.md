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

### - [x] 9.1 Provided values

- `A/`: `IActivityProvidedValues`, `ActivityProvidedValue` and `WorkflowValueSource`; `ActivityRegistration.ProvidedValues` and `Provides(...)`.
- The designer's nodes carry the values their activity provides (its own declaration and its registration's).
- **Tests**: the model builder merges both; the definition endpoint returns them.
- **Notes from implementing this step:**
  - **API (`A/`).**
    - `IActivityProvidedValues.GetProvidedValues()` returns `ActivityProvidedValue`s: a source (`WorkflowValueSource.Input`, `Output` or `Properties`), a name, a variable type name (`any` by default) and a description.
    - `ActivityRegistration.ProvidedValues` and the fluent `Provides(source, name, typeName, description)` declare values when a module registers an activity, without changing its code.
  - **Merging.** `IActivity.GetProvidedValues(WorkflowOptions)` merges both. The registration's come first, and the activity's own replace those of the same source and name.
  - **Designer.** Each node of the definition carries `providedValues` (`WorkflowDesignerProvidedValue`: source, name, type name, description). The client type is `ProvidedValue`.
  - **Tests.** `ActivityProvidedValuesTests` (2): the merge and its order, and an activity without declarations. Workflows tests: 283/283.

### - [x] 9.2 Built-in declarations

- The events and tasks of the built-in modules declare their values (see the inventory in the notes).
- **Tests**: the declarations of a few activities, including one whose name comes from a setting.
- **Notes from implementing this step:**
  - **Inventory.** An inventory of the built-in activities found which values the events start or resume a workflow with, and which values the tasks write. The declarations follow it.
  - **Events (Input).**
    - Content events: `ContentItem` (content item) and `ContentEvent` (object).
    - User events: `User`. The login event declares `UserName`, `Roles`, `Provider` and `ExternalClaims` instead; the logout event `UserName` and `Roles`.
    - User Task: `UserAction`, `ContentItem` and `ContentEvent`.
    - Signal: `Signal`. Catch Workflow Fault: `ErrorInfo`.
  - **Tasks (Properties).**
    - Create, Update and Retrieve Content: `ContentItem`.
    - Register User: `EmailConfirmationUrl`. Validate User: `UserName`, when it sets the user name.
    - For Each and For Loop: their loop variable. Set Property: its property.
  - **Tasks (Output and Input).**
    - Set Output: its output. HTTP Request event: `FormLocation`, when it has a form location key.
    - Get Users By Role: its output, when the name isn't computed by Liquid.
    - Notify Content Owner: `Input["Owner"]`.
  - **Not declared.**
    - Set Variable, since variables are listed anyway.
    - Started By Workflow, whose inputs are input variables.
    - Twitter's `TwitterResponse`; its module can declare it.
  - **Base classes.** The base content and user events declare their values with a virtual `GetProvidedValues`, which the login and logout events override.
  - **Tests.** The definition endpoint lists the values of a content event, a signal, a loop and a Set Property activity (whose names come from their settings), and none for Notify. Workflows tests: 284/284.

### - [x] 9.3 Available data in the designer

- `AvailableData.vue` in the Activity tab: variables, values of the activities on a path to the edited one, and the workflow's last result and correlation id, each with its JavaScript and Liquid expressions.
- Insert at the cursor of the last focused field of the activity's editor, or copy.
- The Monaco completions suggest the available values (`input('…')`, `property('…')`, `Workflow.Input.…`, `Workflow.Properties.…`, `Workflow.Output.…`).
- **Tests** (Vitest): the upstream values, insertion in an input, a text area and Monaco, copying, and the completions.
- **Notes from implementing this step:**
  - **Logic** (`available/availableData.ts`).
    - `upstreamNodeIds` walks the transitions back from the activity: the activities on a path to it, nearest first.
    - `availableData` lists the variables, the values of those activities, and the workflow's last result and correlation id. A Properties value named like a declared variable is only listed as the variable.
  - **Expressions.**
    - Variables: `variable("x")`. Input: `input("x")`. Properties: `property("x")`.
    - Output: `workflow().Output["x"]`, since the script function `output(name, value)` only writes.
    - Last result: `lastResult()`. Correlation id: `correlationId()`.
    - In Liquid, `{{ Workflow.Input.x }}`, with brackets for a name that isn't an identifier.
  - **Panel** (`available/AvailableData.vue`). It sits under the Outputs section of the Activity tab, and can be collapsed (remembered).
    - Each value shows its type, its source, its description, and a JavaScript and a Liquid expression.
    - The buttons don't take the focus, so the editor keeps its cursor.
  - **Insertion** (`available/insertion.ts`). The panel remembers the editor field that had the focus last, and inserts at its cursor:
    - Monaco: the editor whose container has the field, from `monaco.editor.getEditors()`;
    - CodeMirror: `replaceSelection`;
    - inputs and text areas: `setRangeText`.
    - It then dispatches `change`, so the form applies. Without a field, the expression is copied, with a toast.
  - **Completions.** `CompletionSource.values` adds the values of the activities on a path to the selected activity to the JavaScript and Liquid completions, with the activity's name in the detail.
  - **Checked in the app.** The list rendered on User Registration's Script task. Clicking `correlationId()` inserted it at the Monaco cursor, and I undid it.
  - **Tests** (Vitest):
    - `availableData.spec.ts` (4): the path order, the groups, the expressions and the completion values.
    - `insertion.spec.ts` (5): input, text area, CodeMirror, Monaco, and fields that don't take text.
    - `AvailableDataPanel.spec.ts` (3): the list, the insert event, and collapsing.
    - A completions test.
    - Vitest: 249/249.

### - [x] 9.4 Script errors in the engine

- `WorkflowExecutionContext.ReportScriptError`; the JavaScript evaluator reports the errors it falls back from; the engine records them on the activity's journal record, and faults the instance when the workflow faults on script errors.
- `WorkflowType.FaultOnScriptErrors`, in the draft settings, the settings forms, versions, the diff and recipes.
- **Tests**: an error is recorded on the activity that ran the script; with the setting, the instance faults there; the copies of the setting.
- **Notes from implementing this step:**
  - **Reporting.**
    - `WorkflowExecutionContext.ReportScriptError` collects the errors in `ScriptErrors`.
    - `JavaScriptWorkflowScriptEvaluator` reports the errors it falls back from, as before after logging them.
    - The stopped evaluations (execution limits, cancellation) still fault the instance, as `main` made them.
  - **Engine.**
    - `ExecuteWorkflowAsync` notes how many errors there were before each activity runs. The errors that activity reported become its journal record's `Error`, on a `Completed` or `Halted` record.
    - When the workflow type faults on script errors, it throws a `WorkflowScriptException` with them, so the instance faults at that activity with a red node, the fault handler and Retry.
  - **Setting.**
    - `WorkflowType.FaultOnScriptErrors` ("Fault the workflow on script errors", off by default) follows `BranchingMode` through every copy: the draft and its settings, the three settings forms, versions and their fingerprint, the diff and the recipe step.
    - The client's settings type has `faultOnScriptErrors`.
  - **Tests.**
    - A script that sets its outcome and throws: off, the instance finishes and the script's record is `Completed` with the error; on, the instance is faulted and the record is `Faulted`.
    - The setting's diff, new version, publish and settings form.
    - Workflows tests: 287/287.

### - [x] 9.5 Fields of the values, and an easier list (from review)

- A provided value can list its **fields** (`ActivityProvidedValue.Members`): the content item and content event of the content events and tasks, the user of the user events and of Notify Content Owner, and the fault of Catch Workflow Fault. Each field has its own JavaScript and Liquid expressions (`input("ContentEvent").ContentType`, `{{ Workflow.Input.ContentEvent.ContentType }}`).
- `ContentEventContext` and `WorkflowFaultModel` are registered for Liquid, so the expressions of their fields work.
- The list is easier to read: bold value names, bold **JS** and **Liquid** labels, expressions that wrap after a dot instead of being cut, a copy icon, and the fields in a collapsible group under their value, each field in a box of its own.
- **Tests**: the fields in the definition; the Liquid registrations; Vitest for the fields' expressions and the list.
- **Notes from implementing this step:**
  - **Fields.**
    - `ActivityProvidedValue.Members` lists `ActivityProvidedValueMember`s: a name, a type and a description.
    - `WorkflowValueMembers` holds the shared lists:
      - `ContentItem` (11 fields);
      - `ContentEvent` (`ContentEventContext`: Name, ContentType, ContentItemId, ContentItemVersionId, IsStart);
      - `User` (the fields the Users module exposes to Liquid);
      - `WorkflowFault`.
    - The content events and tasks, User Task, the user events, Notify Content Owner and Catch Workflow Fault use them. The designer definition carries them as `members`.
  - **Liquid.** `ContentEventContext` (Contents) and `WorkflowFaultModel` (Workflows) are registered for member access; `{{ Workflow.Input.ContentEvent.ContentType }}` returned nothing before. `User` already had its accessor.
  - **Client.**
    - `AvailableValueItem.vue` renders a value and, recursively, its fields in a `<details>` ("Fields (n)").
    - A field's expressions append `.Name` (or `["name"]`) to its value's: `input("ContentEvent").ContentType`.
    - The completions list the fields too.
  - **From review.**
    - Bold names, and bold **JS**/**Liquid** labels in a three-column snippet row.
    - Code that wraps, with a `<wbr>` after each dot, so lines break between members rather than inside a name.
    - The `fa-copy` icon, and a bordered box for each value and for each field in it.
    - The button title is "Insert or copy …": a click inserts at the cursor, or copies when no field was focused.
  - **Verified in the preview.**
    - Content Created Event → Log Task lists ContentItem (Fields (11)) and ContentEvent (Fields (5)).
    - Typing in the Log text, then clicking the ContentType Liquid field, gives `Created a {{ Workflow.Input.ContentEvent.ContentType }}`.
  - **Tests.**
    - `Definition_ActivitiesThatProvideValues_ListThem` checks the fields and the Liquid accessors.
    - Vitest covers `memberExpressionsOf`, the fields in `availableData` and `upstreamValues`, and the fields list in the panel (252 tests).
    - Workflows tests: 287/287.

### - [x] 9.6 Script errors in the designer

- The instance page marks the activities whose journal records have errors with a warning badge and the message; the legend explains it.
- **Tests** (Vitest): the warning badge.
- **Notes from implementing this step:**
  - **Errors by activity.**
    - `canvas/scriptErrors.ts` (`scriptErrorsByActivity`) reads them from the instance's journal: the records that completed or halted with an error, each message once per activity.
    - A faulted record is the fault, shown as before.
  - **Canvas.**
    - `ActivityNode` takes `scriptErrors`. Unless the activity faulted, it gets an amber outline (`has-script-errors`) and a warning badge with a bug icon.
    - The badge's title is "a script failed and used its fallback value" followed by the messages; the node's accessible name says it too.
  - **Journal and legend.**
    - A record with a script error shows a "Script error" badge next to its status, and its message in the warning color.
    - The viewer's legend explains the badge when the instance has script errors.
    - The legend now wraps on a narrow toolbar, and "Runs on version n" stays on one line.
  - **Verified in the preview.**
    - Content Created → Log → Script Task (`var total = input('Order').Total;`), then a new Coming Soon item.
    - The instance finished; the Script Task is amber with "Cannot read properties of null (reading 'Total')", and so is its journal record.
    - The Log task wrote "Created a ComingSoon", which confirms the Liquid fields of 9.5 at run time.
  - **Tests.**
    - Vitest (256): `scriptErrorsByActivity`; the node badge, which a fault hides; the journal's script error badge.

### - [x] 9.7 Docs and release notes
- **Notes from implementing this step:**
  - **Workflows docs.**
    - **Available Data** (under Scripts and Expressions) describes the section, inserting or copying an expression, the fields, and a table of what the built-in activities provide. Outputs are listed with the variables they're stored in.
    - **Available Data for Developers** covers `IActivityProvidedValues` with a sample, the sources and their expressions, `Members` and `WorkflowValueMembers`, the Liquid member access the fields need, and `ActivityRegistration.Provides`.
    - **Script Errors** (under Execution Journal) explains the default value, the amber node and the journal badge, the **Fault the workflow on script errors** setting, and that stopped expressions always fault. The journal's developer list gains `ReportScriptError`.
    - **Liquid Expressions and ContentItem Events** shows `{{ Workflow.Input.ContentEvent.ContentType }}`.
    - **Versions** now lists everything a version holds, including variables, branching and the new setting.
  - **Release notes.** `4.0.0.md` has **Workflow Available Data** and **Workflow Script Errors** sections.
  - **API.** `ActivityRegistration.Provides` takes an optional `members` argument, so a registration can declare fields too; tested in `ActivityProvidedValuesTests`.
  - **Tests.** CI-flag build clean; workflow tests 288/288.

### - [x] 9.8 Last result types, a content type picker, and settings and data views (from review)

- **Last result.** Activities declare what they set as the last result (`WorkflowValueSource.LastResult`, `ActivityProvidedValue.LastResult`), so the Available data view shows its type, what it is and its fields, from the activities connected to the selected one.
- **Content types.** The content events pick their content types in the `bootstrap-select` picker (`@crestapps/bootstrap-select`), searchable, with the selected types as tags, instead of checkboxes.
- **Views.** The Activity tab shows the activity's **Settings** or its **Available data**, one at a time, so the panel is less busy.
- **Hints.** The hints of checkboxes are `hint dashed`, inside the `form-check` after the label.
- **Tests**: the last result in the definition and in `availableData`; the views of the panel; Vitest and xUnit.
- **Notes from implementing this step:**
  - **Last result.**
    - `WorkflowValueSource.LastResult` and `ActivityProvidedValue.LastResult(typeName, description, members)` declare it. `WorkflowValueMembers` gains `Result` (`Succeeded`, `Errors`) and `HttpResponse`.
    - Every activity that sets it declares it: Create, Update and Retrieve Content (content item, with its fields), Script, Liquid, HTTP Request (the response, with its fields), Email, SMS and Meta Conversions (`Result`), the notification tasks (a count), For Each and For Loop, Execute Workflow, Validate User, Create Tenant and the Timer event. `NotifyUserTaskActivity` declares it for every notification task, and Notify Content Owner adds its owner.
    - The designer takes it from the activities with a transition to the selected one: their type when they agree, their fields when they have the same, and "Retrieve Owner: The content item the activity retrieved." Otherwise it's **Any**, "Its type depends on that activity". The last result isn't listed with the activity's own values.
  - **Content type picker.**
    - The `SelectContentTypes` view component has a `displayMode` parameter; `Picker` renders `Picker.cshtml`, a multi-select with `data-show-selected-tags`, `data-live-search`, `data-actions-box` and checkbox indicators, "Any content type" when none is selected.
    - `content-type-picker.ts` (Contents) initializes the pickers with `observeAndInit`, including those the designer injects, and destroys them on `oc:editor-unmounting`. The plugin fires `change` itself, so the designer applies the selection.
    - The seven content event editors use it; the other pages keep their checkboxes.
    - `@T["No content type matches {0}", "{0}"]`: without the argument, the localizer's formatting threw and the editor couldn't load. The select has no `form-select` class, which the plugin copies to its wrapper (a second border and caret).
  - **Settings and data views.**
    - A segmented tab list (`panel-view-settings`, `panel-view-data`) at the top of the Activity tab. The settings are hidden rather than removed, so the editor keeps its changes and cursor.
    - Clicking an expression inserts it where the cursor was in the settings and shows them again (`canInsertAt`), or copies it. Another activity opens on its settings. The Available data section lost its collapse toggle.
  - **Hints.** 21 checkbox hints of the Workflows views and the activity editors moved inside their `form-check` as `hint dashed`, and the Update Content task's became dashed.
  - **Docs.** Available Data (the views), a Last Result section with a table, the developer API, the `displayMode` of `SelectContentTypes`, and the release notes.
  - **Verified in the preview.** The picker on Content Created (search, select all, Button and Form as tags, saved); the views; after a Script task, the last result reads "Script Task: The value the script returns."; the dashed hints in the Workflow tab.
  - **Tests.** Vitest 258 (last result, views, insertion from the data view); `Definition_ActivitiesThatProvideValues_ListThem` covers Liquid's and HTTP Request's last results and the response fields; workflow tests 288/288; CI-flag build clean.

### - [x] 9.9 Global values, functions, and the activity's own values (from review)

- **Global.** The Liquid values every template reads (`{{ Site.SiteName }}`, `User`, `Request`, `Culture`, `Environment`, `Content`), with their fields, listed by `IWorkflowGlobalValueProvider`s.
- **Functions.** The functions scripts call (`uuid()`, `log()`, `setProperty()`, `queryString()`, …), each with a call to insert.
- **This activity.** The values an activity sets before it evaluates its own expressions, such as the `EmailConfirmationUrl` of Register User Task (`ActivityProvidedValue.AvailableToItself`).
- **Inputs and outputs of other workflows.** An **Inputs** group lists the inputs the workflow starts with (its input variables), and the last result of Execute Workflow has the outputs of the workflow it runs as fields.
- **Picker.** The content type picker uses the markup of the bootstrap-select examples.
- **Tests**: the global values in the definition; Vitest for the groups and the Liquid-only and script-only values.
- **Notes from implementing this step:**
  - **Why.**
    - The User Registration sample's Register User Task uses `{{ Site.SiteName }}` and `{{ Workflow.Properties.EmailConfirmationUrl | raw }}`, and neither was listed.
    - `Site` is a global Liquid value. `EmailConfirmationUrl` is set by the task itself before it renders its email, so only the activities after it listed it.
  - **Inventory.**
    - The workflow Liquid evaluator builds its context on `TemplateOptions.Scope`, so it sees the core globals: `Site` (Settings), `User`, `Request`, `Culture`, `Environment`, `HttpContext`, `TrackingConsent`, and `Content` with the Contents feature.
    - Scripts get the `IGlobalMethodProvider` methods (`uuid`, `base64`, `html`, `log`, the HTTP methods) and the workflow methods.
  - **API.**
    - `IWorkflowGlobalValueProvider` returns `WorkflowGlobalValue`s: a kind (`Value` or `Function`), a name, a type, a description, a Liquid path or a JavaScript call, and fields. `WorkflowGlobalValue.Liquid` and `WorkflowGlobalValue.Function` build them.
    - `DefaultWorkflowGlobalValueProvider` (Workflows) lists `Site`, `User`, `Request`, `Culture` and `Environment` with their fields, and the core and workflow functions. `HttpWorkflowGlobalValueProvider` lists the HTTP functions; `ContentWorkflowGlobalValueProvider` (Contents) lists `Content`.
    - The definition carries `globalValues`, sorted by kind and name.
  - **This activity.**
    - `ActivityProvidedValue.AvailableToItself`, set on Register User Task's `EmailConfirmationUrl` and Notify Content Owner's `Owner`, which both set the value before they evaluate their messages.
    - The designer lists them in a **This activity** group, and the completions of the activity's own editors include them.
  - **Client.**
    - **Global** and **Functions** groups after **Workflow**. A Liquid value has no JavaScript button, a function no Liquid one: `AvailableValue.javaScript` and `liquid` can be null, and the completions skip what's missing.
    - A field name can be a path (`User.Identity.Name`).
    - A line under the list links to the Liquid documentation for filters such as `raw`.
  - **Inputs and outputs of other workflows** (from review: "inherited" values).
    - The **Inputs** group, before **Global**, lists the variables marked as inputs as `input("name")` and `{{ Workflow.Input.name }}`, "Passed by the workflow that runs this one, or by what starts it" unless the variable has a description.
    - `ExecuteWorkflowTask` declares its last result with the outputs it stores (`Outputs`) as fields: `lastResult().greeting` after running **Sample: format a greeting**.
  - **Picker.** As in the bootstrap-select examples: `class="selectpicker"`, a `placeholder` instead of `title`, no `form-select`, one field and one caret.
  - **Verified in the preview.**
    - Register User Task (User Registration) lists **This activity**: `EmailConfirmationUrl`, and **Global**: `Site` with `{{ Site.SiteName }}`, then **Functions**.
    - The Content Created picker is a single field.
  - **Tests.**
    - Vitest 262: the inputs, the global and function values, the Liquid-only and script-only buttons, the filters link, and the activity's own values.
    - `Definition_ActivitiesThatProvideValues_ListThem` checks `Site` with its `SiteName`, `uuid()`, the HTTP functions, `Content`, and the outputs of Execute Workflow as the fields of its last result.
    - Workflow tests 288/288; CI-flag build clean.

### - [x] 9.10 End-to-end test

- The available data of an activity after a content event lists its content item, and clicking an expression inserts it into the editor.
- A script error shows a warning on the instance page; with the setting, the instance is faulted at that activity.
- **Notes from implementing this step:**
  - **Seeded workflows** (`workflows-designer-tests.recipe.json`).
    - **Available data**: Content Published → Retrieve Content → Log.
    - **Script errors**: HTTP request → a Script that reads `input('Order').Total` and fails → Set Property.
    - The recipe also enables `OrchardCore.ContentTypes` and adds an **Article** type, so the content type picker has an option.
  - **`AvailableData_LogAfterAContentEvent_ListsTheDataAndInsertsAField`.**
    - Picks Article in the event's picker (a tag, saved, still there after a reload).
    - On the Log task: the event's `input("ContentItem")`, the last result typed as a content item from Retrieve Content, and the **Global** group.
    - Clicking the Liquid of `ContentEvent.ContentType` inserts it after the typed text, and the settings come back.
  - **`ScriptErrors_ScriptFails_TheInstanceShowsItOrFaultsWithTheSetting`.**
    - By default, the instance finishes; the Script is `has-script-errors` with "Total" in its badge, and the legend and the journal's **Script error** badge show.
    - With **Fault the workflow on script errors** checked and published, the next instance faults at the Script, and Set Property doesn't run.
  - **Logged errors.** `OrchardTestServer` ignores the errors this workflow logs on purpose (the evaluator's error for `input('Order').Total`, and the `WorkflowScriptException` fault), as it does for the transient failure activity.
  - **Tests.** The designer functional class passes 27/27.

## Definition of done (Phase 9)

- [x] Steps 9.1–9.10 are checked.
- [x] The CI-flag build is green; `OrchardCore.Tests`, Vitest and the functional `*Cms*` tests pass.
- [x] `yarn lint`, `yarn check` and `yarn build` leave a clean `git status`.
- [x] Workflows run as before when the new setting is off.
- [x] Docs and release notes are updated.
- **Notes from checking the Definition of done:**
  - **Build.** The CI-flag build of `OrchardCore.slnx` has 0 warnings and 0 errors.
  - **`OrchardCore.Tests`.** 3,664 tests: 3,662 passed and 2 skipped (Unix-only).
  - **Vitest.** The designer's 262 tests pass.
  - **Functional `*Cms*`.**
    - 167 tests. In the run with every host at once, 131 passed and 36 failed in 0 ms, as their hosts timed out starting together on this machine.
    - The 9 classes that failed passed when run again four at a time: 36 of 36. `WorkflowsDesignerTests` has 27 tests.
  - **Assets.**
    - `yarn lint` has 0 errors (the 2 warnings in `OrchardCore.Cors` were already there), and `yarn check` passes.
    - The full `yarn build` couldn't start its processes on the busy machine ("spawn UNKNOWN", Parcel crashing with 0xC0000409), even with the Parcel cache cleared.
    - Built in batches of 6 with `yarn build -n` (227 groups in 38 batches, all on the first try), it reproduces the committed output. `git status` only shows the six line-ending files from Phase 1.
  - **Setting off.** `FaultOnScriptErrors` is off by default and stored workflows don't have it. With it off, a failing script still returns its default value and the instance goes on (`WorkflowManagerTests`), and the other workflow tests pass unchanged.
  - **Docs.** Available Data (the views, the groups, Last Result, global values and functions), Script Errors, the `SelectContentTypes` picker, and the release notes.
  - **From review, after 9.10.**
    - `Co-Authored-By` trailers were removed from the branch's commit messages (the code is unchanged) and the branch was force-pushed; the plan cites the new hashes.
    - TheAdmin's and TheTheme's `.btn-theme` helper now sets Bootstrap's button variables, so bootstrap-select's toggle, whose default style class is also `btn-theme`, keeps its border. The admin list filters use `btn-sm` and don't change.

## Open questions

- Should the Issues tab warn about a script that reads a value no activity on a path provides?
- Should Liquid errors be recorded too? Liquid renders what it can, so it rarely fails at run time.

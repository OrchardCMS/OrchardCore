# Workflow activity reference

## Base classes

| Type | Namespace | Use |
|------|-----------|-----|
| `TaskActivity<TActivity>` | `OrchardCore.Workflows.Activities` | typed task; `Name` defaults to type name |
| `TaskActivity` | same | base task (`Activity, ITask`) |
| `EventActivity` | same | base event (`Activity, IEvent`); `Execute` returns `Halt()` |
| `ActivityDisplayDriver<TActivity>` | `OrchardCore.Workflows.Display` | thumbnail + design shapes only |
| `ActivityDisplayDriver<TActivity, TEditViewModel>` | same | adds the edit shape + model mapping |

`Activity` provides `Properties` (a `JsonObject` bag), the `GetProperty`/`SetProperty` helpers, and the `Outcome`/`Outcomes`/`Halt`/`Noop` static helpers.

## Task lifecycle

1. `GetPossibleOutcomes` — called by the designer and runtime to know the ports.
2. `CanExecuteAsync` (optional) — gate before running.
3. `ExecuteAsync` — do work, return `Outcomes("...")`.
4. (If it returned `Halt()`) `ResumeAsync` later.

## Event lifecycle

1. `EventActivity.Execute` returns `Halt()` — workflow suspends, saved to DB.
2. External code raises the event (e.g. `IWorkflowManager.TriggerEventAsync`).
3. `CanExecuteAsync` decides whether this instance should resume.
4. `Resume`/`ResumeAsync` returns the outcome to continue.

## Display driver internals

`ActivityDisplayDriver<TActivity, TEditViewModel>` derives the three shape types from the activity name:

```
{ActivityName}_Fields_Thumbnail
{ActivityName}_Fields_Design
{ActivityName}_Fields_Edit
```

Override the mapping hooks:

```csharp
protected override void EditActivity(TActivity activity, TEditViewModel model) { /* activity -> model */ }
protected override void UpdateActivity(TEditViewModel model, TActivity activity) { /* model -> activity */ }
// async variants: EditActivityAsync, UpdateActivityAsync
```

`UpdateAsync` already calls `context.Updater.TryUpdateModelAsync(viewModel, Prefix)` for you, then `UpdateActivity`, then returns `Edit(...)`.

## Persisting properties

```csharp
public WorkflowExpression<string> Text
{
    get => GetProperty(() => new WorkflowExpression<string>());
    set => SetProperty(value);
}
```

- Key is the member name via `[CallerMemberName]`.
- Stored as JSON in `Properties`; survives halt/resume and process restarts.
- Provide a default factory so unset properties don't return null.

## Expressions

User-facing inputs should be `WorkflowExpression<T>` so authors can use Liquid or JavaScript:

```csharp
var text = await _expressionEvaluator.EvaluateAsync(Text, workflowContext, null);
```

Inject `IWorkflowExpressionEvaluator` (Liquid) and/or `IWorkflowScriptEvaluator` (JS). The view model usually exposes `.Expression` (the raw template string) for the editor.

## Outcomes

Declare in `GetPossibleOutcomes` (localized), return in `Execute`/`Resume` (string names):

```csharp
public override IEnumerable<Outcome> GetPossibleOutcomes(...) => Outcomes(S["Yes"], S["No"]);

public override async Task<ActivityExecutionResult> ExecuteAsync(...)
    => condition ? Outcomes("Yes") : Outcomes("No");
```

`ActivityExecutionResult`:
- `Outcomes(params string[])` — continue down those ports.
- `Halt()` → `ActivityExecutionResult.Halted` (`IsHalted = true`).
- `Noop()` → `ActivityExecutionResult.Empty`.

## Workflow context data

```csharp
public sealed class WorkflowExecutionContext
{
    public IDictionary<string, object> Input { get; }      // from initiator
    public IDictionary<string, object> Output { get; }     // to initiator
    public IDictionary<string, object> Properties { get; } // cross-activity state
}
```

Read input: `workflowContext.Input.GetValue<string>("Key")`. Write output: `workflowContext.Output["Key"] = value`.

## Registration

```csharp
// In Startup.ConfigureServices
services.AddActivity<LogTask, LogTaskDisplayDriver>();
services.AddActivity<SignalEvent, SignalEventDisplayDriver>();
```

`AddActivity` registers the activity, its driver, and adds it to `WorkflowOptions`. Put HTTP/feature-gated activities in a `[RequireFeatures(...)]` startup as the repo does for `OrchardCore.Workflows.Http`.

## Real examples in the repo

| Activity | File | Shows |
|----------|------|-------|
| `LogTask` | `OrchardCore.Workflows/Activities/LogTask.cs` | minimal task, expression eval |
| `NotifyTask` + driver | `OrchardCore.Workflows/{Activities,Drivers}` | task + display driver pair |
| `SetOutputTask` | `OrchardCore.Workflows/Activities/SetOutputTask.cs` | writing `Output` |
| `SetPropertyTask` | same | writing `Properties` |
| `SignalEvent` | `OrchardCore.Workflows/Http/Activities/SignalEvent.cs` | event gate + resume |
| `TimerEvent` | `OrchardCore.Workflows/Timers/TimerEvent.cs` | event with persisted state |

---
name: orchardcore-workflow-activity
description: Creates custom OrchardCore workflow activities — tasks and events — with their display drivers and registration. Use when the user needs a new workflow Task or Event, custom outcomes, activity editor UI, or to read/write workflow input, output, and properties.
---

# OrchardCore Workflow Activity

This skill guides you through writing custom workflow activities following project conventions.

A workflow is a graph of **activities** connected by **outcomes**. Two activity kinds:

- **Task** — does work, then returns one or more outcomes (`TaskActivity`). Cannot start a workflow.
- **Event** — waits for something to happen, halting the workflow until resumed (`EventActivity`). Can be a workflow start point.

Each activity pairs with a **display driver** that renders its thumbnail, design, and editor shapes, and is registered with `services.AddActivity<TActivity, TDriver>()`.

## Decide: Task or Event

| Need | Use |
|------|-----|
| Perform an action and continue (log, notify, HTTP call, set value) | `TaskActivity<T>` |
| Wait for / react to an external trigger (signal, timer, HTTP request) | `EventActivity` |
| Start a workflow | Event (tasks can't start) |

## Workflow

### Step 1: Create the activity class

A **task**:

```csharp
public class LogTask : TaskActivity<LogTask>
{
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    protected readonly IStringLocalizer S;

    public LogTask(IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizer<LogTask> localizer)
    {
        _expressionEvaluator = expressionEvaluator;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Log Task"];
    public override LocalizedString Category => S["Primitives"];

    // Persisted property — stored in the activity's Properties bag
    public WorkflowExpression<string> Text
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var text = await _expressionEvaluator.EvaluateAsync(Text, workflowContext, null);
        // ... do work ...
        return Outcomes("Done");
    }
}
```

An **event** halts in `Execute`, gates on `CanExecuteAsync`, and resumes in `Resume`:

```csharp
public class SignalEvent : EventActivity
{
    public static string EventName => nameof(SignalEvent);
    public override string Name => EventName;

    public override LocalizedString DisplayText => S["Signal Event"];
    public override LocalizedString Category => S["HTTP"];

    public WorkflowExpression<string> SignalName
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var signalName = await _expressionEvaluator.EvaluateAsync(SignalName, workflowContext, null);
        return string.Equals(workflowContext.Input.GetValue<string>("Signal"), signalName, StringComparison.OrdinalIgnoreCase);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes(S["Done"]);

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcomes("Done");
}
```

`EventActivity.Execute` returns `Halt()` by default — the workflow persists and waits.

### Step 2: Create the display driver

```csharp
public sealed class LogTaskDisplayDriver : ActivityDisplayDriver<LogTask, LogTaskViewModel>
{
    protected override void EditActivity(LogTask activity, LogTaskViewModel model)
        => model.Text = activity.Text.Expression;

    protected override void UpdateActivity(LogTaskViewModel model, LogTask activity)
        => activity.Text = new WorkflowExpression<string>(model.Text);
}
```

`ActivityDisplayDriver<TActivity, TEditViewModel>` wires thumbnail/design/edit shapes automatically. You only map between activity and view model.

### Step 3: Create the shape templates

Three Razor shapes, named by activity:

| Shape | File | Renders |
|-------|------|---------|
| Thumbnail | `LogTask_Fields_Thumbnail.cshtml` | activity picker entry |
| Design | `LogTask_Fields_Design.cshtml` | node on the workflow canvas |
| Edit | `LogTask_Fields_Edit.cshtml` | property editor form |

### Step 4: Register

In `Startup.cs`:

```csharp
services.AddActivity<LogTask, LogTaskDisplayDriver>();
```

### Step 5: Test

Build, open Admin → Workflows, create a workflow, drop your activity, wire its outcome, run. Events: trigger the external condition and confirm the halted workflow resumes.

## Quick Reference

### Required / common members

| Member | Purpose |
|--------|---------|
| `Name` | technical id (defaults to type name via `TaskActivity<T>`) |
| `DisplayText` | localized label (`S["..."]`) |
| `Category` | grouping in the activity picker |
| `GetPossibleOutcomes(...)` | declares outcome ports |
| `ExecuteAsync(...)` | task work; return `Outcomes(...)` |
| `CanExecuteAsync(...)` | event gate (true → may run/resume) |
| `Resume(...)` / `ResumeAsync(...)` | event continuation after halt |

### Outcome & result helpers (from `Activity`)

| Call | Meaning |
|------|---------|
| `Outcomes("Done")` | continue down the `Done` port |
| `Outcomes("Yes", "No")` | multiple outcomes |
| `Halt()` | suspend (event waiting) |
| `Noop()` | empty result |
| `Outcomes(S["Done"])` | declare a possible outcome (in `GetPossibleOutcomes`) |

### Persisting state

`GetProperty`/`SetProperty` store into the activity's `Properties` JSON bag, keyed by member name (`[CallerMemberName]`):

```csharp
public LogLevel LogLevel
{
    get => GetProperty(() => LogLevel.Information); // default if unset
    set => SetProperty(value);
}
```

### Workflow-level data (`WorkflowExecutionContext`)

| Dict | Use |
|------|-----|
| `Input` | values supplied by the initiator (read) |
| `Output` | values returned to the initiator (write) |
| `Properties` | shared state between activities |

```csharp
workflowContext.Output["Result"] = value;
workflowContext.Properties["Counter"] = 1;
var signal = workflowContext.Input.GetValue<string>("Signal");
```

## Gotchas

- Use `WorkflowExpression<string>` for user-editable fields so authors can write Liquid/JS; evaluate with `IWorkflowExpressionEvaluator`.
- Events must override `Name` to a stable value (often a `static EventName`) — other code references it by name to resume.
- `GetPossibleOutcomes` returns `Outcome` objects (localized labels); `ExecuteAsync` returns `ActivityExecutionResult` via `Outcomes("...")` strings. Keep the names aligned.
- Long-running events persist the workflow to the DB; don't hold non-serializable state on the activity between halt and resume.

## References

- `references/activities.md` — base classes, state, outcomes, expression evaluation, full examples
- `src/docs/reference/modules/Workflows/README.md` (repo) — official reference + built-in activity catalog
- `src/docs/topics/workflows/README.md` (repo) — concepts
- `AGENTS.md` (repo root) — build commands

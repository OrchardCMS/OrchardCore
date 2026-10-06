# Workflows (`OrchardCore.Workflows`)

The Workflows module provides a way for users to visually implement business rules using flowchart diagrams.

## General Concepts

A workflow is a collection of **activities** that are connected to each other. These connections are called **transitions**.  
Activities and their transitions are stored in a **Workflow Definition**.

A workflow is essentially a visual script, where each activity is a statement of that script.

There are two types of activities: **Task** and **Event**.  
A Task activity typically performs an action, such as publishing a content item, while an Event activity typically listens for an event to happen before execution continues.

In order for a workflow to execute, at least one activity must be marked as the *start of the workflow*.  
Only Event activities can be marked as the start of a workflow.  
An example of such an event activity is *Content Created*, which executes whenever a content item is created.  
A workflow can have more than one start event. This allows you to trigger (run) a workflow in response to various types of events.

Each activity has one or more **outcomes**, which represent a source endpoint from which a connection can be made to the next activity, which are called transitions.  
By connecting activities, you are effectively creating a program that can be executed by Orchard in response to a multitude of events.

## Workflow Designer

Workflow definitions are edited in the workflow designer: open **Workflows** in the admin menu, then select a workflow.

![The workflow designer](docs/workflow-designer.png)

The designer has three areas:

- **Activities** (on the left) lists the activities you can add, grouped by category. Search them by name or category, or show only events or tasks. Drag an activity onto the canvas, or click it to add it in the middle of the view.
- **The canvas** (in the middle) shows the activities and the transitions between them. Each activity shows its type, its settings, and one port per outcome.
    - Drag from an outcome's port to another activity to connect them. An outcome has at most one transition, so connecting it again replaces its previous transition.
    - Start activities show a **Start** badge, and activities with problems show their number of issues.
    - Right-click an activity, a transition or the canvas for more actions, such as making an event the start activity.
- **The properties panel** (on the right) has three tabs:
    - **Activity**: the editor of the selected activity. Double-click an activity, or select it and press Enter, to edit it.
    - **Workflow**: the settings of the workflow: its name, whether it is enabled, whether it is a singleton, its lock settings, and whether finished instances are deleted.
    - **Issues**: the problems found in the workflow, errors first. Select one to go to its activity.

The activities pane and the properties panel can be collapsed to a narrow rail, to give the canvas more room. Hover a rail to open its pane over the canvas, or click it to expand the pane again. The properties rail shows the three tabs, so you can go straight to one of them. The width of the properties panel and whether each pane is collapsed are remembered in your browser.

### Focusing on Part of a Workflow

In a large workflow, you can hide the activities you aren't working on. Right-click an activity (or press Shift+F10 or the Menu key) and choose **Collapse the activities after it** to hide the activities that come after it. An activity that can also be reached another way stays visible, and start activities are never hidden. The collapsed activity looks like a stack and shows how many activities it hides: click that number, or choose **Show the hidden activities after it**, to show them again.

While activities are hidden, a notice at the bottom of the canvas shows how many, with **Show all**. Hidden activities are still part of the workflow: they are saved, published and executed as usual. Selecting one, for example from the **Issues** tab, shows it again. The collapsed activities of each workflow are remembered in your browser.

### Drafts and Publishing

The designer saves your changes as you make them, to a **draft** of the workflow. A draft doesn't run: the workflow keeps running its published definition until you publish the draft.

- The toolbar shows whether all changes are saved. When the connection is lost, the designer retries until it can save, and leaving the page asks first while changes aren't saved.
- **Publish** makes the draft the published definition. Errors must be fixed first. When the workflow has warnings or running instances, the designer asks before publishing. Running instances continue on the new definition.
- **Discard draft** deletes the draft and goes back to the published definition.
- When someone else changes the workflow while you edit it, the designer stops saving and asks what to do. **Reload** loads their version and drops your unsaved changes. **Overwrite** saves your layout and connections over theirs.
- When someone else last changed the draft, a banner shows who and when.

The changes made in an activity editor are applied when you leave a field, and when you select another activity. When a field is invalid, the editor shows the error, and the designer asks before you leave that activity.

**Instances** lists the instances of the workflow, and **Export** downloads its published definition as a recipe.

### Keyboard Shortcuts

| Keys | Action |
|------|--------|
| Ctrl+Z, Ctrl+Y (or Ctrl+Shift+Z) | Undo, redo a change on the canvas |
| Tab | Move between activities; the focused activity is selected |
| Enter | Edit the selected activity |
| Escape, in the properties panel | Go back to the activity on the canvas |
| Arrow keys | Move the selected activities by one grid cell, or by one pixel with Shift |
| Delete, Backspace | Delete the selection; the toast that follows can undo it |
| Ctrl+A | Select all the activities |
| Escape | Clear the selection |
| Shift+F10, Menu key | Open the actions of the focused activity, including **Connect an outcome to…** |
| +, − | Zoom in, zoom out |
| Ctrl+wheel | Zoom around the pointer |
| Drag the background, Space+drag, middle button, wheel | Pan |

Undo covers the changes made on the canvas: adding, moving, connecting and deleting activities. The changes made in an activity editor are applied on the server, so the undo history starts over after them.

### Right-to-Left Languages

In right-to-left languages, the activities pane and the properties panel swap sides, but the canvas isn't mirrored: a workflow looks the same to every user, whatever their language.

### Workflow Instances

The page of a workflow instance shows its workflow in a read-only designer, with the activities the instance waits on (its **blocking** activities) highlighted. Select an activity to see its details. Right-click an activity to collapse the activities after it; the blocking activities are never hidden. The **State** tab shows the instance's state as JSON.

![A workflow instance waiting on a signal](docs/workflow-instance-viewer.png)

## Vocabulary

When working with Orchard Workflows, you will encounter the following terms:

### Workflow Definition

A document (as in a "document-DB" document) that contains all the necessary information about a workflow, such as its name, whether it's enabled or not, its set of activities and their transitions.

### Workflow Instance

A document that represents an "instance" of a workflow definition. A workflow instance contains runtime-state of a workflow.  
Whenever a workflow is started, a new workflow instance is created of a given workflow definition.

### Activity

A step in a workflow definition.  
An activity performs an action and provides zero or more outcomes, which are used to connect to the next activity to execute.  
There are two types of activities: Task and Event.

### Task

A specialized type of activity. Tasks perform actions such as sending emails, publishing content and making HTTP requests.

### Event

A specialized type of activity.  
Like tasks, events can perform actions, but typically all they do is halt the workflow, awaiting an event to happen before continuing on to the next activity.  
When an event is configured as the starting activity of a workflow, that workflow is started when that event is triggered.

### Workflow Designer

The editor that allows you to create and manage a workflow definition visually. See [Workflow Designer](#workflow-designer).

### Activity Editor

Most activities expose settings that can be configured in the activity editor, on the **Activity** tab of the designer's properties panel.
To configure an activity, double-click it on the canvas, or select it and press Enter.

### Activities Pane

The list of the activities you can add to a workflow, on the left of the designer. Drag an activity onto the canvas, or click it to add it.

### Outcome

Each activity has zero or more outcomes. When an activity has executed, it yields control back to the workflow manager along with a list of outcomes.  
The workflow manager uses this list of outcomes to determine which activities to execute next.

Although many activities support multiple outcomes, they typically return only one of them when done executing.  
For example, the *Send Email* activity has two possible outcomes: "Done" and "Failed".  
When the email was sent successfully, it yields "Done" as the outcome, and "Failed" otherwise.

### Transition

A transition is the connection between the outcome of one activity to another activity. Transitions are created by dragging from an outcome's port to another activity in the designer, or with the **Connect an outcome to…** action of an activity.

### Workflow Manager

A service class that can execute workflows. When a workflow is executed, it takes care of creating a workflow instance which is then executed.

### Workflow Execution Context

When the Workflow Manager executes a workflow, it creates an object called the Workflow Execution Context. The Workflow Execution Context is a collection of all information relevant to workflow execution.  
For example, it contains a reference to the workflow instance, workflow definition, correlation values, input, output and properties.  
Each activity has access to this execution context.

### Correlation

Correlation is the act of associating a workflow instance with one or more *identifiers*. These identifiers can be anything.  
For example, when a workflow has the *Content Created* event as its starting point, the workflow instance will be associated, or rather *correlated* to the content item ID that was just created.  
This allows long-running workflow scenarios where only workflow instances associated with a given content item ID are resumed.

### Input

When a workflow is executed, the caller can provide input to the workflow instance. This input is stored in the `Input` dictionary of the workflow execution context.  
This is analogous to providing arguments to a function.

### Output

When a workflow executes, each activity can provide output values to the workflow instance. This output is stored in the `Output` dictionary of the workflow execution context.  
This is analogous to returning values from a function.

### Properties

When a workflow executes, each activity can set property values to the workflow instance. These properties are stored in the `Properties` dictionary of the workflow execution context.  
Each activity can set and access these properties, allowing a workflow to compute and retrieve information that can then be processed by other activities further down the chain.  
This is analogous to a function setting local variables.

## Workflow Execution

When a workflow executes, the **Workflow Manager** creates a **Workflow Instance** and a **Workflow Execution Context**.  
A workflow instance maintains state about the execution, such as which activity to execute next and state that can be provided by individual activities.  
A Workflow Instance is ultimately persisted in the underlying data storage provider, while a Workflow Execution Context exists only in memory for the duration of a workflow execution.  
Workflows can be **short-running** as well as **long-running**.

### Short-running workflows

When a workflow executes without encountering any **blocking** activities (i.e. activities that wait for an event to occur, such as *Signal*), the workflow will run to completion in one go.

### Long-running workflows

When a workflow executes and encounters a blocking activity (such as an event), the workflow manager will *halt* execution and persist the workflow instance to the underlying persistence layer.  
When the appropriate event is triggered (which could happen seconds, days, weeks or even years from now), the workflow manager will load the workflow instance from storage and resume execution.

## Scripts and Expressions

Many activities have settings that can contain either **JavaScript** or **Liquid** syntax.  
For example, activities such as **Correlate**, **For Each**, **For Loop**, **If / Else**, **Set Output**, **Set Property**, and **While Loop** provide a syntax selector in their editors.  
Liquid-enabled fields allow you to enter Liquid markup, enabling access to system-wide variables and filters as well as variables from the **workflow execution context**.

### JavaScript Functions

The following JavaScript functions are available by default to any activity that supports script expressions:

| Function           | Description                                                                                                                                                                                                                                                                                      | Signature                                                                                |
|--------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| `workflow`         | Returns the `WorkflowExecutionContext` which provides access to all information related to the current workflow execution context.                                                                                                                                                               | `workflow(): WorkflowExecutionContext`                                                   |
| `workflowId`       | Returns the unique workflow ID.                                                                                                                                                                                                                                                                  | `workflowId(): String`                                                                   |
| `input`            | Returns the input parameter with the specified name. Input to the workflow is provided when the workflow is executed by the workflow manager.                                                                                                                                                    | `input(name: string): any`                                                               |
| `output`           | Sets an output parameter with the specified name. Workflow output can be collected by the invoker of the workflow.                                                                                                                                                                               | `output(name: string, value: any): void`                                                 |
| `property`         | Returns the property value with the specified name. Properties are a dictionary that workflow activities can read and write information from and to.                                                                                                                                             | `property(name: string): any`                                                            |
| `setProperty`      | Stores the specified data in workflow properties.                                                                                                                                                                                                                                                | `setProperty(name: string,data:any):void`                                                |
| `executeQuery`     | Returns the result of the query, see [more](../Queries/README.md#scripting).                                                                                                                                                                                                                     | `executeQuery(name: String, parameters: Dictionary<string,object>): IEnumerable<object>` |
| `log`              | Output logs according to the specified log level. Allowed log levels : `'Trace','Debug','Information','Warning','Error','Critical','None'`                                                                                                                                                       | `log(level: string, text: string, param: object): void`                                  |
| `lastResult`       | Returns the value that the previous activity provided, if any.                                                                                                                                                                                                                                   | `lastResult(): any`                                                                      |
| `correlationId`    | Returns the correlation value of the workflow instance.                                                                                                                                                                                                                                          | `correlationId(): string`                                                                |
| `setCorrelationId` | Set the correlation value of the workflow instance.                                                                                                                                                                                                                                              | `setCorrelationId(id:string): void`                                                      |
| `signalUrl`        | Returns workflow trigger URL with a protected SAS token into which the specified signal name is encoded. Use this to generate URLs that can be shared with trusted parties to trigger the current workflow if it is blocked on the Signal activity that is configured with the same signal name. | `signalUrl(signal: string): string`                                                      |

#### JavaScript Functions in HTTP activities

The following JavaScript functions are available by default to any HTTP activity that supports script expressions:

| Function                 | Description                                                                                                                                                                                                 | Signature                                                                                                                           |
|--------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------|
| `httpContext`            | Returns the `HttpContext` which encapsulates all HTTP-specific information about an individual HTTP request.                                                                                                | `httpContext(): HttpContext`                                                                                                        |
| `queryString`            | Returns the entire query string (including the leading `?`) when invoked with no arguments, or the value(s) of the parameter name passed in as an argument.                                                 | `queryString(): String`<br/>`queryString(name: String): String` or `Array`                                                          |
| `responseWrite`          | Writes the argument string directly to the HTTP response stream.                                                                                                                                            | `responseWrite(text: String): void`                                                                                                 |
| `absoluteUrl`            | Returns the absolute URL for the relative path argument.                                                                                                                                                    | `absoluteUrl(relativePath: String): String`                                                                                         |
| `readBody`               | Returns the raw HTTP request body.                                                                                                                                                                          | `readBody(): String`                                                                                                                |
| `requestForm`            | Returns the value(s) of the form field name passed in as an argument.                                                                                                                                       | `requestForm(): String`<br/>`requestForm(name: String): String` or `Array`                                                          |
| `deserializeRequestData` | Deserializes the request data automatically for requests that send JSON or form data. Returns the entire request data as a JSON object. Replaces deprecated queryStringAsJson and requestFormAsJson methods | `deserializeRequestData(): { "field1": [ "field1-value1", "field1-value2" ], "field2": [ "field2-value1", "field2-value2" ], ... }` |

### Liquid Expressions

The following Liquid tags, properties and filters are available by default to any activity that supports Liquid expressions:

| Expression               | Type     | Description                                                                                                               | Example                               |
|--------------------------|----------|---------------------------------------------------------------------------------------------------------------------------|---------------------------------------|
| `Workflow.CorrelationId` | Property | Returns the correlation value of the workflow instance.                                                                   | `{{ Workflow.CorrelationId }}`        |
| `Workflow.Input`         | Property | Returns the Input dictionary.                                                                                             | `{{ Workflow.Input["ContentItem"] }}` |
| `Workflow.Output`        | Property | Returns the Output dictionary.                                                                                            | `{{ Workflow.Output["SomeResult"] }}` |
| `Workflow.Properties`    | Property | Returns the Properties dictionary.                                                                                        | `{{ Workflow.Properties["Foo"] }}`    |
| `signal_url`             | Filter   | Returns the workflow trigger URL. You can use the `input("Signal")` JavaScript method to check which signal is triggered. | `{{ 'Approved' \| signal_url }}`      |

Instead of using the indexer syntax on the three workflow dictionaries `Input`, `Output` and `Properties`, you can also use dot notation, e.g.:

```liquid
{{ Workflow.Input.ContentItem }}
```

### Liquid Expressions and ContentItem Events

When handling content related events using a workflow, the content item in question is made available to the workflow via the `Input` dictionary.  
For example, if you have a workflow that starts with the **Content Created Event** activity, you can send an email or make an HTTP request and reference the content item from liquid-enabled fields as follows:

```liquid
{{ Workflow.Input.ContentItem | display_url }}
{{ Workflow.Input.ContentItem | display_text }}
{{ Workflow.Input.ContentItem.DisplayText }}
```

For more examples of supported content item filters, see the documentation on [Liquid](../Liquid/README.md).

## Activities out of the box

The following activities are available with any default Orchard installation:

| Activity                      | Type  | Description                                                                         |
|-------------------------------|-------|-------------------------------------------------------------------------------------|
| **Workflows**                 | *     | *                                                                                   |
| Correlate                     | Task  | Correlate the current workflow instance with a value.                               |
| For Each                      | Task  | Iterate over a JavaScript list or a Liquid-rendered JSON or comma-separated list.  |
| Fork                          | Task  | Fork workflow execution into separate paths of execution.                           |
| For Loop                      | Task  | Iterate between JavaScript or Liquid numeric start and end values.                  |
| If / Else                     | Task  | Evaluate a JavaScript or Liquid boolean condition and continue execution based on the outcome. |
| Join                          | Task  | Join a forked workflow execution back into a single path of execution.              |
| Liquid                        | Task  | Evaluate a Liquid expression and store the result as the workflow's last result.    |
| Log                           | Task  | Write a log entry.                                                                  |
| Notify                        | Task  | Display a notification.                                                             |
| Script                        | Task  | Execute script and continue execution based on the returned outcome.                |
| Set Output                    | Task  | Evaluate a JavaScript or Liquid expression and store the result into the workflow's output. |
| Set Property                  | Task  | Evaluate a JavaScript or Liquid expression and store the result into workflow properties. |
| While Loop                    | Task  | Iterate while a JavaScript or Liquid condition is true.                             |
| **HTTP Workflow Activities**  | *     | *                                                                                   | * |
| HTTP Redirect                 | Task  | Redirect the user agent to the specified URL (301/302).                             |
| HTTP Request                  | Task  | Perform a HTTP request to a given URL.                                              |
| Filter Incoming HTTP Request  | Event | Executes when the specified HTTP request comes in. Similar to an MVC Action Filter. |
| Signal                        | Event | Executes when a signal is triggered.                                                |
| **Email**                     | *     | *                                                                                   | * |
| Send Email                    | Task  | Send an email.                                                                      |
| **Timer Workflow Activities** | *     | *                                                                                   | * |
| Timer                         | Event | Executes repeatedly according to a specified CRON expression.                       |
| **Contents**                  | *     | *                                                                                   | * |
| Content Created               | Event | Executes when content is created.                                                   |
| Content Deleted               | Event | Executes when content is deleted.                                                   |
| Content Published             | Event | Executes when content is published.                                                 |
| Content Unpublished           | Event | Executes when content is unpublished.                                               |
| Content Updated               | Event | Executes when content is updated.                                                   |
| Content Versioned             | Event | Executes when content is versioned.                                                 |
| Create Content                | Task  | Create a content item.                                                              |
| Delete Content                | Task  | Delete a content item.                                                              |
| Publish Content               | Task  | Publish a content item.                                                             |
| **User**                      | *     | *                                                                                   | * |
| ValidateUser                  | Task  | Used to check if the user is logged in and has the specified role(s).               |

## Developing Custom Activities

Orchard is built to be extended, and the `Workflows` module is no different. When creating your own module, you can develop custom workflow activities.  
Developing custom activities involve the following steps:

1. Create a new class that directly or indirectly implements `IActivity`. In most cases, you either derive from `TaskActivity` or `EventActivity`, depending on whether your activity represents an event or not. Although not required, it is recommended to keep this class in a folder called `Activities`.
2. Create a new **display driver** class that directly or indirectly implements `IDisplayDriver`. An activity display driver controls the activity's display on the designer's **canvas**, in its **activities pane** and in the **activity editor**. Although not required, it is recommended to keep this class in a folder called `Drivers`.
3. Optionally implement a **view model** if your activity has properties that the user should be able to configure.
4. Implement the various Razor views for the various shapes provided by the driver. Although not required, it is recommended to store these files in the `Views/Items` folder. Note that it is required for your views to be discoverable by the display engine.  

You may trigger a custom event activity by calling the `TriggerEventAsync` method on `IWorkflowManager`. The following is an example of how to trigger the workflow for a custom event named `CustomTaskActivity`

```csharp
var customData = new CustomDto();

var input = new Dictionary<string, object>()
{
    // Here we are passing custom data to the workflow's input.
    { "data", customData}
};

await workflowManager.TriggerEventAsync("CustomTaskActivity", input);
```

You may passing an instance of a custom object to the workflow's input by adding it to the input collection. If you are looking to use liquid to access the member of the custom object, you must register a member access strategy. The following example for defining a custom type.

```csharp
services.Configure<TemplateOptions>(o =>
{
    o.MemberAccessStrategy.Register<CustomDto>();
});
```

### Activity Display Types

An activity has the following display types:

- Thumbnail
- Design

**Thumbnail**
Used for the activity's card in the designer's activities pane.

**Design**
Used for the body of the activity on the designer's canvas.

The designer renders the `Content` zone of these shapes, where `ActivityDisplayDriver` places the `{Name}_Fields_Thumbnail` and `{Name}_Fields_Design` shapes. It draws the card, the node, the icon and the outcome ports itself.

### Activity Icons

The designer shows an icon for each activity, on the canvas and in the activities pane. An activity that doesn't declare one gets the default icon of its category, or a generic event or task icon. To declare one, set a Font Awesome class when registering the activity:

```csharp
services.AddActivity<NotifyTask, NotifyTaskDisplayDriver>(activity => activity.Icon = "fa-solid fa-bell");
```

### Activity Editors in the Designer

The designer loads an activity's editor (the `{Name}_Fields_Edit` shape) into its properties panel without reloading the page. It posts the editor as a form when a field changes, and replaces it when another activity is selected. Editors made of plain form fields need nothing more. An editor that has a script must be safe to inject:

- **Register the script as a resource** (`<script asp-name="…" at="Foot">` or `asp-src`), so the designer loads it with the editor. A script that is already on the page isn't loaded again, so it must also initialize the editors that are added to the page later. In the Orchard Core repository, `observeAndInit(selector, init)` from `@orchardcore/bloom/helpers/observeAndInit` does this: it runs `init` once for every matching element, including the ones added later.
- **Scope the script to its editor**, for example with a `data-task-editor="my-task"` wrapper element, and read element ids from `data-` attributes rather than hard-coding them.
- **Keep the form fields up to date.** The designer collects the form with `FormData`. It dispatches a `submit` event on the form first, so rich editors that copy their value when the form is submitted keep working. Dispatch a bubbling `change` event on a field when the user commits an edit, so the designer applies it.
- **Release what the script creates** when the editor is removed. Right before replacing an editor, the designer dispatches the bubbling `oc:editor-unmounting` event on the element that contains it.

The bloom helpers in `@orchardcore/bloom/helpers/editorLifecycle` implement this contract:

- `onEditorUnmounting(element, dispose)` calls `dispose` once, when the editor is removed.
- `dispatchFieldChange(field)` reports an edit.
- `bindCodeMirrorToTextArea(editor, textArea)` does all of it for a CodeMirror 5 editor.

```typescript
import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";
import { bindCodeMirrorToTextArea } from "@orchardcore/bloom/helpers/editorLifecycle";

observeAndInit('[data-task-editor="my-task"]', (element) => {
    const textArea = element.querySelector<HTMLTextAreaElement>(`#${CSS.escape(element.dataset.expressionId ?? "")}`);

    if (textArea) {
        bindCodeMirrorToTextArea(CodeMirror.fromTextArea(textArea, { mode: { name: "liquid" } }), textArea);
    }
});
```

The validation errors that the driver's `UpdateAsync` adds are shown in the editor.

### IActivity

`IActivity` has the following members:

- `Name`
- `Category`
- `DisplayText`
- `Properties`
- `HasEditor`
- `GetPossibleOutcomes`
- `CanExecuteAsync`
- `ExecuteAsync`
- `ResumeAsync`
- `OnInputReceivedAsync`
- `OnWorkflowStartingAsync`
- `OnWorkflowStartedAsync`
- `OnWorkflowResumingAsync`
- `OnWorkflowResumedAsync`
- `OnActivityExecutingAsync`
- `OnActivityExecutedAsync`

The following is an example of a simple task activity implementation that displays a notification:

```csharp
public class NotifyTask : TaskActivity
{
    private readonly INotifier _notifier;
    private readonly IStringLocalizer S;
    private readonly IHtmlLocalizer H;

    public NotifyTask(INotifier notifier, IStringLocalizer<NotifyTask> s, IHtmlLocalizer<NotifyTask> h)
    {
        _notifier = notifier;

        S = s;
        H = h;
    }

    // The technical name of the activity. Activities on a workflow definition reference this name.
    public override string Name => nameof(NotifyTask);

    // The displayed name of the activity, so it can use localization.
    public override LocalizedString DisplayText => S["Notify Task"];

    // The category to which this activity belongs. The activity picker groups activities by this category.
    public override LocalizedString Category => S["UI"];

    // A description of this activity's purpose. 
    public override LocalizedString Description => S["Display a message."];

    // The notification type to display.
    public NotifyType NotificationType
    {
        get => GetProperty<NotifyType>();
        set => SetProperty(value);
    }

    // The message to display.
    public WorkflowExpression<string> Message
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    // Returns the possible outcomes of this activity.
    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcomes(S["Done"]);
    }

    // This is the heart of the activity and actually performs the work to be done.
    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var message = await workflowContext.EvaluateExpressionAsync(Message);
        _notifier.Add(NotificationType, H[message]);
        return Outcomes("Done");
    }
}
```

The following is an example of a simple activity display driver:

```csharp
public class NotifyTaskDisplayDriver : ActivityDisplayDriver<NotifyTask, NotifyTaskViewModel>
{
    protected override void EditActivity(NotifyTask activity, NotifyTaskViewModel model)
    {
        model.NotificationType = activity.NotificationType;
        model.Message = activity.Message.Expression;
    }

    protected override void UpdateActivity(NotifyTaskViewModel model, NotifyTask activity)
    {
        activity.NotificationType = model.NotificationType;
        activity.Message = new WorkflowExpression<string>(model.Message);
    }
}
```

The above code performs a simple mapping of a `NotifyTask` to a `NotifyTaskViewModel` and vice versa.  
This simple implementation is possible because the actual creation of the necessary editor and display shapes are taken care of by `ActivityDisplayDriver<TActivity, TEditViewModel>`, which looks like this (modified to focus on the important parts):

```csharp
public abstract class ActivityDisplayDriver<TActivity, TEditViewModel> : ActivityDisplayDriver<TActivity> where TActivity : class, IActivity where TEditViewModel : class, new()
{
    private static string ThumbnailshapeType = $"{typeof(TActivity).Name}_Fields_Thumbnail";
    private static string DesignShapeType = $"{typeof(TActivity).Name}_Fields_Design";
    private static string EditShapeType = $"{typeof(TActivity).Name}_Fields_Edit";

    public override Task<IDisplayResult> DisplayAsync(TActivity activity, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape(ThumbnailshapeType, new ActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape(DesignShapeType, new ActivityViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }

    public override IDisplayResult Edit(TActivity activity, BuildEditorContext context)
    {
        return Initialize<TEditViewModel>(_editShapeType, viewModel => EditActivityAsync(activity, viewModel)).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(TActivity activity, UpdateEditorContext context)
    {
        var viewModel = new TEditViewModel();
        if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            await UpdateActivityAsync(viewModel, activity);
        }

        return Edit(activity, context);
    }
}
```

Notice that the shape names are derived from the activity type, effectively implementing a naming convention for the shape template names to use.  
Continuing with the `NotifyTask` example, we now need to create the following Razor files:

- `NotifyTask.Fields.Design.cshtml`
- `NotifyTask.Fields.Thumbnail.cshtml`
- `NotifyTask.Fields.Edit.cshtml`

## Recipe Configuration

Workflow trimming settings can be configured using the `Settings` recipe step:

```json
{
  "steps": [
    {
      "name": "settings",
      "WorkflowTrimmingSettings": {
        "RetentionDays": 90,
        "Disabled": false,
        "Statuses": [
          "Finished",
          "Faulted",
          "Aborted"
        ]
      }
    }
  ]
}
```

| Property        | Type            | Description                                                                                                                                    |
|-----------------|-----------------|------------------------------------------------------------------------------------------------------------------------------------------------|
| `RetentionDays` | Integer         | The number of days to retain workflow instances before trimming. Default: `90`.                                                                |
| `Disabled`      | Boolean         | Whether automatic workflow trimming is disabled.                                                                                               |
| `Statuses`      | Array of String | The workflow statuses to include in trimming. Values: `Idle`, `Starting`, `Resuming`, `Executing`, `Halted`, `Finished`, `Faulted`, `Aborted`. |

## Trimming

Old workflow instances can be automatically deleted with the Trimming feature. This is enabled by default and you can configure it (including disabling it) in Settings → Workflow Trimming. Without trimming, workflow instances remain in the database indefinitely.

By default, the trimming background task runs once a day and removes at most 5000 workflow instances. You can change the frequency of the background task via [the `OrchardCore.BackgroundTasks` configuration](../BackgroundTasks/README.md), and the batch size via the `OrchardCore:Workflows` configuration from e.g. an `appsettings` file:

```json
{
  "OrchardCore": {
    "Workflows": {
      "Trimming": {
        "BatchSize": 1000
      }
    }
  }
}
```

See [Configuration](../Configuration/README.md) for more information on such configuration.

!!! tip
    If you enable the trimming feature on a site that has tens or even hundreds of thousands of workflow instances already, the initial trimming operation may take weeks to complete. You can expedite this by lowering the background task's frequency, even to once a minute temporarily with the `* * * * *` cron expression.

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/n-O4WO6dVJk" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/IcR-YpxKlGQ" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/pi_WiSqp5x4" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/Sd-aYy5DblI" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

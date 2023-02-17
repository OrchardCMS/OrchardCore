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
An example of such an event activity is _Content Created_, which executes whenever a content item is created.  
A workflow can have more than one start event. This allows you to trigger (run) a workflow in response to various types of events.

Each activity has one or more **outcomes**, which represent a source endpoint from which a connection can be made to the next activity, which are called transitions.  
By connecting activities, you are effectively creating a program that can be executed by Orchard in response to a multitude of events.

![The workflow editor](docs/workflow-editor.png)

1. Activity Picker (Task / Event)
2. Activity actions (click an activity to display activity actions)
3. An activity configured as the starting activity of the workflow.
4. An activity.
5. An Outcome ("Done") of an activity.
6. A transition between two activities (from "Content Created" via the "Done" outcome to the "Send Email" activity).
7. The workflow editor design surface.
8. Edit the workflow definition properties (Name, Enabled, etc.)
9. List the workflow instances for this workflow definition.

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

### Workflow Editor

An editor that allows you to create and manage a workflow definition using a drag & drop visual interface.

### Activity Editor

Most activities expose settings that can be configured via the activity editor.  
To configure an activity, you can either double-click an activity on the design surface of the workflow editor, or click an activity once to activate a small popup that provides various actions you can perform on an activity.  
One of these actions is the *Edit* action.

### Activity Picker

When you are in the Workflow Editor, you use the Activity Picker to add activities to the design surface.  
Open the activity picker by clicking **Add Task** or **Add Event** to add a task or event, respectively.

### Outcome

Each activity has zero or more outcomes. When an activity has executed, it yields control back to the workflow manager along with a list of outcomes.  
The workflow manager uses this list of outcomes to determine which activities to execute next.

Although many activities support multiple outcomes, they typically return only one of them when done executing.  
For example, the _Send Email_ activity has two possible outcomes: "Done" and "Failed".  
When the email was sent successfully, it yields "Done" as the outcome, and "Failed" otherwise.

### Transition

A transition is the connection between the outcome of one activity to another activity. Transitions are created using drag & drop operations in the workflow editor.

### Workflow Manager

A service class that can execute workflows. When a workflow is executed, it takes care of creating a workflow instance which is then executed.

### Workflow Execution Context

When the Workflow Manager executes a workflow, it creates an object called the Workflow Execution Context. The Workflow Execution Context is a collection of all information relevant to workflow execution.  
For example, it contains a reference to the workflow instance, workflow definition, correlation values, input, output and properties.  
Each activity has access to this execution context.

### Correlation

Correlation is the act of associating a workflow instance with one or more _identifiers_. These identifiers can be anything.  
For example, when a workflow has the _Content Created_ event as its starting point, the workflow instance will be associated, or rather _correlated_ to the content item ID that was just created.  
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

When a workflow executes without encountering any **blocking** activities (i.e. activities that wait for an event to occur, such as _Signal_), the workflow will run to completion in one go.

### Long-running workflows

When a workflow executes and encounters a blocking activity (such as an event), the workflow manager will _halt_ execution and persist the workflow instance to the underlying persistence layer.  
When the appropriate event is triggered (which could happen seconds, days, weeks or even years from now), the workflow manager will load the workflow instance from storage and resume execution.

## Scripts and Expressions

Many activities have settings that can contain either **JavaScript** or **Liquid** syntax.  
For example, when adding the **Notify** activity, its editor shows the following fields:  
These type of fields allow you to enter Liquid markup, enabling access to system-wide variables and filters as well as variables from the **workflow execution context**.

### JavaScript Functions

The following JavaScript functions are available by default to any activity that supports script expressions:

| Function | Description | Signature |
| -------- | ----------- | --------- |
| `workflow` | Returns the `WorkflowExecutionContext` which provides access to all information related to the current workflow execution context. | `workflow(): WorkflowExecutionContext` |
| `workflowId` | Returns the unique workflow ID. | `workflowId(): String` |
| `input` | Returns the input parameter with the specified name. Input to the workflow is provided when the workflow is executed by the workflow manager. | `input(name: string): any` |
| `output` | Sets an output parameter with the specified name. Workflow output can be collected by the invoker of the workflow. | `output(name: string, value: any): void` |
| `property` | Returns the property value with the specified name. Properties are a dictionary that workflow activities can read and write information from and to. | `property(name: string): any` |
| `setProperty` | Stores the specified data in workflow properties. | `setProperty(name: string,data:any):void` |
| `executeQuery` | Returns the result of the query, see [more](../Queries/#scripting). | `executeQuery(name: String, parameters: Dictionary<string,object>): IEnumerable<object>` |
| `log` | Output logs according to the specified log level. Allowed log levels : `'Trace','Debug','Information','Warning','Error','Critical','None'` | `log(level: string, text: string, param: object): void` |
| `lastResult` | Returns the value that the previous activity provided, if any. | `lastResult(): any` |
| `correlationId` | Returns the correlation value of the workflow instance. | `correlationId(): string` |
| `signalUrl` | Returns workflow trigger URL with a protected SAS token into which the specified signal name is encoded. Use this to generate URLs that can be shared with trusted parties to trigger the current workflow if it is blocked on the Signal activity that is configured with the same signal name. | `signalUrl(signal: string): string` |

#### JavaScript Functions in HTTP activities

The following JavaScript functions are available by default to any HTTP activity that supports script expressions:

| Function | Description | Signature |
| -------- | ----------- | --------- |
| `httpContext` | Returns the `HttpContext` which encapsulates all HTTP-specific information about an individual HTTP request. | `httpContext(): HttpContext` |
| `queryString` | Returns the entire query string (including the leading `?`) when invoked with no arguments, or the value(s) of the parameter name passed in as an argument. | `queryString(): String`<br/>`queryString(name: String): String` or `Array` |
| `responseWrite` | Writes the argument string directly to the HTTP response stream. | `responseWrite(text: String): void` |
| `absoluteUrl` | Returns the absolute URL for the relative path argument. | `absoluteUrl(relativePath: String): String` |
| `readBody` | Returns the raw HTTP request body. | `readBody(): String` |
| `requestForm` | Returns the value(s) of the form field name passed in as an argument. | `requestForm(): String`<br/>`requestForm(name: String): String` or `Array` |
| `deserializeRequestData` | Deserializes the request data automatically for requests that send JSON or form data. Returns the entire request data as a JSON object. Replaces deprecated queryStringAsJson and requestFormAsJson methods | `deserializeRequestData(): { "field1": [ "field1-value1", "field1-value2" ], "field2": [ "field2-value1", "field2-value2" ], ... }` |

### Liquid Expressions

The following Liquid tags, properties and filters are available by default to any activity that supports Liquid expressions:

| Expression | Type | Description | Example |
| ---------- | ---- | ----------- | ------- |
| `Workflow.CorrelationId` | Property | Returns the correlation value of the workflow instance. | `{{ Workflow.CorrelationId }}` |
| `Workflow.Input` | Property | Returns the Input dictionary. | `{{ Workflow.Input["ContentItem"] }}` |
| `Workflow.Output` | Property | Returns the Output dictionary. | `{{ Workflow.Output["SomeResult"] }}` |
| `Workflow.Properties` | Property | Returns the Properties dictionary. | `{{ Workflow.Properties["Foo"] }}` |
| `signal_url` | Filter | Returns the workflow trigger URL. You can use the `input("Signal")` JavaScript method to check which signal is triggered. | `{{ 'Approved' \| signal_url }}` |

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

| Activity | Type | Description |
| -------- | ---- | ----------- |
| **Workflows** | * | * |
| Correlate | Task | Correlate the current workflow instance with a value. |
| For Each | Task | Iterate over a list. |
| Fork | Task | Fork workflow execution into separate paths of execution. |
| For Loop | Task | Iterates for N times. |
| If / Else | Task | Evaluate a boolean condition and continues execution based on the outcome. |
| Join | Task | Join a forked workflow execution back into a single path of execution. |
| Log | Task | Write a log entry. |
| Notify | Task | Display a notification. |
| Script | Task | Execute script and continue execution based on the returned outcome. |
| Set Output | Task | Evaluate a script expression and store the result into the workflow's output. |
| Set Property | Task | Execute script and continue execution based on the returned outcome. |
| While Loop | Task | Iterate while a condition is true. |
| **HTTP Workflow Activities** | * | * | * |
| HTTP Redirect | Task | Redirect the user agent to the specified URL (301/302). |
| HTTP Request | Task | Perform a HTTP request to a given URL. |
| Filter Incoming HTTP Request | Event | Executes when the specified HTTP request comes in. Similar to an MVC Action Filter. |
| Signal | Event | Executes when a signal is triggered. |
| **Email** | * | * | * |
| Send Email | Task | Send an email. |
| **Timer Workflow Activities** | * | * | * |
| Timer | Event | Executes repeatedly according to a specified CRON expression. |
| **Contents** | * | * | * |
| Content Created | Event | Executes when content is created. |
| Content Deleted | Event | Executes when content is deleted. |
| Content Published | Event | Executes when content is published. |
| Content Unpublished | Event | Executes when content is unpublished. |
| Content Updated | Event | Executes when content is updated. |
| Content Versioned| Event | Executes when content is versioned. |
| Create Content | Task | Create a content item. |
| Delete Content | Task | Delete a content item. |
| Publish Content | Task | Publish a content item. |
**User** | * | * | * |
| ValidateUser | Task | Used to check if the user is logged in and has the specified role(s). |

## Developing Custom Activities

Orchard is built to be extended, and the `Workflows` module is no different. When creating your own module, you can develop custom workflow activities.  
Developing custom activities involve the following steps:

1. Create a new class that directly or indirectly implements `IActivity`. In most cases, you either derive from `TaskActivity` or `EventActivity`, depending on whether your activity represents an event or not. Although not required, it is recommended to keep this class in a folder called `Activities`.
2. Create a new **display driver** class that directly or indirectly implements `IDisplayDriver`. An activity display driver controls the activity's display on the **workflow editor canvas**, the **activity picker** and the **activity editor**. Although not required, it is recommended to keep this class in a folder called `Drivers`.
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
Used when the activity is rendered as part of the activity picker.

**Design**
Used when the activity is rendered as part of the workflow editor design surface.

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

    public override IDisplayResult Display(TActivity activity)
    {
        return Combine(
            Shape(ThumbnailshapeType, new ActivityViewModel<TActivity>(activity)).Location("Thumbnail", "Content"),
            Shape(DesignShapeType, new ActivityViewModel<TActivity>(activity)).Location("Design", "Content")
        );
    }

    public override IDisplayResult Edit(TActivity activity)
    {
        return Initialize<TEditViewModel>(EditShapeType, model =>
        {
            return EditActivityAsync(activity, model);
        }).Location("Content");
    }

    public async override Task<IDisplayResult> UpdateAsync(TActivity activity, IUpdateModel updater)
    {
        var viewModel = new TEditViewModel();
        if (await updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            await UpdateActivityAsync(viewModel, activity);
        }

        return Edit(activity);
    }
}
```

Notice that the shape names are derived from the activity type, effectively implementing a naming convention for the shape template names to use.  
Continuing with the `NotifyTask` example, we now need to create the following Razor files:

- `NotifyTask.Fields.Design.cshtml`
- `NotifyTask.Fields.Thumbnail.cshtml`
- `NotifyTask.Fields.Edit.cshtml`

## Videos

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/n-O4WO6dVJk" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

<iframe width="560" height="315" src="https://www.youtube-nocookie.com/embed/IcR-YpxKlGQ" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

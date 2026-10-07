using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.UserTasks.Activities;

public class UserTaskEvent : EventActivity, IActivityProvidedValues
{
    protected readonly IStringLocalizer S;

    public UserTaskEvent(IStringLocalizer<UserTaskEvent> localizer)
    {
        S = localizer;
    }

    public override string Name => nameof(UserTaskEvent);
    public override LocalizedString DisplayText => S["User Task Event"];
    public override LocalizedString Category => S["Content"];

    public IList<string> Actions
    {
        get => GetProperty(() => new List<string>());
        set => SetProperty(value);
    }

    public IList<string> Roles
    {
        get => GetProperty(() => new List<string>());
        set => SetProperty(value);
    }

    public override bool CanExecute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var action = GetProvidedAction(workflowContext);
        return Actions.Contains(action);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Actions.SelectMany(a => Outcome(S[a]));
    }

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var action = GetProvidedAction(workflowContext);

        return Outcome(action);
    }

    private static string GetProvidedAction(WorkflowExecutionContext workflowContext)
    {
        return (string)workflowContext.Input[ContentEventConstants.UserActionInputKey];
    }

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "UserAction", TypeName = "string", Description = S["The action the user chose."] },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "ContentItem", TypeName = "contentItem", Description = S["The content item the user acted on."] },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "ContentEvent", TypeName = "object", Description = S["The event: its Name, ContentType, ContentItemId and ContentItemVersionId."] },
        ];
}

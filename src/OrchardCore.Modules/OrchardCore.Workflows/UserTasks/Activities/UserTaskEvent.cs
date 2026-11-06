using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.UserTasks.Activities;

public class UserTaskEvent : EventActivity, IActivityProvidedValues, IActivityDescription
{
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public UserTaskEvent(IStringLocalizerFactory localizerFactory, IStringLocalizer<UserTaskEvent> localizer)
    {
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public override string Name => nameof(UserTaskEvent);
    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UserTaskEvent>("User Task Event");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Content;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);
    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

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
#pragma warning disable OCLOC104 // The action names are entered by the user who designs the workflow.
        return Actions.SelectMany(a => Outcome(S[a]));
#pragma warning restore OCLOC104
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

    private static readonly ActivityProvidedValue[] s_providedValues =
    [
        new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "UserAction", TypeName = "string", Description = LocalizationSource.Create<UserTaskEvent>("The action the user chose.") },
        new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "ContentItem", TypeName = "contentItem", Description = LocalizationSource.Create<UserTaskEvent>("The content item the user acted on."), Members = WorkflowValueMembers.ContentItem },
        new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = "ContentEvent", TypeName = "object", Description = LocalizationSource.Create<UserTaskEvent>("The content event."), Members = WorkflowValueMembers.ContentEvent },
    ];

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        => s_providedValues;
}

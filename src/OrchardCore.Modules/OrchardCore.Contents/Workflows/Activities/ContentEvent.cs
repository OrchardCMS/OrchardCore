using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public abstract class ContentEvent : ContentActivity, IEvent, IActivityProvidedValues
{
    protected ContentEvent(
        IContentManager contentManager,
        IWorkflowScriptEvaluator scriptEvaluator,
        IStringLocalizer localizer)
        : base(contentManager, scriptEvaluator, localizer)
    {
    }

    public IList<string> ContentTypeFilter
    {
        get => GetProperty<IList<string>>(defaultValue: () => []);
        set => SetProperty(value);
    }

    public override async Task<bool> CanExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var content = await GetContentAsync(workflowContext);

        if (content == null)
        {
            return false;
        }

        var contentTypes = ContentTypeFilter.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

        // "" means 'any'.
        return contentTypes.Count == 0 || contentTypes.Any(contentType => content.ContentItem.ContentType == contentType);
    }

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Halt();
    }

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        return Outcome("Done");
    }

    public virtual IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = ContentEventConstants.ContentItemInputKey, TypeName = "contentItem", Description = S["The content item of the event."], Members = WorkflowValueMembers.ContentItem(S) },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = ContentEventConstants.ContentEventInputKey, TypeName = "object", Description = S["The content event."], Members = WorkflowValueMembers.ContentEvent(S) },
        ];
}

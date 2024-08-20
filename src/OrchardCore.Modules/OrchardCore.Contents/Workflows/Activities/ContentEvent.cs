using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Contents.Workflows.Activities;

public abstract class ContentEvent : ContentActivity, IEvent
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
        return Outcomes("Done");
    }
}

using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
using OrchardCore.Localization;
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
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer localizer)
        : base(contentManager, scriptEvaluator, localizerFactory, localizer)
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
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = ContentEventConstants.ContentItemInputKey, TypeName = "contentItem", Description = LocalizationSource.Create<ContentEvent>("The content item of the event."), Members = WorkflowValueMembers.ContentItem },
            new ActivityProvidedValue { Source = WorkflowValueSource.Input, Name = ContentEventConstants.ContentEventInputKey, TypeName = "object", Description = LocalizationSource.Create<ContentEvent>("The content event."), Members = WorkflowValueMembers.ContentEvent },
        ];
}

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class NotifyTask : TaskActivity<NotifyTask>, IActivityDescription
{
    private readonly INotifier _notifier;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;
    private readonly HtmlEncoder _htmlEncoder;

    public NotifyTask(
        INotifier notifier,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<NotifyTask> localizer,
        HtmlEncoder htmlEncoder)
    {
        _notifier = notifier;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
        _htmlEncoder = htmlEncoder;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<NotifyTask>("Notify Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.UI;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public NotifyType NotificationType
    {
        get => GetProperty<NotifyType>();
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Message
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var message = await _expressionEvaluator.EvaluateAsync(Message, workflowContext, _htmlEncoder);

        // The notification message can contain HTML by design
        await _notifier.AddAsync(NotificationType, new LocalizedHtmlString(nameof(NotifyTask), message));

        return Outcome("Done");
    }
}

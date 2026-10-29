using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Twitter.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Twitter.Workflows.Activities;

public class UpdateTwitterStatusTask : TaskActivity<UpdateTwitterStatusTask>, IActivityDescription
{
    private readonly TwitterClient _twitterClient;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public UpdateTwitterStatusTask(
        TwitterClient twitterClient,
        IWorkflowExpressionEvaluator expressionEvaluator,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<UpdateTwitterStatusTask> localizer
        )
    {
        _twitterClient = twitterClient;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UpdateTwitterStatusTask>("Update X (Twitter) Status Task");

    public static LocalizationSource CategoryName => TwitterLocalizationSources.Social;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    // The category to which this activity belongs. The activity picker groups activities by this category.
    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    // The message to display.
    public WorkflowExpression<string> StatusTemplate
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    // Returns the possible outcomes of this activity.
    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Failed"]);

    // This is the heart of the activity and actually performs the work to be done.
    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        // The twitter client encodes the status using FormUrlEncodedContent
        var status = await _expressionEvaluator.EvaluateAsync(StatusTemplate, workflowContext, null);

        var result = await _twitterClient.UpdateStatus(status);
        workflowContext.Properties.Add("TwitterResponse", await result.Content.ReadAsStringAsync());

        return result.IsSuccessStatusCode
            ? Outcome("Done")
            : Outcome("Failed");
    }
}

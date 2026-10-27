using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

public class LogTask : TaskActivity<LogTask>, IActivityDescription
{
    private readonly ILogger _logger;
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public LogTask(ILogger<LogTask> logger, IWorkflowExpressionEvaluator expressionEvaluator, IStringLocalizerFactory localizerFactory, IStringLocalizer<LogTask> localizer)
    {
        _logger = logger;
        _expressionEvaluator = expressionEvaluator;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<LogTask>("Log Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Primitives;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public LogLevel LogLevel
    {
        get => GetProperty(() => LogLevel.Information);
        set => SetProperty(value);
    }

    public WorkflowExpression<string> Text
    {
        get => GetProperty(() => new WorkflowExpression<string>());
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var text = await _expressionEvaluator.EvaluateAsync(Text, workflowContext, null);
        var logLevel = LogLevel;

        _logger.Log(logLevel, 0, text, null, (state, error) => state.ToString());

        return Outcome("Done");
    }
}

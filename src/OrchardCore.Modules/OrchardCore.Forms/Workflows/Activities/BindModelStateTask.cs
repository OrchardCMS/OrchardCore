using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Forms.Workflows.Activities;

public class BindModelStateTask : TaskActivity<BindModelStateTask>, IActivityDescription
{
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizerFactory _localizerFactory;
    protected readonly IStringLocalizer S;

    public BindModelStateTask(
        IHttpContextAccessor httpContextAccessor,
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizerFactory localizerFactory,
        IStringLocalizer<BindModelStateTask> localizer
    )
    {
        _updateModelAccessor = updateModelAccessor;
        _httpContextAccessor = httpContextAccessor;
        _localizerFactory = localizerFactory;
        S = localizer;
    }

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<BindModelStateTask>("Bind Model State Task");

    public static LocalizationSource CategoryName => ActivityCategoryLocalizationSources.Validation;

    public override LocalizedString DisplayText => _localizerFactory.Localize(DisplayName);

    public override LocalizedString Category => _localizerFactory.Localize(CategoryName);

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"]);

    public override ActivityExecutionResult Execute(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var updater = _updateModelAccessor.ModelUpdater
            ?? throw new InvalidOperationException("Cannot add model validation errors when there's no Updater present.");

        var httpContext = _httpContextAccessor.HttpContext;

        foreach (var item in httpContext.Request.Form)
        {
            // Avoid creating a new array for rawValue if there's only one value.
            object rawValue;
            if (item.Value.Count == 0)
            {
                rawValue = null;
            }
            else if (item.Value.Count == 1)
            {
                rawValue = item.Value[0];
            }
            else
            {
                rawValue = item.Value.ToArray();
            }

            updater.ModelState.SetModelValue(item.Key, rawValue, item.Value.ToString());
        }

        return Outcome("Done");
    }
}

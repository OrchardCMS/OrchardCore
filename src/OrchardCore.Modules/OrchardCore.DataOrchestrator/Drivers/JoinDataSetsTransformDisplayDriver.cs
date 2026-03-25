using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class JoinDataSetsTransformDisplayDriver : EtlActivityDisplayDriver<JoinDataSetsTransform, JoinDataSetsTransformViewModel>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEtlPipelineService _pipelineService;

    public JoinDataSetsTransformDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IEtlPipelineService pipelineService)
    {
        _httpContextAccessor = httpContextAccessor;
        _pipelineService = pipelineService;
    }

    protected override async ValueTask EditActivityAsync(JoinDataSetsTransform activity, JoinDataSetsTransformViewModel model)
    {
        if (long.TryParse(_httpContextAccessor.HttpContext?.Request.Query["pipelineId"], out var pipelineId))
        {
            var pipeline = await _pipelineService.GetByDocumentIdAsync(pipelineId);
            if (pipeline != null)
            {
                model.AvailableJoinSources = pipeline.Activities
                    .Where(x => string.Equals(x.Name, nameof(ContentItemSource), StringComparison.Ordinal) ||
                                string.Equals(x.Name, nameof(JsonSource), StringComparison.Ordinal) ||
                                string.Equals(x.Name, nameof(ExcelSource), StringComparison.Ordinal))
                    .Select(x => new SelectListItem
                    {
                        Value = x.ActivityId,
                        Text = $"{x.Name} ({x.ActivityId[..Math.Min(8, x.ActivityId.Length)]})",
                    })
                    .ToList();
            }
        }

        model.JoinSourceActivityId = activity.JoinSourceActivityId;
        model.LeftField = activity.LeftField;
        model.RightField = activity.RightField;
        model.JoinType = activity.JoinType;
        model.RightPrefix = activity.RightPrefix;
    }

    protected override void UpdateActivity(JoinDataSetsTransformViewModel model, JoinDataSetsTransform activity)
    {
        activity.JoinSourceActivityId = model.JoinSourceActivityId;
        activity.LeftField = model.LeftField;
        activity.RightField = model.RightField;
        activity.JoinType = model.JoinType;
        activity.RightPrefix = model.RightPrefix;
    }
}

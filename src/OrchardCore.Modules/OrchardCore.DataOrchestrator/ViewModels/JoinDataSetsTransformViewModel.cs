using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.DataOrchestrator.ViewModels;

public class JoinDataSetsTransformViewModel
{
    public string JoinSourceActivityId { get; set; }

    public string LeftField { get; set; }

    public string RightField { get; set; }

    public string JoinType { get; set; }

    public string RightPrefix { get; set; }

    public IList<SelectListItem> AvailableJoinSources { get; set; } = [];
}

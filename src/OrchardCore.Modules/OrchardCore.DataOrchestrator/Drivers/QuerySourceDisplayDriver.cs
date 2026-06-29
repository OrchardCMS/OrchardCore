using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class QuerySourceDisplayDriver : EtlActivityDisplayDriver<QuerySource, QuerySourceViewModel>
{
    protected override void EditActivity(QuerySource activity, QuerySourceViewModel model)
    {
        model.QueryName = activity.QueryName;
        model.ParametersJson = activity.ParametersJson;
    }

    protected override void UpdateActivity(QuerySourceViewModel model, QuerySource activity)
    {
        activity.QueryName = model.QueryName;
        activity.ParametersJson = model.ParametersJson;
    }
}

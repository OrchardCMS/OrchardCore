using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.Display;
using OrchardCore.DataOrchestrator.ViewModels;
using OrchardCore.Queries;

namespace OrchardCore.DataOrchestrator.Drivers;

public sealed class QuerySourceDisplayDriver : EtlActivityDisplayDriver<QuerySource, QuerySourceViewModel>
{
    private readonly IQueryManager _queryManager;

    public QuerySourceDisplayDriver(IQueryManager queryManager)
    {
        _queryManager = queryManager;
    }

    protected override async ValueTask EditActivityAsync(QuerySource activity, QuerySourceViewModel model)
    {
        model.AvailableQueries = (await _queryManager.ListQueriesAsync())
            .OrderBy(query => query.Name)
            .Select(query => new SelectListItem
            {
                Text = string.IsNullOrEmpty(query.Source) ? query.Name : $"{query.Name} ({query.Source})",
                Value = query.Name,
            })
            .ToList();
        model.QueryName = activity.QueryName;
        model.ParametersJson = activity.ParametersJson;
    }

    protected override void UpdateActivity(QuerySourceViewModel model, QuerySource activity)
    {
        activity.QueryName = model.QueryName;
        activity.ParametersJson = model.ParametersJson;
    }
}

using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Elasticsearch.ViewModels;

namespace OrchardCore.Elasticsearch.Drivers;

/// <summary>
/// Builds the editor of the Elasticsearch query runner, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class ElasticsearchQueryRunnerDisplayDriver : DisplayDriver<AdminQueryViewModel>
{
    internal readonly IStringLocalizer S;

    public ElasticsearchQueryRunnerDisplayDriver(IStringLocalizer<ElasticsearchQueryRunnerDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AdminQueryViewModel model, BuildEditorContext context)
    {
        return Initialize<AdminQueryViewModel>("ElasticsearchQueryRunner_Edit", m =>
        {
            m.Id = model.Id;
            m.DecodedQuery = model.DecodedQuery;
            m.Parameters = model.Parameters;
            m.Indexes = model.Indexes;
            m.MatchAllQuery = model.MatchAllQuery;
        }).Location(l => l.Zone("Content", "1").Card(S["Query"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AdminQueryViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Id, m => m.DecodedQuery, m => m.Parameters);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(AdminQueryViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}

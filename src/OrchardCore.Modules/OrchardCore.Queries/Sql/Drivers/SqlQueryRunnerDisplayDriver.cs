using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Queries.Sql.ViewModels;

namespace OrchardCore.Queries.Sql.Drivers;

/// <summary>
/// Builds the editor of the SQL query runner, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class SqlQueryRunnerDisplayDriver : DisplayDriver<AdminQueryViewModel>
{
    internal readonly IStringLocalizer S;

    public SqlQueryRunnerDisplayDriver(IStringLocalizer<SqlQueryRunnerDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AdminQueryViewModel model, BuildEditorContext context)
    {
        return Initialize<AdminQueryViewModel>("SqlQueryRunner_Edit", m =>
        {
            m.DecodedQuery = model.DecodedQuery;
            m.Parameters = model.Parameters;
        }).Location(l => l.Zone("Content", "1").Card(S["Query"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(AdminQueryViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.DecodedQuery, m => m.Parameters);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the scripts of the page select.
    protected override void BuildPrefix(AdminQueryViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}

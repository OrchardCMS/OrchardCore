using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Queries.ViewModels;

namespace OrchardCore.Queries.Drivers;

public sealed class QueryDisplayDriver : DisplayDriver<Query>
{
    private readonly IQueryManager _queryManager;

    internal readonly IStringLocalizer S;

    public QueryDisplayDriver(
        IQueryManager queryManager,
        IStringLocalizer<QueryDisplayDriver> stringLocalizer)
    {
        _queryManager = queryManager;
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(Query query, BuildDisplayContext context)
    {
        return CombineAsync(
            Dynamic("Query_Fields_SummaryAdmin", model =>
            {
                model.Name = query.Name;
                model.Source = query.Source;
                model.Schema = query.Schema;
                model.Query = query;
            }).Location("Content:1"),
            Dynamic("Query_Buttons_SummaryAdmin", model =>
            {
                model.Name = query.Name;
                model.Source = query.Source;
                model.Schema = query.Schema;
                model.Query = query;
            }).Location("Actions:5")
        );
    }

    public override Task<IDisplayResult> EditAsync(Query query, BuildEditorContext context)
    {
        return CombineAsync(
            Initialize<EditQueryViewModel>("Query_Fields_Edit", model =>
            {
                model.Name = query.Name;
                model.Source = query.Source;
                model.Schema = query.Schema;
                model.Query = query;
            }).Location("Content:1"),
            Initialize<EditQueryViewModel>("Query_Fields_Buttons", model =>
            {
                model.Name = query.Name;
                model.Source = query.Source;
                model.Schema = query.Schema;
                model.Query = query;
            }).Location("Actions:5")
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(Query model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Source,
            m => m.Schema);

        if (string.IsNullOrEmpty(model.Name))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["Name is required"]);
        }

        if (!string.IsNullOrEmpty(model.Schema) && !model.Schema.IsJson())
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Schema), S["Invalid schema JSON supplied."]);
        }
        var safeName = model.Name.ToSafeName();
        if (string.IsNullOrEmpty(safeName) || model.Name != safeName)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["Name contains illegal characters"]);
        }
        else
        {
            var existing = await _queryManager.GetQueryAsync(safeName);

            if (existing != null && existing != model)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Name), S["A query with the same name already exists"]);
            }
        }

        return await EditAsync(model, context);
    }
}

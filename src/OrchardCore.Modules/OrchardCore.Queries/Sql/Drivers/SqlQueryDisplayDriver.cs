using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries.Sql.Models;
using OrchardCore.Queries.Sql.ViewModels;

namespace OrchardCore.Queries.Sql.Drivers;

public sealed class SqlQueryDisplayDriver : DisplayDriver<Query>
{
    internal readonly IStringLocalizer S;

    public SqlQueryDisplayDriver(IStringLocalizer<SqlQueryDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Display(Query query, BuildDisplayContext context)
    {
        if (query.Source != SqlQuerySource.SourceName)
        {
            return null;
        }

        return Combine(
            Dynamic("SqlQuery_SummaryAdmin", model =>
            {
                model.Query = query;
            }).Location("Content:5"),
            Dynamic("SqlQuery_Buttons_SummaryAdmin", model =>
            {
                model.Query = query;
            }).Location("Actions:2")
        );
    }

    public override IDisplayResult Edit(Query query, BuildEditorContext context)
    {
        if (query.Source != SqlQuerySource.SourceName)
        {
            return null;
        }

        return Initialize<SqlQueryViewModel>("SqlQuery_Edit", async model =>
        {
            model.ReturnDocuments = query.ReturnContentItems;

            var metadata = query.As<SqlQueryMetadata>();
            model.Query = metadata.Template;

            // Extract query from the query string if we come from the main query editor.
            if (string.IsNullOrEmpty(metadata.Template))
            {
                await context.Updater.TryUpdateModelAsync(model, string.Empty, m => m.Query);
            }
        }).Location("Content:5");
    }

    public override async Task<IDisplayResult> UpdateAsync(Query query, UpdateEditorContext context)
    {
        if (query.Source != SqlQuerySource.SourceName)
        {
            return null;
        }

        var viewModel = new SqlQueryViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix,
            m => m.Query,
            m => m.ReturnDocuments);

        if (string.IsNullOrWhiteSpace(viewModel.Query))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Query), S["The query field is required"]);
        }

        query.ReturnContentItems = viewModel.ReturnDocuments;
        query.Put(new SqlQueryMetadata()
        {
            Template = viewModel.Query,
        });

        return Edit(query, context);
    }
}

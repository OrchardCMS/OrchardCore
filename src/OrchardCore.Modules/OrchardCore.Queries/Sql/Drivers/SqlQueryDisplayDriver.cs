using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Queries.Sql.Models;
using OrchardCore.Queries.Sql.ViewModels;

namespace OrchardCore.Queries.Sql.Drivers;

public sealed class SqlQueryDisplayDriver : DisplayDriver<Query>
{
    private readonly INotifier _notifier;
    private readonly SqlLiquidOutputExpressionDetector _outputExpressionDetector;

    internal readonly IStringLocalizer S;

    public SqlQueryDisplayDriver(
        INotifier notifier,
        SqlLiquidOutputExpressionDetector outputExpressionDetector,
        IStringLocalizer<SqlQueryDisplayDriver> stringLocalizer)
    {
        _notifier = notifier;
        _outputExpressionDetector = outputExpressionDetector;
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
            var template = string.Empty;

            if (query.TryGet<SqlQueryMetadata>(out var metadata))
            {
                template = metadata.Template;
            }

            model.Query = template;
            model.HasLiquidOutputExpressions = _outputExpressionDetector.ContainsOutputStatement(model.Query);

            // Extract query from the query string if we come from the main query editor.
            if (string.IsNullOrEmpty(template))
            {
                await context.Updater.TryUpdateModelAsync(model, string.Empty, m => m.Query);
                model.HasLiquidOutputExpressions = _outputExpressionDetector.ContainsOutputStatement(model.Query);
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

        viewModel.HasLiquidOutputExpressions = _outputExpressionDetector.ContainsOutputStatement(viewModel.Query);

        if (viewModel.HasLiquidOutputExpressions)
        {
            await _notifier.AddAsync(
                NotifyType.Warning,
                new LocalizedHtmlString(
                    nameof(SqlQueryDisplayDriver),
                    S["Potentially unsafe Liquid output expressions ('{{ ... }}') were detected in this SQL query. Avoid injecting user input with Liquid output and use SQL parameters instead."].Value));
        }

        query.ReturnContentItems = viewModel.ReturnDocuments;
        query.Put(new SqlQueryMetadata()
        {
            Template = viewModel.Query,
        });

        return Edit(query, context);
    }
}

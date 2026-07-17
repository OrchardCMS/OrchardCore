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

    public override async Task<IDisplayResult> EditAsync(Query query, BuildEditorContext context)
    {
        if (query.Source != SqlQuerySource.SourceName)
        {
            return null;
        }

        var template = string.Empty;
        if (query.TryGet<SqlQueryMetadata>(out var metadata))
        {
            template = metadata.Template;
        }

        // Extract query from the query string if we come from the main query editor.
        // Create model object here, to make sure that TryUpdateModelAsync work on a specific object type, not over a proxied one
        var viewModel = new SqlQueryViewModel();
        if (string.IsNullOrEmpty(template))
        {
            await context.Updater.TryUpdateModelAsync(viewModel, string.Empty, m => m.Query);
            template = viewModel.Query;
        }

        return Initialize<SqlQueryViewModel>("SqlQuery_Edit", model =>
        {
            model.ReturnDocuments = query.ReturnContentItems;
            model.Query = template;
            model.HasLiquidOutputExpressions = _outputExpressionDetector.ContainsOutputStatement(model.Query);
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

        return await EditAsync(query, context);
    }
}

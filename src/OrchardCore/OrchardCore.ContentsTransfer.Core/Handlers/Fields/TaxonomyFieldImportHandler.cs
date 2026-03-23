using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public sealed class TaxonomyFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    private readonly IStringLocalizer S;

    public TaxonomyFieldImportHandler(IStringLocalizer<TaxonomyFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context)
    {
        var prefix = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_";

        return
        [
            new ImportColumn()
            {
                Name = $"{prefix}{nameof(TaxonomyField.TaxonomyContentItemId)}",
                Description = S["The taxonomy content item id for {0}", context.ContentPartFieldDefinition.DisplayName()],
                Type = ImportColumnType.ExportOnly,
            },
            new ImportColumn()
            {
                Name = $"{prefix}{nameof(TaxonomyField.TermContentItemIds)}",
                Description = S["A comma-separated list of taxonomy term content item ids for {0}", context.ContentPartFieldDefinition.DisplayName()],
                Type = ImportColumnType.ExportOnly,
            },
        ];
    }

    public Task ImportAsync(ContentFieldImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentFieldExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentPart);
        ArgumentNullException.ThrowIfNull(context.Row);

        var field = context.ContentPart.Get<TaxonomyField>(context.ContentPartFieldDefinition.Name);

        if (field == null)
        {
            return Task.CompletedTask;
        }

        var columns = GetColumns(context).ToArray();
        context.Row[columns[0].Name] = field.TaxonomyContentItemId ?? string.Empty;
        context.Row[columns[1].Name] = field.TermContentItemIds?.Length > 0 ? string.Join(",", field.TermContentItemIds) : string.Empty;

        return Task.CompletedTask;
    }
}

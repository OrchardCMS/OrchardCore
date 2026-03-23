using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTransfer.Handlers.Fields;

public sealed class LinkFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    private readonly IStringLocalizer S;

    public LinkFieldImportHandler(IStringLocalizer<LinkFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public IReadOnlyCollection<ImportColumn> GetColumns(ImportContentFieldContext context)
    {
        var prefix = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_";
        var settings = context.ContentPartFieldDefinition.GetSettings<LinkFieldSettings>();

        return
        [
            new ImportColumn()
            {
                Name = $"{prefix}{nameof(LinkField.Url)}",
                Description = S["The URL value for {0}", context.ContentPartFieldDefinition.DisplayName()],
            },
            new ImportColumn()
            {
                Name = $"{prefix}{nameof(LinkField.Text)}",
                Description = S["The link text value for {0}", context.ContentPartFieldDefinition.DisplayName()],
                IsRequired = settings?.LinkTextMode == LinkTextMode.Required,
            },
            new ImportColumn()
            {
                Name = $"{prefix}{nameof(LinkField.Target)}",
                Description = S["The target value for {0}", context.ContentPartFieldDefinition.DisplayName()],
            },
        ];
    }

    public Task ImportAsync(ContentFieldImportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentPart);
        ArgumentNullException.ThrowIfNull(context.Columns);
        ArgumentNullException.ThrowIfNull(context.Row);

        var columns = GetColumns(context).ToArray();
        string url = null;
        string text = null;
        string target = null;

        foreach (DataColumn column in context.Columns)
        {
            if (Is(column.ColumnName, columns[0]))
            {
                url = context.Row[column]?.ToString();
            }
            else if (Is(column.ColumnName, columns[1]))
            {
                text = context.Row[column]?.ToString();
            }
            else if (Is(column.ColumnName, columns[2]))
            {
                target = context.Row[column]?.ToString();
            }
        }

        if (url is null && text is null && target is null)
        {
            return Task.CompletedTask;
        }

        context.ContentPart.Alter<LinkField>(context.ContentPartFieldDefinition.Name, field =>
        {
            field.Url = url?.Trim();
            field.Text = text?.Trim();
            field.Target = target?.Trim();
        });

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentFieldExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentPart);
        ArgumentNullException.ThrowIfNull(context.Row);

        var field = context.ContentPart.Get<LinkField>(context.ContentPartFieldDefinition.Name);

        if (field == null)
        {
            return Task.CompletedTask;
        }

        var columns = GetColumns(context).ToArray();
        context.Row[columns[0].Name] = field.Url ?? string.Empty;
        context.Row[columns[1].Name] = field.Text ?? string.Empty;
        context.Row[columns[2].Name] = field.Target ?? string.Empty;

        return Task.CompletedTask;
    }
}

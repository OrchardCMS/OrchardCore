using System.Data;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public sealed class YoutubeFieldImportHandler : ContentImportHandlerBase, IContentFieldImportHandler
{
    private readonly IStringLocalizer S;

    public YoutubeFieldImportHandler(IStringLocalizer<YoutubeFieldImportHandler> stringLocalizer)
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
                Name = $"{prefix}{nameof(YoutubeField.RawAddress)}",
                Description = S["The raw YouTube URL for {0}", context.ContentPartFieldDefinition.DisplayName()],
            },
            new ImportColumn()
            {
                Name = $"{prefix}{nameof(YoutubeField.EmbeddedAddress)}",
                Description = S["The embedded YouTube URL for {0}", context.ContentPartFieldDefinition.DisplayName()],
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
        string rawAddress = null;
        string embeddedAddress = null;

        foreach (DataColumn column in context.Columns)
        {
            if (Is(column.ColumnName, columns[0]))
            {
                rawAddress = context.Row[column]?.ToString();
            }
            else if (Is(column.ColumnName, columns[1]))
            {
                embeddedAddress = context.Row[column]?.ToString();
            }
        }

        if (rawAddress is null && embeddedAddress is null)
        {
            return Task.CompletedTask;
        }

        context.ContentPart.Alter<YoutubeField>(context.ContentPartFieldDefinition.Name, field =>
        {
            field.RawAddress = rawAddress?.Trim();
            field.EmbeddedAddress = embeddedAddress?.Trim();
        });

        return Task.CompletedTask;
    }

    public Task ExportAsync(ContentFieldExportMapContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(context.ContentPart);
        ArgumentNullException.ThrowIfNull(context.Row);

        var field = context.ContentPart.Get<YoutubeField>(context.ContentPartFieldDefinition.Name);

        if (field == null)
        {
            return Task.CompletedTask;
        }

        var columns = GetColumns(context).ToArray();
        context.Row[columns[0].Name] = field.RawAddress ?? string.Empty;
        context.Row[columns[1].Name] = field.EmbeddedAddress ?? string.Empty;

        return Task.CompletedTask;
    }
}

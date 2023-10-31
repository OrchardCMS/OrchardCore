using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public class TextFieldImportHandler : ContentFieldImportHandlerBase
{
    public TextFieldImportHandler(IStringLocalizer<TextFieldImportHandler> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    public override IReadOnlyCollection<ImportColumn> Columns(ImportContentFieldContext context)
    {
        return new[]
        {
            new ImportColumn()
            {
                Name = $"{context.PartName}_{context.ContentPartFieldDefinition.Name}_{nameof(TextField.Text)}",
                Description = Description(context),
                IsRequired = IsRequired(context),
                AdditionalNames = AdditionalValues(context),
            }
        };
    }

    public override async Task MapAsync(ContentFieldImportMapContext context)
    {
        if (context.ContentItem == null)
        {
            throw new ArgumentNullException(nameof(context.ContentItem));
        }

        if (context.Columns == null)
        {
            throw new ArgumentNullException(nameof(context.Columns));
        }

        if (context.Row == null)
        {
            throw new ArgumentNullException(nameof(context.Row));
        }

        var firstColumn = Columns(context).FirstOrDefault();

        foreach (DataColumn column in context.Columns)
        {
            if (!Is(column.ColumnName, firstColumn))
            {
                continue;
            }
            var text = context.Row[column]?.ToString();

            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            await SetValueAsync(context, text);
        }
    }

    public override async Task MapOutAsync(ContentFieldExportMapContext context)
    {
        if (context.ContentItem == null)
        {
            throw new ArgumentNullException(nameof(context.ContentItem));
        }


        if (context.Row == null)
        {
            throw new ArgumentNullException(nameof(context.Row));
        }

        var firstColumn = Columns(context).FirstOrDefault();

        if (firstColumn != null)
        {
            context.Row[firstColumn.Name] = await GetValueAsync(context);
        }
    }

    protected virtual Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        context.ContentPart.Alter<TextField>(context.ContentPartFieldDefinition.Name, (field) =>
        {
            field.Text = text?.Trim();
        });

        return Task.CompletedTask;
    }

    protected virtual Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<TextField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Text);
    }

    protected virtual string Description(ImportContentFieldContext context)
    {
        return S["A text value for {0}", context.ContentPartFieldDefinition.DisplayName()];
    }

    protected virtual bool IsRequired(ImportContentFieldContext context)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<TextFieldSettings>();

        return settings?.Required ?? false;
    }

    protected virtual string[] AdditionalValues(ImportContentFieldContext context)
    {
        var predefined = context.ContentPartFieldDefinition.GetSettings<TextFieldPredefinedListEditorSettings>();

        if (predefined == null)
        {
            var multiText = context.ContentPartFieldDefinition.GetSettings<MultiTextFieldSettings>();

            return multiText?.Options?.Select(x => x.Value)?.ToArray() ?? Array.Empty<string>();
        }

        return predefined?.Options?.Select(x => x.Value)?.ToArray() ?? Array.Empty<string>();
    }
}

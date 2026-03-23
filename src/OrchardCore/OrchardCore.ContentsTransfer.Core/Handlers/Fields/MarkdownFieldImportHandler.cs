using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Markdown.Fields;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public sealed class MarkdownFieldImportHandler : StandardFieldImportHandler
{
    protected readonly IStringLocalizer S;

    public MarkdownFieldImportHandler(IStringLocalizer<MarkdownFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override string BindingPropertyName
        => nameof(MarkdownField.Markdown);

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        context.ContentPart.Alter<MarkdownField>(context.ContentPartFieldDefinition.Name, field =>
        {
            field.Markdown = text;
        });

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<MarkdownField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Markdown);
    }

    protected override string Description(ImportContentFieldContext context)
        => S["A markdown value for {0}", context.ContentPartFieldDefinition.DisplayName()];
}

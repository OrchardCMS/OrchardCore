using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public sealed class HtmlFieldImportHandler : StandardFieldImportHandler
{
    protected readonly IStringLocalizer S;

    public HtmlFieldImportHandler(IStringLocalizer<HtmlFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override string BindingPropertyName
        => nameof(HtmlField.Html);

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        context.ContentPart.Alter<HtmlField>(context.ContentPartFieldDefinition.Name, field =>
        {
            field.Html = text;
        });

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<HtmlField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Html);
    }

    protected override string Description(ImportContentFieldContext context)
        => S["An HTML value for {0}", context.ContentPartFieldDefinition.DisplayName()];
}

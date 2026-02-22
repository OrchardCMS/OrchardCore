using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public class TextFieldImportHandler : StandardFieldImportHandler
{
    protected readonly IStringLocalizer S;

    public TextFieldImportHandler(IStringLocalizer<TextFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override string BindingPropertyName
        => nameof(TextField.Text);

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        context.ContentPart.Alter<TextField>(context.ContentPartFieldDefinition.Name, (field) =>
        {
            field.Text = text?.Trim();
        });

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<TextField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Text);
    }

    protected override string Description(ImportContentFieldContext context)
        => S["A text value for {0}", context.ContentPartFieldDefinition.DisplayName()];

    protected override bool IsRequired(ImportContentFieldContext context)
        => context.ContentPartFieldDefinition.GetSettings<TextFieldSettings>()?.Required ?? false;

    protected override string[] GetValidValues(ImportContentFieldContext context)
    {
        var predefined = context.ContentPartFieldDefinition.GetSettings<TextFieldPredefinedListEditorSettings>();

        if (predefined == null)
        {
            var multiText = context.ContentPartFieldDefinition.GetSettings<MultiTextFieldSettings>();

            return multiText?.Options?.Select(x => x.Value)?.ToArray() ?? [];
        }

        return predefined?.Options?.Select(x => x.Value)?.ToArray() ?? [];
    }
}

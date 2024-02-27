using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public class NumberFieldImportHandler : StandardFieldImportHandler
{
    private readonly IStringLocalizer S;

    public NumberFieldImportHandler(IStringLocalizer<NumberFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        var value = string.IsNullOrEmpty(text) ? context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>()?.DefaultValue : text?.Trim();

        if (decimal.TryParse(value, out var decimalValue))
        {
            context.ContentPart.Alter<NumericField>(context.ContentPartFieldDefinition.Name, (field) =>
            {
                field.Value = decimalValue;
            });
        }

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<NumericField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Value);
    }

    protected override string Description(ImportContentFieldContext context)
        => S["A numeric value for {0}", context.ContentPartFieldDefinition.DisplayName()];

    protected override bool IsRequired(ImportContentFieldContext context)
        => context.ContentPartFieldDefinition.GetSettings<NumericFieldSettings>()?.Required ?? false;

    protected override string BindingPropertyName
        => nameof(NumericField.Value);
}

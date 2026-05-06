using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTransfer.Handlers.Fields;

public sealed class MultiTextFieldImportHandler : StandardFieldImportHandler
{
    internal readonly IStringLocalizer S;

    public MultiTextFieldImportHandler(IStringLocalizer<MultiTextFieldImportHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override string BindingPropertyName
        => nameof(MultiTextField.Values);

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        var values = string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        context.ContentPart.Alter<MultiTextField>(context.ContentPartFieldDefinition.Name, field =>
        {
            field.Values = values;
        });

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<MultiTextField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Values == null ? null : string.Join("|", field.Values));
    }

    protected override string Description(ImportContentFieldContext context)
        => S["All values for {0}. Separate each value with bar (i.e., | )", context.ContentPartFieldDefinition.DisplayName()];

    protected override string[] GetValidValues(ImportContentFieldContext context)
        => context.ContentPartFieldDefinition.GetSettings<MultiTextFieldSettings>()?.Options?.Select(x => x.Value)?.ToArray() ?? [];
}

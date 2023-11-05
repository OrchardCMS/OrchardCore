using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentsTransfer.Handlers.Fields;

public class BooleanFieldImportHandler : StandardFieldImportHandler
{
    public BooleanFieldImportHandler(IStringLocalizer<BooleanFieldImportHandler> stringLocalizer)
        : base(stringLocalizer)
    {
    }

    protected override Task SetValueAsync(ContentFieldImportMapContext context, string text)
    {
        var value = context.ContentPartFieldDefinition.GetSettings<BooleanFieldSettings>()?.DefaultValue ?? false;

        if (!string.IsNullOrEmpty(text) && string.Equals(bool.TrueString, text.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            value = true;
        }

        context.ContentPart.Alter<BooleanField>(context.ContentPartFieldDefinition.Name, (field) =>
        {
            field.Value = value;
        });

        return Task.CompletedTask;
    }

    protected override Task<object> GetValueAsync(ContentFieldExportMapContext context)
    {
        var field = context.ContentPart.Get<BooleanField>(context.ContentPartFieldDefinition.Name);

        return Task.FromResult<object>(field?.Value);
    }

    protected override string Description(ImportContentFieldContext context)
     => S["A numeric value for {0}", context.ContentPartFieldDefinition.DisplayName()];


    protected override bool IsRequired(ImportContentFieldContext context)
        => false;

    protected override string BindingPropertyName => nameof(NumericField.Value);

    protected override string[] GetValidValues(ImportContentFieldContext context)
    {
        return new[]
        {
            bool.TrueString,
            bool.FalseString,
        };
    }
}

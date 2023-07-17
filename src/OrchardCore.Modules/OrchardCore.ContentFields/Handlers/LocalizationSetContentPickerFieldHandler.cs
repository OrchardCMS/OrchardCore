using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.Handlers;

public class LocalizationSetContentPickerFieldHandler : ContentFieldHandler<LocalizationSetContentPickerField>
{
    protected readonly IStringLocalizer S;

    public LocalizationSetContentPickerFieldHandler(IStringLocalizer<LocalizationSetContentPickerFieldHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, LocalizationSetContentPickerField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<LocalizationSetContentPickerFieldSettings>();

        if (settings.Required && field.LocalizationSets.Length == 0)
        {
            context.Fail(S["The {0} field is required.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.LocalizationSets));
        }

        if (!settings.Multiple && field.LocalizationSets.Length > 1)
        {
            context.Fail(S["The {0} field cannot contain multiple items.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.LocalizationSets));
        }

        return Task.CompletedTask;
    }
}

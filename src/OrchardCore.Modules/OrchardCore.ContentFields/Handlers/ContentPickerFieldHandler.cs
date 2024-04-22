using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.Handlers;

public class ContentPickerFieldHandler : ContentFieldHandler<ContentPickerField>
{
    protected readonly IStringLocalizer S;

    public ContentPickerFieldHandler(IStringLocalizer<ContentPickerFieldHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, ContentPickerField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<ContentPickerFieldSettings>();

        if (settings.Required && field.ContentItemIds.Length == 0)
        {
            context.Fail(S["The {0} field is required.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.ContentItemIds));
        }

        if (!settings.Multiple && field.ContentItemIds.Length > 1)
        {
            context.Fail(S["The {0} field cannot contain multiple items.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.ContentItemIds));
        }

        return Task.CompletedTask;
    }
}


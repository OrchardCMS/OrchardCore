using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.Handlers;

public class MultiTextFieldHandler : ContentFieldHandler<MultiTextField>
{
    protected readonly IStringLocalizer S;

    public MultiTextFieldHandler(IStringLocalizer<MultiTextFieldHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, MultiTextField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<MultiTextFieldSettings>();

        if (settings.Required && field.Values.Length == 0)
        {
            context.Fail(S["A value is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Values));
        }

        return Task.CompletedTask;
    }
}

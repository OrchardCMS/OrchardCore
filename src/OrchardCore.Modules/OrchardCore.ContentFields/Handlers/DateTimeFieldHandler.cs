using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.Handlers;

public class DateTimeFieldHandler : ContentFieldHandler<DateTimeField>
{
    protected readonly IStringLocalizer S;

    public DateTimeFieldHandler(IStringLocalizer<DateTimeFieldHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, DateTimeField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<DateTimeFieldSettings>();

        if (settings.Required && !field.Value.HasValue)
        {
            context.Fail(S["A value is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Value));
        }

        return Task.CompletedTask;
    }
}

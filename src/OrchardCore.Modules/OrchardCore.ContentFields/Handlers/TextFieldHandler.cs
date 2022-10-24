using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.Handlers;

public class TextFieldHandler : ContentFieldHandler<TextField>
{
    private readonly IStringLocalizer<TextFieldHandler> S;

    public TextFieldHandler(IStringLocalizer<TextFieldHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateFieldContentContext context, TextField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<TextFieldSettings>();

        if (settings.Required && String.IsNullOrEmpty(field.Text))
        {
            context.Fail(S["A value is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Text));
        }

        return Task.CompletedTask;
    }
}

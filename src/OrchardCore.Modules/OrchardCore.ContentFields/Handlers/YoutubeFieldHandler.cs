using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.Handlers;

public class YoutubeFieldHandler : ContentFieldHandler<YoutubeField>
{
    protected readonly IStringLocalizer S;

    public YoutubeFieldHandler(IStringLocalizer<YoutubeFieldHandler> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, YoutubeField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<YoutubeFieldSettings>();

        if (settings.Required && String.IsNullOrWhiteSpace(field.RawAddress))
        {
            context.Fail(S["A value is required for '{0}'.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.RawAddress));
        }

        if (field.RawAddress != null)
        {
            var uri = new Uri(field.RawAddress);

            // if it is a url with QueryString
            if (!String.IsNullOrWhiteSpace(uri.Query))
            {
                var query = QueryHelpers.ParseQuery(uri.Query);
                if (!query.ContainsKey("v"))
                {
                    context.Fail(S["The format of the url is invalid"], nameof(field.RawAddress));
                }
            }
        }

        return Task.CompletedTask;
    }
}

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Infrastructure.Html;

namespace OrchardCore.ContentFields.Handlers;

public class LinkFieldHandler : ContentFieldHandler<LinkField>
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    protected readonly IStringLocalizer S;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly HtmlEncoder _htmlencoder;

    public LinkFieldHandler(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IStringLocalizer<LinkFieldHandler> localizer,
        IHtmlSanitizerService htmlSanitizerService,
        HtmlEncoder htmlencoder)
    {
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        S = localizer;
        _htmlSanitizerService = htmlSanitizerService;
        _htmlencoder = htmlencoder;
    }

    public override Task ValidatingAsync(ValidateContentFieldContext context, LinkField field)
    {
        var settings = context.ContentPartFieldDefinition.GetSettings<LinkFieldSettings>();

        var urlToValidate = field.Url;
        if (!String.IsNullOrEmpty(urlToValidate))
        {
            var indexAnchor = urlToValidate.IndexOf('#');
            if (indexAnchor > -1)
            {
                urlToValidate = urlToValidate[..indexAnchor];
            }

            if (urlToValidate.StartsWith("~/", StringComparison.Ordinal))
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                urlToValidate = urlHelper.Content(urlToValidate);
            }

            urlToValidate = urlToValidate.ToUriComponents();
        }

        // Validate Url
        if (settings.Required && String.IsNullOrWhiteSpace(field.Url))
        {
            context.Fail(S["The url is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Url));
        }
        else if (!String.IsNullOrWhiteSpace(field.Url))
        {
            if (!Uri.IsWellFormedUriString(urlToValidate, UriKind.RelativeOrAbsolute))
            {
                context.Fail(S["{0} is an invalid url.", field.Url], nameof(field.Url));
            }
            else
            {
                var link = $"<a href=\"{_htmlencoder.Encode(urlToValidate)}\"></a>";

                if (!String.Equals(link, _htmlSanitizerService.Sanitize(link), StringComparison.OrdinalIgnoreCase))
                {
                    context.Fail(S["{0} is an invalid url.", field.Url], nameof(field.Url));
                }
            }
        }

        if (settings.LinkTextMode == LinkTextMode.Required && String.IsNullOrWhiteSpace(field.Text))
        {
            context.Fail(S["The link text is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Text));
        }
        else if (settings.LinkTextMode == LinkTextMode.Static && String.IsNullOrWhiteSpace(settings.DefaultText))
        {
            context.Fail(S["The text default value is required for {0}.", context.ContentPartFieldDefinition.DisplayName()], nameof(field.Text));
        }

        return Task.CompletedTask;
    }
}

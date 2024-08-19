using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.Settings;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers;

public sealed class HtmlMenuItemPartDisplayDriver : ContentPartDisplayDriver<HtmlMenuItemPart>
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly HtmlEncoder _htmlencoder;

    internal readonly IStringLocalizer S;

    public HtmlMenuItemPartDisplayDriver(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IStringLocalizer<HtmlMenuItemPartDisplayDriver> localizer,
        IHtmlSanitizerService htmlSanitizerService,
        HtmlEncoder htmlencoder
        )
    {
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _htmlSanitizerService = htmlSanitizerService;
        _htmlencoder = htmlencoder;
        S = localizer;
    }

    public override IDisplayResult Display(HtmlMenuItemPart part, BuildPartDisplayContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<HtmlMenuItemPartSettings>();

        if (settings.SanitizeHtml)
        {
            part.Html = _htmlSanitizerService.Sanitize(part.Html);
        }

        return Combine(
            Dynamic("HtmlMenuItemPart_Admin", shape =>
            {
                shape.MenuItemPart = part;
            })
            .Location("Admin", "Content:10"),
            Dynamic("HtmlMenuItemPart_Thumbnail", shape =>
            {
                shape.MenuItemPart = part;
            })
            .Location("Thumbnail", "Content:10")
        );
    }

    public override IDisplayResult Edit(HtmlMenuItemPart part, BuildPartEditorContext context)
    {
        return Initialize<HtmlMenuItemPartEditViewModel>("HtmlMenuItemPart_Edit", model =>
        {
            model.Name = part.ContentItem.DisplayText;
            model.Url = part.Url;
            model.Target = part.Target;
            model.Html = part.Html;
            model.MenuItemPart = part;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(HtmlMenuItemPart part, UpdatePartEditorContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<HtmlMenuItemPartSettings>();
        var model = new HtmlMenuItemPartEditViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.ContentItem.DisplayText = model.Name;
        part.Html = settings.SanitizeHtml ? _htmlSanitizerService.Sanitize(model.Html) : model.Html;
        part.Url = model.Url;
        part.Target = model.Target;

        var urlToValidate = part.Url;

        if (!string.IsNullOrEmpty(urlToValidate))
        {
            urlToValidate = urlToValidate.Split('#', 2)[0];

            if (urlToValidate.StartsWith("~/", StringComparison.Ordinal))
            {
                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                urlToValidate = urlHelper.Content(urlToValidate);
            }

            urlToValidate = urlToValidate.ToUriComponents();

            if (!Uri.IsWellFormedUriString(urlToValidate, UriKind.RelativeOrAbsolute))
            {
                context.Updater.ModelState.AddModelError(nameof(part.Url), S["{0} is an invalid url.", part.Url]);
            }
            else
            {
                var link = $"<a href=\"{_htmlencoder.Encode(urlToValidate)}\"></a>";

                if (!string.Equals(link, _htmlSanitizerService.Sanitize(link), StringComparison.OrdinalIgnoreCase))
                {
                    context.Updater.ModelState.AddModelError(nameof(part.Url), S["{0} is an invalid url.", part.Url]);
                }
            }
        }

        return Edit(part, context);
    }
}

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers;

public sealed class LinkMenuItemPartDisplayDriver : ContentPartDisplayDriver<LinkMenuItemPart>
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly HtmlEncoder _htmlEncoder;

    internal readonly IStringLocalizer S;

    public LinkMenuItemPartDisplayDriver(
        IUrlHelperFactory urlHelperFactory,
        IActionContextAccessor actionContextAccessor,
        IStringLocalizer<LinkMenuItemPartDisplayDriver> localizer,
        IHtmlSanitizerService htmlSanitizerService,
        HtmlEncoder htmlEncoder
        )
    {
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
        _htmlSanitizerService = htmlSanitizerService;
        _htmlEncoder = htmlEncoder;
        S = localizer;
    }

    public override IDisplayResult Display(LinkMenuItemPart part, BuildPartDisplayContext context)
    {
        return Combine(
            Dynamic("LinkMenuItemPart_Admin", shape =>
            {
                shape.MenuItemPart = part;
            })
            .Location("Admin", "Content:10"),
            Dynamic("LinkMenuItemPart_Thumbnail", shape =>
            {
                shape.MenuItemPart = part;
            })
            .Location("Thumbnail", "Content:10")
        );
    }

    public override IDisplayResult Edit(LinkMenuItemPart part, BuildPartEditorContext context)
    {
        return Initialize<LinkMenuItemPartEditViewModel>("LinkMenuItemPart_Edit", model =>
        {
            model.Name = part.ContentItem.DisplayText;
            model.Url = part.Url;
            model.Target = part.Target;
            model.MenuItemPart = part;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(LinkMenuItemPart part, UpdatePartEditorContext context)
    {
        var model = new LinkMenuItemPartEditViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.Url = model.Url;
        part.Target = model.Target;
        part.ContentItem.DisplayText = model.Name;

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
                var link = $"<a href=\"{_htmlEncoder.Encode(urlToValidate)}\"></a>";

                if (!string.Equals(link, _htmlSanitizerService.Sanitize(link), StringComparison.OrdinalIgnoreCase))
                {
                    context.Updater.ModelState.AddModelError(nameof(part.Url), S["{0} is an invalid url.", part.Url]);
                }
            }
        }

        return Edit(part, context);
    }
}

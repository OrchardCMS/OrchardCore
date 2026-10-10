using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;

namespace OrchardCore.Menu.Drivers;

public sealed class HtmlMenuItemPartDisplayDriver : ContentPartDisplayDriver<HtmlMenuItemPart>
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHtmlSanitizerService _htmlSanitizerService;

    internal readonly IStringLocalizer S;

    public HtmlMenuItemPartDisplayDriver(
        IUrlHelperFactory urlHelperFactory,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<HtmlMenuItemPartDisplayDriver> localizer,
        IHtmlSanitizerService htmlSanitizerService)
    {
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
        _htmlSanitizerService = htmlSanitizerService;
        S = localizer;
    }

    public override IDisplayResult Display(HtmlMenuItemPart part, BuildPartDisplayContext context)
    {
        return Combine(
            Dynamic("HtmlMenuItemPart_Admin", static (shape, part) =>
            {
                shape.MenuItemPart = part;
            }, part)
            .Location("Admin", "Content:10"),
            Dynamic("HtmlMenuItemPart_Thumbnail", static (shape, part) =>
            {
                shape.MenuItemPart = part;
            }, part)
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
        var model = new HtmlMenuItemPartEditViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        part.ContentItem.DisplayText = model.Name;
        part.Html = model.Html;
        part.Url = model.Url;
        part.Target = model.Target;

        var urlToValidate = part.Url;

        if (!string.IsNullOrEmpty(urlToValidate))
        {
            urlToValidate = urlToValidate.Split('#', 2)[0];

            if (urlToValidate.StartsWith("~/", StringComparison.Ordinal))
            {
                // In .NET 10, create ActionContext directly instead of using obsolete IActionContextAccessor
                var httpContext = _httpContextAccessor.HttpContext;
                var actionContext = await httpContext.GetActionContextAsync();
                var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
                urlToValidate = urlHelper.Content(urlToValidate);
            }

            if (!MenuShapes.IsSafeUrl(urlToValidate, _htmlSanitizerService))
            {
                context.Updater.ModelState.AddModelError(nameof(part.Url), S["{0} is an invalid url.", part.Url]);
            }
        }

        return Edit(part, context);
    }
}

using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.Settings;
using OrchardCore.Html.ViewModels;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Html.Drivers;

public sealed class HtmlBodyPartDisplayDriver : ContentPartDisplayDriver<HtmlBodyPart>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IHtmlSanitizerService _htmlSanitizerService;
    private readonly HtmlEncoder _htmlEncoder;
    private readonly IShortcodeService _shortcodeService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public HtmlBodyPartDisplayDriver(ILiquidTemplateManager liquidTemplateManager,
        IHtmlSanitizerService htmlSanitizerService,
        HtmlEncoder htmlEncoder,
        IShortcodeService shortcodeService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<HtmlBodyPartDisplayDriver> localizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _htmlSanitizerService = htmlSanitizerService;
        _htmlEncoder = htmlEncoder;
        _shortcodeService = shortcodeService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = localizer;
    }

    public override IDisplayResult Display(HtmlBodyPart HtmlBodyPart, BuildPartDisplayContext context)
    {
        return Initialize<HtmlBodyPartViewModel>(GetDisplayShapeType(context), m => BuildViewModelAsync(m, HtmlBodyPart, context))
            .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
            .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }

    public override async Task<IDisplayResult> EditAsync(HtmlBodyPart HtmlBodyPart, BuildPartEditorContext context)
    {
        var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

        if (settings.RenderLiquid && !await CanManageLiquidTemplatesAsync())
        {
            return null;
        }

        return Initialize<HtmlBodyPartViewModel>(GetEditorShapeType(context), model =>
        {
            model.Html = HtmlBodyPart.Html;
            model.ContentItem = HtmlBodyPart.ContentItem;
            model.HtmlBodyPart = HtmlBodyPart;
            model.TypePartDefinition = context.TypePartDefinition;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(HtmlBodyPart model, UpdatePartEditorContext context)
    {
        var viewModel = new HtmlBodyPartViewModel();
        var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

        if (settings.RenderLiquid && !await CanManageLiquidTemplatesAsync())
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                S["You do not have permission to edit Liquid templates."]);

            return null;
        }

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Html);

        model.Html = viewModel.Html;

        if (settings.RenderLiquid
            && !string.IsNullOrEmpty(model.Html)
            && !_liquidTemplateManager.Validate(model.Html, out var errors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Html),
                S["{0} contains invalid Liquid expression: {1}",
                    context.TypePartDefinition.DisplayName(),
                    string.Join(" ", errors)]);
        }

        return await EditAsync(model, context);
    }

    private async ValueTask BuildViewModelAsync(HtmlBodyPartViewModel model, HtmlBodyPart htmlBodyPart, BuildPartDisplayContext context)
    {
        model.Html = htmlBodyPart.Html;
        model.HtmlBodyPart = htmlBodyPart;
        model.ContentItem = htmlBodyPart.ContentItem;

        var settings = context.TypePartDefinition.GetSettings<HtmlBodyPartSettings>();

        if (settings.RenderLiquid)
        {
            model.Html = await _liquidTemplateManager.RenderStringAsync(htmlBodyPart.Html, _htmlEncoder, model,
                new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
        }

        model.Html = await _shortcodeService.ProcessAsync(model.Html,
            new Context
            {
                ["ContentItem"] = htmlBodyPart.ContentItem,
                ["TypePartDefinition"] = context.TypePartDefinition,
            });

        if (settings.SanitizeHtml)
        {
            model.Html = _htmlSanitizerService.Sanitize(model.Html);
        }
    }

    private Task<bool> CanManageLiquidTemplatesAsync() =>
        _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid.Models;
using OrchardCore.Liquid.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Liquid.Drivers;

public sealed class LiquidPartDisplayDriver : ContentPartDisplayDriver<LiquidPart>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public LiquidPartDisplayDriver(
        ILiquidTemplateManager liquidTemplateManager,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<LiquidPartDisplayDriver> localizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = localizer;
    }

    public override Task<IDisplayResult> DisplayAsync(LiquidPart liquidPart, BuildPartDisplayContext context)
    {
        return CombineAsync(
            Initialize<LiquidPartViewModel>("LiquidPart", m => BuildViewModel(m, liquidPart))
                .Location(OrchardCoreConstants.DisplayType.Detail, "Content"),
            Initialize<LiquidPartViewModel>("LiquidPart_Summary", m => BuildViewModel(m, liquidPart))
                .Location(OrchardCoreConstants.DisplayType.Summary, "Content")
        );
    }

    public override async Task<IDisplayResult> EditAsync(LiquidPart liquidPart, BuildPartEditorContext context)
    {
        if (!await CanManageLiquidTemplatesAsync())
        {
            return null;
        }

        return Initialize<LiquidPartViewModel>("LiquidPart_Edit", m => BuildViewModel(m, liquidPart));
    }

    public override async Task<IDisplayResult> UpdateAsync(LiquidPart model, UpdatePartEditorContext context)
    {
        if (!await CanManageLiquidTemplatesAsync())
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                S["You do not have permission to edit Liquid templates."]);

            return null;
        }

        var viewModel = new LiquidPartViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Liquid);

        if (!string.IsNullOrEmpty(viewModel.Liquid) && !_liquidTemplateManager.Validate(viewModel.Liquid, out var errors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.Liquid), S["The Liquid Body doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
        }
        else
        {
            model.Liquid = viewModel.Liquid;
        }

        return await EditAsync(model, context);
    }

    private Task<bool> CanManageLiquidTemplatesAsync() =>
        _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);

    private static void BuildViewModel(LiquidPartViewModel model, LiquidPart liquidPart)
    {
        model.Liquid = liquidPart.Liquid;
        model.LiquidPart = liquidPart;
        model.ContentItem = liquidPart.ContentItem;
    }
}

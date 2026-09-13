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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public LiquidPartDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<LiquidPartDisplayDriver> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
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

    public override IDisplayResult Edit(LiquidPart liquidPart, BuildPartEditorContext context)
    {
        return Initialize<LiquidPartViewModel>("LiquidPart_Edit", m => BuildViewModel(m, liquidPart))
            .RenderWhen(static driver => driver.IsAuthorizedAsync(), this);
    }

    public override async Task<IDisplayResult> UpdateAsync(LiquidPart model, UpdatePartEditorContext context)
    {
        if (!await IsAuthorizedAsync())
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                nameof(model.Liquid),
                S["You do not have permission to manage Liquid templates."]);

            return Edit(model, context);
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

        return Edit(model, context);
    }

    private Task<bool> IsAuthorizedAsync()
        => _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);

    private static void BuildViewModel(LiquidPartViewModel model, LiquidPart liquidPart)
    {
        model.Liquid = liquidPart.Liquid;
        model.LiquidPart = liquidPart;
        model.ContentItem = liquidPart.ContentItem;
    }
}

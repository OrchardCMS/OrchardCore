using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid.Fields;
using OrchardCore.Liquid.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Liquid.Drivers;

public sealed class LiquidFieldDisplayDriver : ContentFieldDisplayDriver<LiquidField>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public LiquidFieldDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<LiquidFieldDisplayDriver> localizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _liquidTemplateManager = liquidTemplateManager;
        S = localizer;
    }

    public override IDisplayResult Display(LiquidField field, BuildFieldDisplayContext context)
    {
        return Initialize<LiquidFieldViewModel>(GetDisplayShapeType(context), model =>
        {
            model.Liquid = field.Liquid;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
            model.ContentItem = field.ContentItem;
        })
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content")
        .Location(OrchardCoreConstants.DisplayType.Summary, "Content");
    }

    public override IDisplayResult Edit(LiquidField field, BuildFieldEditorContext context)
    {
        return Initialize<LiquidFieldViewModel>(GetEditorShapeType(context), model =>
        {
            model.Liquid = field.Liquid;
            model.Field = field;
            model.Part = context.ContentPart;
            model.PartFieldDefinition = context.PartFieldDefinition;
            model.ContentItem = field.ContentItem;
        })
        .RenderWhen(static driver => driver.IsAuthorizedAsync(), this);
    }

    public override async Task<IDisplayResult> UpdateAsync(LiquidField field, UpdateFieldEditorContext context)
    {
        if (!await IsAuthorizedAsync())
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                nameof(field.Liquid),
                S["You do not have permission to manage Liquid templates."]);

            return Edit(field, context);
        }

        var viewModel = new LiquidFieldViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, model => model.Liquid);

        if (!string.IsNullOrEmpty(viewModel.Liquid) &&
            !_liquidTemplateManager.Validate(viewModel.Liquid, out var errors))
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                nameof(viewModel.Liquid),
                S["The Liquid field doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
        }
        else
        {
            field.Liquid = viewModel.Liquid;
        }

        return Edit(field, context);
    }

    private Task<bool> IsAuthorizedAsync()
        => _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);
}

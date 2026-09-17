using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Html.Models;
using OrchardCore.Html.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Html.Settings;

public sealed class HtmlBodyPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver<HtmlBodyPart>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public HtmlBodyPartSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<HtmlBodyPartSettingsDisplayDriver> localizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = localizer;
    }

    public override async Task<IDisplayResult> EditAsync(ContentTypePartDefinition contentTypePartDefinition, BuildEditorContext context)
    {
        var canManageLiquidTemplates = await CanManageLiquidTemplatesAsync();

        return Initialize<HtmlBodyPartSettingsViewModel>("HtmlBodyPartSettings_Edit", model =>
        {
            var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();

            model.SanitizeHtml = settings.SanitizeHtml;
            model.RenderLiquid = settings.RenderLiquid;
            model.CanManageLiquidTemplates = canManageLiquidTemplates;
        }).Location("Content:20");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
    {
        var model = new HtmlBodyPartSettingsViewModel();
        var settings = contentTypePartDefinition.GetSettings<HtmlBodyPartSettings>();
        var canManageLiquidTemplates = await CanManageLiquidTemplatesAsync();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.SanitizeHtml);

        settings.SanitizeHtml = model.SanitizeHtml;

        if (canManageLiquidTemplates)
        {
            await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.RenderLiquid);
            settings.RenderLiquid = model.RenderLiquid;
        }
        else if (HasPostedRenderLiquid())
        {
            context.Updater.ModelState.AddModelError(
                Prefix,
                S["You do not have permission to enable Liquid templates."]);
        }

        context.Builder.WithSettings(settings);

        return await EditAsync(contentTypePartDefinition, context);
    }

    private Task<bool> CanManageLiquidTemplatesAsync() =>
        _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);

    private bool HasPostedRenderLiquid()
    {
        var fieldName = string.IsNullOrEmpty(Prefix)
            ? nameof(HtmlBodyPartSettingsViewModel.RenderLiquid)
            : $"{Prefix}.{nameof(HtmlBodyPartSettingsViewModel.RenderLiquid)}";

        return _httpContextAccessor.HttpContext?.Request.HasFormContentType == true &&
            _httpContextAccessor.HttpContext.Request.Form.ContainsKey(fieldName);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Markdown.Fields;
using OrchardCore.Markdown.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Markdown.Settings;

public sealed class MarkdownFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MarkdownField>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    internal readonly IStringLocalizer S;

    public MarkdownFieldSettingsDriver(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<MarkdownFieldSettingsDriver> localizer)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        S = localizer;
    }

    public override async Task<IDisplayResult> EditAsync(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        var canManageLiquidTemplates = await CanManageLiquidTemplatesAsync();

        return Initialize<MarkdownFieldSettingsViewModel>("MarkdownFieldSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MarkdownFieldSettings>();

            model.SanitizeHtml = settings.SanitizeHtml;
            model.RenderLiquid = settings.RenderLiquid;
            model.Hint = settings.Hint;
            model.CanManageLiquidTemplates = canManageLiquidTemplates;
        }).Location("Content:20");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        var model = new MarkdownFieldSettingsViewModel();
        var settings = partFieldDefinition.GetSettings<MarkdownFieldSettings>();
        var canManageLiquidTemplates = await CanManageLiquidTemplatesAsync();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.SanitizeHtml, m => m.Hint);

        settings.SanitizeHtml = model.SanitizeHtml;
        settings.Hint = model.Hint;

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

        return await EditAsync(partFieldDefinition, context);
    }

    private Task<bool> CanManageLiquidTemplatesAsync() =>
        _authorizationService.AuthorizeAsync(
            _httpContextAccessor.HttpContext?.User,
            Permissions.ManageLiquidTemplates);

    private bool HasPostedRenderLiquid()
    {
        var fieldName = string.IsNullOrEmpty(Prefix)
            ? nameof(MarkdownFieldSettingsViewModel.RenderLiquid)
            : $"{Prefix}.{nameof(MarkdownFieldSettingsViewModel.RenderLiquid)}";

        return _httpContextAccessor.HttpContext?.Request.HasFormContentType == true &&
            _httpContextAccessor.HttpContext.Request.Form.ContainsKey(fieldName);
    }
}

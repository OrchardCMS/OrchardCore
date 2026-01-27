using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Smtp.Secrets.ViewModels;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Secrets.Drivers;

public sealed class SmtpSecretSettingsDisplayDriver : SiteDisplayDriver<SmtpSecretSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISecretManager _secretManager;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public SmtpSecretSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        ISecretManager secretManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _secretManager = secretManager;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, SmtpSecretSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        var secretInfos = await _secretManager.GetSecretInfosAsync();

        // Filter to only TextSecret types which can contain passwords
        var textSecrets = secretInfos
            .Where(s => s.Type == nameof(TextSecret))
            .Select(s => s.Name)
            .OrderBy(n => n);

        return Initialize<SmtpSecretSettingsViewModel>("SmtpSecretSettings_Edit", model =>
        {
            model.PasswordSecretName = settings.PasswordSecretName;
            model.AvailableSecrets = textSecrets;
        }).Location("Content:5.1#SMTP")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SmtpSecretSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new SmtpSecretSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.PasswordSecretName = model.PasswordSecretName;

        return await EditAsync(site, settings, context);
    }
}

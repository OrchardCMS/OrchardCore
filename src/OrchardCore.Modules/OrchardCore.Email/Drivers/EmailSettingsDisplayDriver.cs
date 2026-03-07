using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
using OrchardCore.Email.ViewModels;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers;

public sealed class EmailSettingsDisplayDriver : SiteDisplayDriver<EmailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IEnumerable<IEmailProvider> _emailProviders;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public EmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellReleaseManager shellReleaseManager,
        IEnumerable<IEmailProvider> emailProviders,
        IStringLocalizer<EmailSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellReleaseManager = shellReleaseManager;
        _emailProviders = emailProviders;
        S = stringLocalizer;
    }
    public override async Task<IDisplayResult> EditAsync(ISite site, EmailSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        var providers = _emailProviders.Select(p => new SelectListItem(p.DisplayName, p.Name));

        return Initialize<EmailSettingsViewModel>("EmailSettings_Edit", async model =>
        {
            model.DefaultProvider = settings.DefaultProviderName;
            model.Providers = providers.ToList();
        }).Location("Content:1#Providers")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, EmailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new EmailSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (settings.DefaultProviderName != model.DefaultProvider)
        {
            settings.DefaultProviderName = model.DefaultProvider;

            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }
}

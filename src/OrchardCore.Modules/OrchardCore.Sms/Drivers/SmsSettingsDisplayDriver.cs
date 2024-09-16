using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

public sealed class SmsSettingsDisplayDriver : SiteDisplayDriver<SmsSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal IStringLocalizer S;

    private readonly SmsProviderOptions _smsProviderOptions;

    protected override string SettingsGroupId
        => SmsSettings.GroupId;

    public SmsSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<SmsProviderOptions> smsProviders,
        IStringLocalizer<SmsSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _smsProviderOptions = smsProviders.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, SmsSettings settings, BuildEditorContext context)
        => Initialize<SmsSettingsViewModel>("SmsSettings_Edit", model =>
        {
            model.DefaultProvider = settings.DefaultProviderName;
            model.Providers = _smsProviderOptions.Providers
            .Where(entry => entry.Value.IsEnabled)
            .Select(entry => new SelectListItem(entry.Key, entry.Key))
            .OrderBy(item => item.Text)
            .ToArray();

        }).Location("Content:1#Providers")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings))
        .OnGroup(SettingsGroupId);

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SmsSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new SmsSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        if (settings.DefaultProviderName != model.DefaultProvider)
        {
            settings.DefaultProviderName = model.DefaultProvider;

            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }
}

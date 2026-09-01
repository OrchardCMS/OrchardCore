using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

public sealed class SmsSettingsDisplayDriver : SiteDisplayDriver<SmsSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IStringLocalizer S;

    private readonly IOptionsMonitor<SmsProviderOptions> _smsProviderOptions;

    protected override string SettingsGroupId
        => SmsSettings.GroupId;

    public SmsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsMonitor<SmsProviderOptions> smsProviders,
        IStringLocalizer<SmsSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _smsProviderOptions = smsProviders;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, SmsSettings settings, BuildEditorContext context)
        => Initialize<SmsSettingsViewModel>("SmsSettings_Edit", model =>
        {
            model.DefaultProvider = settings.DefaultProviderName;
            model.Providers = _smsProviderOptions.CurrentValue.Providers
            .Where(entry => entry.Value.IsEnabled)
            .Select(entry => new SelectListItem(entry.Key, entry.Key))
            .OrderBy(item => item.Text)
            .ToArray();

        }).Location("Content:1#Providers")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings), this)
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
        }

        return Edit(site, settings, context);
    }
}

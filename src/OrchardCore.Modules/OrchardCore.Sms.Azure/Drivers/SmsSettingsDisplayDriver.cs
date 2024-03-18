using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.ViewModels;

namespace OrchardCore.Sms.Azure.Drivers;

public class SmsSettingsDisplayDriver : SectionDisplayDriver<ISite, SmsSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;

    protected IStringLocalizer S;

    private readonly SmsProviderOptions _smsProviderOptions;

    public SmsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        IOptions<SmsProviderOptions> smsProviders,
        ShellSettings shellSettings,
        IStringLocalizer<SmsSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _smsProviderOptions = smsProviders.Value;
        _shellSettings = shellSettings;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(SmsSettings settings)
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
        .OnGroup(SmsSettings.GroupId);

    public override async Task<IDisplayResult> UpdateAsync(SmsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(SmsSettings.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new SmsSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            if (string.IsNullOrEmpty(model.DefaultProvider))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultProvider), S["You must select a default provider."]);
            }
            else
            {
                if (settings.DefaultProviderName != model.DefaultProvider)
                {
                    settings.DefaultProviderName = model.DefaultProvider;

                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
        }

        return Edit(settings);
    }
}

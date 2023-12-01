using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

public class SmsSettingsDisplayDriver : SectionDisplayDriver<ISite, SmsSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IDictionary<string, ISmsProvider> _smsProviders;

    public SmsSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        IDictionary<string, ISmsProvider> smsProviders,
        ShellSettings shellSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _smsProviders = smsProviders;
        _shellSettings = shellSettings;
    }

    public override async Task<IDisplayResult> EditAsync(SmsSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        return Initialize<SmsSettingsViewModel>("SmsSettings_Edit", model =>
        {
            model.DefaultProvider = settings.DefaultProviderName;
            model.Providers = GetProviders();
        }).Location("Content:1#Providers")
        .OnGroup(SmsSettings.GroupId);
    }

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
            if (settings.DefaultProviderName != model.DefaultProvider)
            {
                settings.DefaultProviderName = model.DefaultProvider;

                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }

        return await EditAsync(settings, context);
    }

    private SelectListItem[] _providers;

    private SelectListItem[] GetProviders()
    {
        if (_providers == null)
        {
            var items = new List<SelectListItem>();

            foreach (var provider in _smsProviders)
            {
                items.Add(new SelectListItem(provider.Value.Name, provider.Key));
            }

            _providers =
            [
                .. items.OrderBy(item => item.Text),
            ];
        }

        return _providers;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Core;
using OrchardCore.Email.Core.Services;
using OrchardCore.Email.ViewModels;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers;

public class EmailSettingsDisplayDriver : SectionDisplayDriver<ISite, EmailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly EmailOptions _emailOptions;
    private readonly IEmailProviderResolver _emailProviderResolver;
    private readonly ShellSettings _shellSettings;
    private readonly EmailProviderOptions _emailProviders;

    protected readonly IStringLocalizer S;

    public EmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        IOptions<EmailProviderOptions> emailProviders,
        IOptions<EmailOptions> emailOptions,
        IEmailProviderResolver emailProviderResolver,
        ShellSettings shellSettings,
        IStringLocalizer<EmailSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _emailOptions = emailOptions.Value;
        _emailProviderResolver = emailProviderResolver;
        _emailProviders = emailProviders.Value;
        _shellSettings = shellSettings;
        S = stringLocalizer;
    }
    public override async Task<IDisplayResult> EditAsync(EmailSettings settings, BuildEditorContext context)
    {
        if (!context.GroupId.EqualsOrdinalIgnoreCase(EmailSettings.GroupId))
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        context.Shape.Metadata.Wrappers.Add("Settings_Wrapper__Reload");

        return Initialize<EmailSettingsViewModel>("EmailSettings_Edit", async model =>
        {
            model.DefaultProvider = settings.DefaultProviderName ?? _emailOptions.DefaultProviderName;
            model.Providers = await GetProviderOptionsAsync();
        }).Location("Content:1#Providers")
        .OnGroup(EmailSettings.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(EmailSettings settings, BuildEditorContext context)
    {
        if (!context.GroupId.EqualsOrdinalIgnoreCase(EmailSettings.GroupId))
        {
            return null;
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new EmailSettingsViewModel();

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

    private async Task<SelectListItem[]> GetProviderOptionsAsync()
    {
        var options = new List<SelectListItem>();

        foreach (var entry in _emailProviders.Providers)
        {
            if (!entry.Value.IsEnabled)
            {
                continue;
            }

            var provider = await _emailProviderResolver.GetAsync(entry.Key);

            options.Add(new SelectListItem(provider.DisplayName, entry.Key));
        }

        return options.OrderBy(x => x.Text).ToArray();
    }
}

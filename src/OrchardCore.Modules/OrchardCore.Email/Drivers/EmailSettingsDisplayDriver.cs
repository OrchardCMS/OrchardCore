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
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers;

public sealed class EmailSettingsDisplayDriver : SiteDisplayDriver<EmailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly EmailOptions _emailOptions;
    private readonly IEmailProviderResolver _emailProviderResolver;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly EmailProviderOptions _emailProviders;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public EmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptions<EmailProviderOptions> emailProviders,
        IOptions<EmailOptions> emailOptions,
        IEmailProviderResolver emailProviderResolver,
        IShellReleaseManager shellReleaseManager,
        IStringLocalizer<EmailSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _emailOptions = emailOptions.Value;
        _emailProviderResolver = emailProviderResolver;
        _emailProviders = emailProviders.Value;
        _shellReleaseManager = shellReleaseManager;
        S = stringLocalizer;
    }
    public override async Task<IDisplayResult> EditAsync(ISite site, EmailSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        return Initialize<EmailSettingsViewModel>("EmailSettings_Edit", async model =>
        {
            model.DefaultProvider = settings.DefaultProviderName ?? _emailOptions.DefaultProviderName;
            model.Providers = await GetProviderOptionsAsync();
        }).Location("Content:1#Providers")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, EmailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
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

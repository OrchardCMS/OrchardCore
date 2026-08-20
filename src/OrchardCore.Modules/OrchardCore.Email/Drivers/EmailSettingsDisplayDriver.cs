using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
using OrchardCore.Email.ViewModels;
using OrchardCore.Environment.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers;

public sealed class EmailSettingsDisplayDriver : SiteDisplayDriver<EmailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOptionsMonitor<EmailOptions> _emailOptions;
    private readonly IEmailProviderResolver _emailProviderResolver;
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IOptionsMonitor<EmailProviderOptions> _emailProviders;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public EmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IOptionsMonitor<EmailProviderOptions> emailProviders,
        IOptionsMonitor<EmailOptions> emailOptions,
        IEmailProviderResolver emailProviderResolver,
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IStringLocalizer<EmailSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _emailOptions = emailOptions;
        _emailProviderResolver = emailProviderResolver;
        _emailProviders = emailProviders;
        _optionsUpdateNotifier = optionsUpdateNotifier;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, EmailSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        return Initialize<EmailSettingsViewModel>("EmailSettings_Edit", async model =>
        {
            model.DefaultProvider = settings.DefaultProviderName ?? _emailOptions.CurrentValue.DefaultProviderName;
            model.Providers = await GetProviderOptionsAsync();
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

            _optionsUpdateNotifier.RequestUpdate<EmailOptions>();
        }

        return await EditAsync(site, settings, context);
    }

    private async Task<SelectListItem[]> GetProviderOptionsAsync()
    {
        var options = new List<SelectListItem>();

        foreach (var entry in _emailProviders.CurrentValue.Providers)
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

using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

public sealed class FacebookPixelSettingsDisplayDriver : SiteDisplayDriver<FacebookPixelSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public FacebookPixelSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FacebookPixelSettingsDisplayDriver> logger
        ,
        IStringLocalizer<FacebookPixelSettingsDisplayDriver> stringLocalizer)
    {
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => FacebookConstants.PixelSettingsGroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, FacebookPixelSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        var conversionsApiAccessToken = string.Empty;
        var hasDecryptionError = false;

        if (!string.IsNullOrWhiteSpace(settings.ConversionsApiAccessToken))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.ConversionsApiProtectorName);
                conversionsApiAccessToken = protector.Unprotect(settings.ConversionsApiAccessToken);
            }
            catch (CryptographicException)
            {
                _logger.LogError("The Meta Conversions API access token could not be decrypted. It may have been encrypted using a different key.");
                hasDecryptionError = true;
            }
        }

        void PopulateViewModel(FacebookPixelSettingsViewModel model)
        {
            model.PixelId = settings.PixelId;
            model.ConversionsApiTestEventCode = settings.ConversionsApiTestEventCode;
            model.ConversionsApiAccessToken = conversionsApiAccessToken;
            model.HasDecryptionError = hasDecryptionError;
        }

        return Combine(
            Initialize<FacebookPixelSettingsViewModel>("FacebookPixelSettings_Edit", PopulateViewModel)
                .Location(l => l.Zone("Content", "0").Card(S["Pixel"], "10"))
                .OnGroup(SettingsGroupId),
            Initialize<FacebookPixelSettingsViewModel>("FacebookPixelSettings_Edit__ConversionsApi", PopulateViewModel)
                .Location(l => l.Zone("Content", "0.1").Card(S["Conversions API"], "20"))
                .OnGroup(SettingsGroupId)
        );
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, FacebookPixelSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageFacebookApp))
        {
            return null;
        }

        var model = new FacebookPixelSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.PixelId = model.PixelId?.Trim();
        settings.ConversionsApiTestEventCode = model.ConversionsApiTestEventCode?.Trim();

        if (context.Updater.ModelState.IsValid)
        {
            var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.ConversionsApiProtectorName);
            settings.ConversionsApiAccessToken = protector.Protect(model.ConversionsApiAccessToken ?? string.Empty);
        }

        return await EditAsync(site, settings, context);
    }
}

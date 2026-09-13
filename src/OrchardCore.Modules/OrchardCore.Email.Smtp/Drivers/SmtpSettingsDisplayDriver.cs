using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Email.Smtp.ViewModels;
using OrchardCore.Environment.Options;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Drivers;

public sealed class SmtpSettingsDisplayDriver : SiteDisplayDriver<SmtpSettings>
{
    [Obsolete("This property should no longer be used. Instead use EmailSettings.GroupId")]
    public const string GroupId = EmailSettings.GroupId;

    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptionsMonitor<SmtpOptions> _smtpOptions;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailAddressValidator _emailValidator;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public SmtpSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<SmtpOptions> options,
        IAuthorizationService authorizationService,
        IEmailAddressValidator emailAddressValidator,
        IStringLocalizer<SmtpSettingsDisplayDriver> stringLocalizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _smtpOptions = options;
        _authorizationService = authorizationService;
        _emailValidator = emailAddressValidator;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, SmtpSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        var smtpOptions = _smtpOptions.CurrentValue;

        return Initialize<SmtpSettingsViewModel>("SmtpSettings_Edit", model =>
        {
            // For backward compatibility with instances before the SMTP provider was factored out of
            // OrchardCore.Email, if IsEnabled is null, we check to see if there's already valid configuration.
            model.IsEnabled = settings.IsEnabled ?? smtpOptions.ConfigurationExists();
            model.DefaultSender = settings.DefaultSender;
            model.DeliveryMethod = settings.DeliveryMethod;
            model.PickupDirectoryLocation = string.IsNullOrWhiteSpace(settings.PickupDirectoryLocation)
                ? SmtpPickupDirectoryResolver.DefaultPickupDirectoryLocation
                : settings.PickupDirectoryLocation;
            model.Host = settings.Host;
            model.Port = settings.Port;
            model.ProxyHost = settings.ProxyHost;
            model.ProxyPort = settings.ProxyPort;
            model.EncryptionMethod = settings.EncryptionMethod;
            model.AutoSelectEncryption = settings.AutoSelectEncryption;
            model.RequireCredentials = settings.RequireCredentials;
            model.UseDefaultCredentials = settings.UseDefaultCredentials;
            model.UserName = settings.UserName;
            // Passwords are write-only; an empty input retains the stored credential.
            model.Password = null;
            model.IgnoreInvalidSslCertificate = settings.IgnoreInvalidSslCertificate;
        }).Location("Content:5#SMTP")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, SmtpSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new SmtpSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var editor = new SmtpSettingsEditor(_optionsUpdateNotifier, _dataProtectionProvider, _emailValidator, S);
        editor.Apply(site, settings, model,
            (name, message) => context.Updater.ModelState.AddModelError(Prefix, name, message),
            () => context.Updater.ModelState.IsValid);

        return await EditAsync(site, settings, context);
    }
}

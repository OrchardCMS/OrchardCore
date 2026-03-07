using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Email.Smtp.ViewModels;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp.Drivers;

public sealed class SmtpSettingsDisplayDriver : SiteDisplayDriver<SmtpSettings>
{
    [Obsolete("This property should no longer be used. Instead use EmailSettings.GroupId")]
    public const string GroupId = EmailSettings.GroupId;

    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailAddressValidator _emailValidator;
    private readonly EmailOptions _emailOptions;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public SmtpSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IEmailAddressValidator emailAddressValidator,
        IOptions<EmailOptions> emailOptions,
        IStringLocalizer<SmtpSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _emailValidator = emailAddressValidator;
        _emailOptions = emailOptions.Value;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, SmtpSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        return Initialize<SmtpSettingsViewModel>("SmtpSettings_Edit", model =>
        {
            // For backward compatibility with instances before the SMTP provider was factored out of
            // OrchardCore.Email, if IsEnabled is null, we check to see if there's already valid configuration.
            model.DefaultSender = settings.DefaultSender;
            model.DeliveryMethod = settings.DeliveryMethod;
            model.PickupDirectoryLocation = settings.PickupDirectoryLocation;
            model.Host = settings.Host;
            model.Port = settings.Port;
            model.ProxyHost = settings.ProxyHost;
            model.ProxyPort = settings.ProxyPort;
            model.EncryptionMethod = settings.EncryptionMethod;
            model.AutoSelectEncryption = settings.AutoSelectEncryption;
            model.RequireCredentials = settings.RequireCredentials;
            model.UseDefaultCredentials = settings.UseDefaultCredentials;
            model.UserName = settings.UserName;
            model.Password = settings.Password;
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

        if (_emailOptions.DefaultProviderName != "SMTP")
        {
            var model = new SmtpSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (string.IsNullOrEmpty(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is a required field."]);
            }

            if (!_emailValidator.Validate(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is invalid."]);
            }

            if (model.DeliveryMethod == SmtpDeliveryMethod.Network && string.IsNullOrWhiteSpace(model.Host))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Host), S["The {0} field is required.", "Host name"]);
            }

            if (model.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
                && string.IsNullOrWhiteSpace(model.PickupDirectoryLocation))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PickupDirectoryLocation), S["The {0} field is required.", "Pickup directory location"]);
            }

            settings.DefaultSender = model.DefaultSender;
            settings.Host = model.Host;
            settings.Port = model.Port;
            settings.AutoSelectEncryption = model.AutoSelectEncryption;
            settings.RequireCredentials = model.RequireCredentials;
            settings.UseDefaultCredentials = model.UseDefaultCredentials;
            settings.EncryptionMethod = model.EncryptionMethod;
            settings.UserName = model.UserName;
            settings.ProxyHost = model.ProxyHost;
            settings.ProxyPort = model.ProxyPort;
            settings.IgnoreInvalidSslCertificate = model.IgnoreInvalidSslCertificate;
            settings.DeliveryMethod = model.DeliveryMethod;
            settings.PickupDirectoryLocation = model.PickupDirectoryLocation;

            if (settings.Password != model.Password)
            {
                var protector = _dataProtectionProvider.CreateProtector(SmtpOptionsConfiguration.ProtectorName);

                settings.Password = protector.Protect(model.Password);
            }

            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }
}

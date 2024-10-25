using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Core;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Email.Smtp.ViewModels;
using OrchardCore.Entities;
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
    private readonly SmtpOptions _smtpOptions;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailAddressValidator _emailValidator;

    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public SmtpSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        IOptions<SmtpOptions> options,
        IAuthorizationService authorizationService,
        IEmailAddressValidator emailAddressValidator,
        IStringLocalizer<SmtpSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _smtpOptions = options.Value;
        _authorizationService = authorizationService;
        _emailValidator = emailAddressValidator;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(ISite site, SmtpSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        return Initialize<SmtpSettingsViewModel>("SmtpSettings_Edit", model =>
        {
            // For backward compatibility with instances before the SMTP provider was factored out of
            // OrchardCore.Email, if IsEnabled is null, we check to see if there's already valid configuration.
            model.IsEnabled = settings.IsEnabled ?? _smtpOptions.ConfigurationExists();
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
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new SmtpSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var emailSettings = site.As<EmailSettings>();

        var hasChanges = model.IsEnabled != settings.IsEnabled;

        if (!model.IsEnabled)
        {
            if (hasChanges && emailSettings.DefaultProviderName == SmtpEmailProvider.TechnicalName)
            {
                emailSettings.DefaultProviderName = null;

                site.Put(emailSettings);
            }

            settings.IsEnabled = false;
        }
        else
        {
            if (string.IsNullOrEmpty(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is a required field."]);
            }
            else if (!_emailValidator.Validate(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is invalid."]);
            }

            if (model.DeliveryMethod == SmtpDeliveryMethod.Network
                && string.IsNullOrWhiteSpace(model.Host))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Host), S["The {0} field is required.", "Host name"]);
            }
            else if (model.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory
                && string.IsNullOrWhiteSpace(model.PickupDirectoryLocation))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PickupDirectoryLocation), S["The {0} field is required.", "Pickup directory location"]);
            }

            hasChanges |= model.DefaultSender != settings.DefaultSender;
            hasChanges |= model.Host != settings.Host;
            hasChanges |= model.Port != settings.Port;
            hasChanges |= model.AutoSelectEncryption != settings.AutoSelectEncryption;
            hasChanges |= model.RequireCredentials != settings.RequireCredentials;
            hasChanges |= model.UseDefaultCredentials != settings.UseDefaultCredentials;
            hasChanges |= model.EncryptionMethod != settings.EncryptionMethod;
            hasChanges |= model.UserName != settings.UserName;
            hasChanges |= model.ProxyHost != settings.ProxyHost;
            hasChanges |= model.ProxyPort != settings.ProxyPort;
            hasChanges |= model.IgnoreInvalidSslCertificate != settings.IgnoreInvalidSslCertificate;
            hasChanges |= model.DeliveryMethod != settings.DeliveryMethod;
            hasChanges |= model.PickupDirectoryLocation != settings.PickupDirectoryLocation;

            // Store the password when there is a new value.
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                // Encrypt the password.
                var protector = _dataProtectionProvider.CreateProtector(SmtpOptionsConfiguration.ProtectorName);

                var protectedPassword = protector.Protect(model.Password);

                // Check if the password changed before setting the password.
                hasChanges |= protectedPassword != settings.Password;

                settings.Password = protectedPassword;
            }

            settings.IsEnabled = true;
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
        }

        if (context.Updater.ModelState.IsValid)
        {
            if (settings.IsEnabled == true && string.IsNullOrEmpty(emailSettings.DefaultProviderName))
            {
                // If we are enabling the only provider, set it as the default one.
                emailSettings.DefaultProviderName = SmtpEmailProvider.TechnicalName;
                site.Put(emailSettings);

                hasChanges = true;
            }

            if (hasChanges)
            {
                _shellReleaseManager.RequestRelease();
            }
        }

        return await EditAsync(site, settings, context);
    }
}

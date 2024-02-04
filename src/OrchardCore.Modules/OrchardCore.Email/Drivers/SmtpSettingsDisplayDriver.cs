using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpSettings>
    {
        [Obsolete("This property should no longer be used. Instead use EmailSettings.GroupId")]
        public const string GroupId = EmailSettings.GroupId;

        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;

        protected readonly IHtmlLocalizer H;

        public SmtpSettingsDisplayDriver(
            IDataProtectionProvider dataProtectionProvider,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<SmtpSettingsDisplayDriver> htmlLocalizer,
            IAuthorizationService authorizationService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _notifier = notifier;
            _authorizationService = authorizationService;
            H = htmlLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(SmtpSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            return Initialize<SmtpSettings>("SmtpSettings_Edit", model =>
            {
                model.IsEnabled = settings.IsEnabled;
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
            .OnGroup(EmailSettings.GroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(ISite site, SmtpSettings settings, IUpdateModel updater, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            if (!context.GroupId.Equals(EmailSettings.GroupId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var model = new SmtpSettings();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                var emailSettings = site.As<EmailSettings>();

                var hasChanges = model.IsEnabled != settings.IsEnabled;

                if (!model.IsEnabled)
                {
                    if (hasChanges && emailSettings.DefaultProviderName == SmtpEmailProvider.TechnicalName)
                    {
                        emailSettings.DefaultProviderName = null;

                        site.Put(emailSettings);

                        await _notifier.WarningAsync(H["You have successfully disabled the default Email provider. The Email service is now disable and will remain disabled until you designate a new default provider."]);
                    }

                    settings.IsEnabled = false;
                }
                else
                {
                    if (string.IsNullOrEmpty(emailSettings.DefaultProviderName))
                    {
                        await _notifier.WarningAsync(H["No designated default email provider is currently enabled. Please select and set one of the available email providers as the default."]);
                    }

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

                    // Restore password if the input is empty, meaning that it has not been reset.
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        // Encrypt the password.
                        var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));

                        var protectedPassword = protector.Protect(model.Password);

                        // Check if the password changed before setting the password.
                        hasChanges |= protectedPassword != settings.Password;

                        settings.Password = protectedPassword;
                    }

                    settings.IsEnabled = true;
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

                // Release the tenant to apply the settings when something changed.
                if (context.Updater.ModelState.IsValid && hasChanges)
                {
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}

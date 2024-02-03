using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
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
        private readonly IAuthorizationService _authorizationService;

        public SmtpSettingsDisplayDriver(
            IDataProtectionProvider dataProtectionProvider,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
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

        public override async Task<IDisplayResult> UpdateAsync(SmtpSettings section, BuildEditorContext context)
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

            var previousPassword = section.Password;
            await context.Updater.TryUpdateModelAsync(section, Prefix);

            // Restore password if the input is empty, meaning that it has not been reset.
            if (string.IsNullOrWhiteSpace(section.Password))
            {
                section.Password = previousPassword;
            }
            else
            {
                // encrypt the password
                var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpSettingsConfiguration));
                section.Password = protector.Protect(section.Password);
            }

            // Release the tenant to apply the settings.
            await _shellHost.ReleaseShellContextAsync(_shellSettings);

            return await EditAsync(section, context);
        }
    }
}

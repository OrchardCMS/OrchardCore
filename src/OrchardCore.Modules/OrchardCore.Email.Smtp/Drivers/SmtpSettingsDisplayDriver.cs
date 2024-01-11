using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpEmailSettings>
    {
        public const string GroupId = "smtp email";
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly ISiteService _site;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private string _defaultSender;

        public SmtpSettingsDisplayDriver(
            IDataProtectionProvider dataProtectionProvider,
            IShellHost shellHost,
            ShellSettings shellSettings,
            ISiteService site,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _site = site;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(SmtpEmailSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSmtpEmailSettings))
            {
                return null;
            }

            if (_defaultSender == null)
            {
                var emailSettings = (await _site.GetSiteSettingsAsync()).As<EmailSettings>();

                _defaultSender = emailSettings.DefaultSender;
            }

            var shapes = new List<IDisplayResult>
            {
                Initialize<SmtpEmailSettings>("SmtpEmailSettings_Edit", model =>
                {
                    model.DefaultSender = _defaultSender;
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
                }).Location("Content:5").OnGroup(GroupId),
            };

            if (_defaultSender != null)
            {
                shapes.Add(Dynamic("SmtpEmailSettings_TestButton").Location("Actions").OnGroup(GroupId));
            }

            return Combine(shapes);
        }

        public override async Task<IDisplayResult> UpdateAsync(SmtpEmailSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageSmtpEmailSettings))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                await context.Updater.TryUpdateModelAsync(section, Prefix);

                // Don't check the DefaultSender from within SmtpEmailSettings.
                context.Updater.ModelState.Remove($"{nameof(ISite)}.{nameof(SmtpEmailSettings.DefaultSender)}");

                var previousPassword = section.Password;
                // Restore password if the input is empty, meaning that it has not been reset.
                if (string.IsNullOrWhiteSpace(section.Password))
                {
                    section.Password = previousPassword;
                }
                else
                {
                    // Encrypt the password.
                    var protector = _dataProtectionProvider.CreateProtector(nameof(SmtpEmailSettingsConfiguration));
                    section.Password = protector.Protect(section.Password);
                }

                // Release the tenant to apply the settings.
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }

            return await EditAsync(section, context);
        }
    }
}

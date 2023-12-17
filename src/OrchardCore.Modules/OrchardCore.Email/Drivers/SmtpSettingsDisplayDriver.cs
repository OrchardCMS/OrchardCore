using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.ViewModels;
using OrchardCore.Environment.Shell;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Models;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpSettings>
    {
        public const string GroupId = "email";
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISecretService _secretService;

        public SmtpSettingsDisplayDriver(
            IDataProtectionProvider dataProtectionProvider,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            ISecretService secretService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _secretService = secretService;
        }

        public override async Task<IDisplayResult> EditAsync(SmtpSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            var secret = await _secretService.GetSecretAsync<TextSecret>(Secrets.Password);
            var shapes = new List<IDisplayResult>
            {
                Initialize<SmtpSettingsEditViewModel>("SmtpSettings_Edit", model =>
                {
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
                    model.Password = secret?.Text;
                    model.IgnoreInvalidSslCertificate = settings.IgnoreInvalidSslCertificate;
                }).Location("Content:5").OnGroup(GroupId),
            };

            if (settings.DefaultSender != null)
            {
                shapes.Add(Dynamic("SmtpSettings_TestButton").Location("Actions").OnGroup(GroupId));
            }

            return Combine(shapes);
        }

        public override async Task<IDisplayResult> UpdateAsync(SmtpSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            if (context.GroupId.Equals(GroupId, StringComparison.OrdinalIgnoreCase))
            {
                var model = new SmtpSettingsEditViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                var passwordSecret = await _secretService.GetSecretAsync<TextSecret>(Secrets.Password);
                if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != passwordSecret?.Text)
                {
                    if (passwordSecret is null)
                    {
                        await _secretService.AddSecretAsync<TextSecret>(
                            name: Secrets.Password,
                            configure: (secret, info) => secret.Text = model.Password);
                    }
                    else
                    {
                        passwordSecret.Text = model.Password;
                        await _secretService.UpdateSecretAsync(passwordSecret);
                    }
                }

                settings = model;

                // Release the tenant to apply the settings.
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }

            return await EditAsync(settings, context);
        }
    }
}

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
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpSettings>
    {
        public const string GroupId = "smtp";
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IShellHost _orchardHost;
        private readonly ShellSettings _currentShellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;

        public SmtpSettingsDisplayDriver(
            IDataProtectionProvider dataProtectionProvider, 
            IShellHost orchardHost, 
            ShellSettings currentShellSettings,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _orchardHost = orchardHost;
            _currentShellSettings = currentShellSettings;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
        }

        public override async Task<IDisplayResult> EditAsync(SmtpSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            var shapes = new List<IDisplayResult>
            {
                Initialize<SmtpSettings>("SmtpSettings_Edit", model =>
                {
                    model.DefaultSender = section.DefaultSender;
                    model.DeliveryMethod = section.DeliveryMethod;
                    model.PickupDirectoryLocation = section.PickupDirectoryLocation;
                    model.Host = section.Host;
                    model.Port = section.Port;
                    model.EnableSsl = section.EnableSsl;
                    model.RequireCredentials = section.RequireCredentials;
                    model.UseDefaultCredentials = section.UseDefaultCredentials;
                    model.UserName = section.UserName;
                    model.Password = section.Password;
                }).Location("Content:5").OnGroup(GroupId)
            };

            if (section?.DefaultSender != null)
            {
                shapes.Add(Dynamic("SmtpSettings_TestButton").Location("Actions").OnGroup(GroupId));
            }

            return Combine(shapes);
        }

        public override async Task<IDisplayResult> UpdateAsync(SmtpSettings section, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
            {
                return null;
            }

            if (context.GroupId == GroupId)
            {
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

                // Reload the tenant to apply the settings
                await _orchardHost.ReloadShellContextAsync(_currentShellSettings);
            }

            return await EditAsync(section, context);
        }
    }
}

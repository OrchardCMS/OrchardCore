using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Services;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpSettings>
    {
        public const string GroupId = "SmtpSettings";
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IShellHost _orchardHost;
        private readonly ShellSettings _currentShellSettings;

        public SmtpSettingsDisplayDriver(IDataProtectionProvider dataProtectionProvider, IShellHost orchardHost, ShellSettings currentShellSettings)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _orchardHost = orchardHost;
            _currentShellSettings = currentShellSettings;
        }

        public override IDisplayResult Edit(SmtpSettings section)
        {
            var shapes = new List<IDisplayResult>
            {
                Shape<SmtpSettings>("SmtpSettings_Edit", model =>
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
                shapes.Add(Shape("SmtpSettings_TestButton").Location("Actions").OnGroup(GroupId));
            }

            return Combine(shapes);
        }

        public override async Task<IDisplayResult> UpdateAsync(SmtpSettings section, IUpdateModel updater, string groupId)
        {
            if (groupId == GroupId)
            {
                var previousPassword = section.Password;
                await updater.TryUpdateModelAsync(section, Prefix);

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
                _orchardHost.ReloadShellContext(_currentShellSettings);
            }

            return Edit(section);
        }
    }
}

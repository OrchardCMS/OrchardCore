using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email.Models;
using OrchardCore.Entities.DisplayManagement;
using OrchardCore.Settings;

namespace OrchardCore.Email.Drivers
{
    public class SmtpSettingsDisplayDriver : SectionDisplayDriver<ISite, SmtpSettings>
    {
        public const string GroupId = "SmtpSettings";

        public override IDisplayResult Edit(SmtpSettings section)
        {
            return Shape<SmtpSettings>( "SmtpSettings_Edit", model => {
                model.DefaultSender = section.DefaultSender;
                model.Host = section.Host;
                model.Port = section.Port;
                model.EnableSsl = section.EnableSsl;
                model.RequireCredentials = section.RequireCredentials;
                model.UseDefaultCredentials = section.UseDefaultCredentials;
                model.UserName = section.UserName;
                model.Password = section.Password;
            } ).Location( "Content:5" ).OnGroup( GroupId );
        }

        public override async Task<IDisplayResult> UpdateAsync(SmtpSettings section, IUpdateModel updater, string groupId)
        {
            if ( groupId == GroupId )
            {
                var previousPassword = section.Password;
                await updater.TryUpdateModelAsync( section, Prefix );

                // Restore password if the input is empty, meaning that it has not been reset.
                if ( string.IsNullOrWhiteSpace(section.Password))
                {
                    section.Password = previousPassword;
                }
            }

            return Edit( section );
        }
    }
}

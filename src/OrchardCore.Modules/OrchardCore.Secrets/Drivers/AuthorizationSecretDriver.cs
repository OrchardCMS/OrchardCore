using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers
{
    public class AuthorizationSecretDriver : DisplayDriver<Secret, AuthorizationSecret>
    {


        public override IDisplayResult Display(AuthorizationSecret secret)
        {
            return
                Combine(
                    View("AuthorizationSecret_Fields_Summary", secret).Location("Summary", "Content"),
                    View("AuthorizationSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AuthorizationSecret secret)
        {
            return Initialize<AuthorizationSecretViewModel>("AuthorizationSecret_Fields_Edit", model =>
            {
                model.AuthenticationString = secret.AuthenticationString;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(AuthorizationSecret secret, IUpdateModel updater)
        {
            var model = new AuthorizationSecretViewModel();

            if (await updater.TryUpdateModelAsync(model, Prefix))
            {
                secret.AuthenticationString = model.AuthenticationString;
            }

            return Edit(secret);
        }
    }
}

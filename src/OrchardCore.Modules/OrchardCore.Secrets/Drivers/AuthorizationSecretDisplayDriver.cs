using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers
{
    public class AuthorizationSecretDisplayDriver : DisplayDriver<Secret, AuthorizationSecret>
    {
        private readonly IStringLocalizer S;

        public AuthorizationSecretDisplayDriver(IStringLocalizer<AuthorizationSecretDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(AuthorizationSecret secret)
        {
            return View("AuthorizationSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content");
        }

        public override Task<IDisplayResult> EditAsync(AuthorizationSecret secret, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(Initialize<AuthorizationSecretViewModel>("AuthorizationSecret_Fields_Edit", model =>
            {
                // The authentication string is never returned to the view.
                model.AuthenticationString = String.Empty;
                model.Context = context;
            }).Location("Content"));
        }

        public override async Task<IDisplayResult> UpdateAsync(AuthorizationSecret secret, UpdateEditorContext context)
        {
            var model = new AuthorizationSecretViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                if (context.IsNew && String.IsNullOrEmpty(model.AuthenticationString))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.AuthenticationString), S["The authentication string is required"]);
                }

                // The authentication string is only updated when a new value has been provided.
                if (!String.IsNullOrEmpty(model.AuthenticationString))
                {
                    secret.AuthenticationString = model.AuthenticationString;
                }
            }

            return await EditAsync(secret, context);
        }
    }
}

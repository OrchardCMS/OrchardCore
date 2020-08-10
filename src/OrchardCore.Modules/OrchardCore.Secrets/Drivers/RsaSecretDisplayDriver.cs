using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers
{
    public class RsaSecretDisplayDriver : DisplayDriver<Secret, RsaSecret>
    {
        private readonly IStringLocalizer S;

        public RsaSecretDisplayDriver(IStringLocalizer<AuthorizationSecretDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(RsaSecret secret)
        {
            return View("RsaSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content");
        }

        public override Task<IDisplayResult> EditAsync(RsaSecret secret, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(Initialize<RsaSecretViewModel>("RsaSecret_Fields_Edit", model =>
            {
                // When this is new generate
                if (context.IsNew)
                {
                    using (var rsa = RSA.Create())
                    {
                        if (String.IsNullOrEmpty(secret.PublicKey))
                        {
                            model.PublicKey = WebEncoders.Base64UrlEncode(rsa.ExportSubjectPublicKeyInfo());
                        }
                        else
                        {
                            // If key validation fails we return the value supplied.
                            model.PublicKey = secret.PublicKey;
                        }
                        if (String.IsNullOrEmpty(secret.PrivateKey))
                        {
                            model.PrivateKey = WebEncoders.Base64UrlEncode(rsa.ExportRSAPrivateKey());
                        }
                        else
                        {
                            model.PrivateKey = secret.PrivateKey;
                        }
                    }
                }
                else
                {
                    // The private key is never returned to the view when editing.
                    model.PublicKey = secret.PublicKey;
                    using (var rsa = RSA.Create())
                    {
                        model.NewPublicKey = WebEncoders.Base64UrlEncode(rsa.ExportSubjectPublicKeyInfo());
                        model.NewPrivateKey = WebEncoders.Base64UrlEncode(rsa.ExportRSAPrivateKey());
                    }
                }

                model.Context = context;
            }).Location("Content"));
        }

        public override async Task<IDisplayResult> UpdateAsync(RsaSecret secret, UpdateEditorContext context)
        {
            var model = new RsaSecretViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix))
            {
                // transfer values, then validate.
                // TODO we should just store this as a UTF8 string not pem.


                // The view will contain the private key
                if (context.IsNew)
                {
                    secret.PublicKey = model.PublicKey;
                    secret.PrivateKey = model.PrivateKey;

                }
                else if (model.CycleKey)
                {
                    secret.PublicKey = model.NewPublicKey;
                    secret.PrivateKey = model.NewPrivateKey;
                }
                try
                {
                    using (var rsa = RSA.Create())
                    {
                        rsa.ImportRSAPrivateKey(secret.PrivateKeyAsBytes(), out _);
                    }
                }
                catch (CryptographicException)
                {
                    // context.Updater.ModelState.AddModelError(Prefix, nameof(model.PrivateKey), S["The private key cannot be decoded."]);
                }

                try
                {
                    using (var rsa = RSA.Create())
                    {
                        rsa.ImportSubjectPublicKeyInfo(secret.PublicKeyAsBytes(), out _);
                    }
                }
                catch (CryptographicException)
                {
                    // context.Updater.ModelState.AddModelError(Prefix, nameof(model.PublicKey), S["The public key cannot be decoded"]);
                }
                // if (context.IsNew && String.IsNullOrEmpty(model.AuthenticationString))
                // {
                //     context.Updater.ModelState.AddModelError(Prefix, nameof(model.AuthenticationString), S["The authentication string is required"]);
                // }

                // // The authentication string is only updated when a new value has been provided.
                // if (!String.IsNullOrEmpty(model.AuthenticationString))
                // {
                //     secret.AuthenticationString = model.AuthenticationString;
                // }
            }

            return await EditAsync(secret, context);
        }
    }
}

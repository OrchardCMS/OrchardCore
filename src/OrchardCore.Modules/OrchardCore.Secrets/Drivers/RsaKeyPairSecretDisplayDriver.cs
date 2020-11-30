using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers
{
    public class RsaKeyPairSecretDisplayDriver : DisplayDriver<Secret, RsaKeyPair>
    {
        private readonly IStringLocalizer S;

        public RsaKeyPairSecretDisplayDriver(IStringLocalizer<RsaKeyPairSecretDisplayDriver> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public override IDisplayResult Display(RsaKeyPair secret)
        {
            return View("RsaKeyPairSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content");
        }

        public override Task<IDisplayResult> EditAsync(RsaKeyPair secret, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(Initialize<RsaKeyPairSecretViewModel>("RsaKeyPairSecret_Fields_Edit", model =>
            {
                // When this is new generate
                if (context.IsNew)
                {
                    using (var rsa = RSA.Create())
                    {
                        if (String.IsNullOrEmpty(secret.PublicKey))
                        {
                            model.PublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
                        }
                        else
                        {
                            // If key validation fails we return the value supplied.
                            model.PublicKey = secret.PublicKey;
                        }
                        if (String.IsNullOrEmpty(secret.PrivateKey))
                        {
                            model.PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
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

                    // TODO remove here for testing
                    // model.PrivateKey = secret.PrivateKey;
                    using (var rsa = RSA.Create())
                    {
                        model.NewPublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
                        model.NewPrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                    }
                }

                model.Context = context;
            }).Location("Content"));
        }

        public override async Task<IDisplayResult> UpdateAsync(RsaKeyPair secret, UpdateEditorContext context)
        {
            var model = new RsaKeyPairSecretViewModel();

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

                if (model.CycleKey)
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
                    if (context.IsNew)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.PrivateKey), S["The private key cannot be decoded."]);
                    }
                    else
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.NewPrivateKey), S["The private key cannot be decoded."]);
                    }
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
                    if (context.IsNew)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.PublicKey), S["The public key cannot be decoded"]);
                    }
                    else
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.NewPrivateKey), S["The public key cannot be decoded"]);
                    }
                }
            }

            return await EditAsync(secret, context);
        }
    }
}

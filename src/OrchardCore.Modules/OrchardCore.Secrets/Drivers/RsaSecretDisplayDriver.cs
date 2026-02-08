using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers;

public sealed class RsaSecretDisplayDriver : DisplayDriver<ISecret, RsaKeySecret>
{
    internal readonly IStringLocalizer S;

    public RsaSecretDisplayDriver(IStringLocalizer<RsaSecretDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(RsaKeySecret secret, BuildDisplayContext context)
    {
        return CombineAsync(
            View("RsaSecret_Fields_Summary", secret).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("RsaSecret_Fields_Thumbnail", secret).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(RsaKeySecret secret, BuildEditorContext context)
    {
        return Initialize<RsaSecretViewModel>("RsaSecret_Fields_Edit", model =>
        {
            model.IsNew = context.IsNew;

            // Generate new keys when creating.
            if (context.IsNew)
            {
                using var rsa = RSA.Create(2048);

                if (string.IsNullOrEmpty(secret.PublicKey))
                {
                    model.PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                }
                else
                {
                    model.PublicKey = secret.PublicKey;
                }

                if (string.IsNullOrEmpty(secret.PrivateKey))
                {
                    model.PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                }
                else
                {
                    model.PrivateKey = secret.PrivateKey;
                }
            }
            else
            {
                // The private key is never returned to the view when editing.
                model.PublicKey = secret.PublicKey;

                using var rsa = RSA.Create(2048);
                model.NewPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
                model.NewPrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            }

            model.IncludesPrivateKey = secret.IncludesPrivateKey;
            model.KeyTypes =
            [
                new SelectListItem
                {
                    Text = S["Public Key Only"],
                    Value = "false",
                    Selected = !model.IncludesPrivateKey,
                },
                new SelectListItem
                {
                    Text = S["Public / Private Key Pair"],
                    Value = "true",
                    Selected = model.IncludesPrivateKey,
                },
            ];
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(RsaKeySecret secret, UpdateEditorContext context)
    {
        var model = new RsaSecretViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        secret.IncludesPrivateKey = model.IncludesPrivateKey;

        // The view will contain the private key when creating.
        if (context.IsNew)
        {
            secret.PublicKey = model.PublicKey;
            secret.PrivateKey = model.IncludesPrivateKey ? model.PrivateKey : null;
        }
        else
        {
            if (!model.IncludesPrivateKey)
            {
                secret.PublicKey = model.PublicKey;
                secret.PrivateKey = null;
            }
            else if (model.HasNewKeys)
            {
                secret.PublicKey = model.NewPublicKey;
                secret.PrivateKey = model.NewPrivateKey;
            }
        }

        // Validate the keys
        if (secret.IncludesPrivateKey && !string.IsNullOrEmpty(secret.PrivateKey))
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(secret.PrivateKey), out _);
            }
            catch (CryptographicException)
            {
                var fieldName = context.IsNew ? nameof(model.PrivateKey) : nameof(model.NewPrivateKey);
                context.Updater.ModelState.AddModelError(Prefix, fieldName, S["The private key cannot be decoded."]);
            }
        }

        if (!string.IsNullOrEmpty(secret.PublicKey))
        {
            try
            {
                using var rsa = RSA.Create();
                rsa.ImportRSAPublicKey(Convert.FromBase64String(secret.PublicKey), out _);
            }
            catch (CryptographicException)
            {
                var fieldName = context.IsNew ? nameof(model.PublicKey) : nameof(model.NewPublicKey);
                context.Updater.ModelState.AddModelError(Prefix, fieldName, S["The public key cannot be decoded."]);
            }
        }

        return Edit(secret, context);
    }
}

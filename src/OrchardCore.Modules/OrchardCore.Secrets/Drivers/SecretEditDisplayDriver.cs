using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers;

/// <summary>
/// Builds the editor of a secret, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class SecretEditDisplayDriver : DisplayDriver<SecretEditViewModel>
{
    internal readonly IStringLocalizer S;

    public SecretEditDisplayDriver(IStringLocalizer<SecretEditDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(SecretEditViewModel model, BuildEditorContext context)
    {
        // The card of the value is named after what the secret keeps.
        var valueCardName = model.SecretType switch
        {
            nameof(RsaKeySecret) => S["Key"],
            nameof(X509Secret) => S["Certificate"],
            _ => S["Value"],
        };

        return Combine(
            Initialize<SecretEditViewModel>("SecretEditGeneral_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "1").Card(S["General"], "10")),
            Initialize<SecretEditViewModel>("SecretEditValue_Edit", m => Populate(model, m))
                .Location(l => l.Zone("Content", "2").Card(valueCardName, "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(SecretEditViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            m => m.Name,
            m => m.Store,
            m => m.Description,
            m => m.ExpiresUtc,
            m => m.TextValue,
            m => m.RsaKeySize,
            m => m.X509Source,
            m => m.X509CertificatePem,
            m => m.X509PrivateKeyPem,
            m => m.X509StoreLocation,
            m => m.X509StoreName,
            m => m.X509Thumbprint);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates and the script of the page selects.
    protected override void BuildPrefix(SecretEditViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }

    private static void Populate(SecretEditViewModel source, SecretEditViewModel model)
    {
        model.IsNew = source.IsNew;
        model.Name = source.Name;
        model.Store = source.Store;
        model.Description = source.Description;
        model.ExpiresUtc = source.ExpiresUtc;
        model.SecretType = source.SecretType;
        model.SecretTypeDisplayName = source.SecretTypeDisplayName;
        model.AvailableStores = source.AvailableStores;
        model.TextValue = source.TextValue;
        model.RsaPublicKey = source.RsaPublicKey;
        model.RsaPrivateKey = source.RsaPrivateKey;
        model.RsaKeySize = source.RsaKeySize;
        model.X509Source = source.X509Source;
        model.X509StoreLocation = source.X509StoreLocation;
        model.X509StoreName = source.X509StoreName;
        model.X509Thumbprint = source.X509Thumbprint;
        model.X509CertificatePem = source.X509CertificatePem;
        model.X509PrivateKeyPem = source.X509PrivateKeyPem;
        model.X509HasPrivateKey = source.X509HasPrivateKey;
    }
}

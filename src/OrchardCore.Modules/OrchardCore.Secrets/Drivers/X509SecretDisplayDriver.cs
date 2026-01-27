using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Drivers;

public sealed class X509SecretDisplayDriver : DisplayDriver<SecretBase, X509Secret>
{
    internal readonly IStringLocalizer S;

    public X509SecretDisplayDriver(IStringLocalizer<X509SecretDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(X509Secret secret, BuildDisplayContext context)
    {
        return CombineAsync(
            View("X509Secret_Fields_Summary", secret).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("X509Secret_Fields_Thumbnail", secret).Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(X509Secret secret, BuildEditorContext context)
    {
        return Initialize<X509SecretViewModel>("X509Secret_Fields_Edit", model =>
        {
            model.StoreLocation = secret.StoreLocation;
            model.StoreName = secret.StoreName;
            model.Thumbprint = secret.Thumbprint;
            model.IsNew = context.IsNew;

            // Load available certificates
            foreach (var (certificate, location, name) in GetAvailableCertificates())
            {
                model.AvailableCertificates.Add(new CertificateInfo
                {
                    StoreLocation = location,
                    StoreName = name,
                    FriendlyName = certificate.FriendlyName,
                    Issuer = certificate.Issuer,
                    Subject = certificate.Subject,
                    NotBefore = certificate.NotBefore,
                    NotAfter = certificate.NotAfter,
                    Thumbprint = certificate.Thumbprint,
                    HasPrivateKey = certificate.HasPrivateKey,
                    Archived = certificate.Archived,
                });
            }
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(X509Secret secret, UpdateEditorContext context)
    {
        var model = new X509SecretViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        secret.StoreLocation = model.StoreLocation;
        secret.StoreName = model.StoreName;
        secret.Thumbprint = model.Thumbprint;

        return Edit(secret, context);
    }

    private static List<(X509Certificate2 certificate, StoreLocation location, StoreName name)> GetAvailableCertificates()
    {
        var certificates = new List<(X509Certificate2, StoreLocation, StoreName)>();

        foreach (StoreLocation location in Enum.GetValues<StoreLocation>())
        {
            foreach (StoreName name in Enum.GetValues<StoreName>())
            {
                // Note: on non-Windows platforms, an exception can
                // be thrown if the store location/name doesn't exist.
                try
                {
                    using var store = new X509Store(name, location);
                    store.Open(OpenFlags.ReadOnly);

                    foreach (var certificate in store.Certificates)
                    {
                        if (!certificate.Archived && certificate.HasPrivateKey)
                        {
                            certificates.Add((certificate, location, name));
                        }
                    }
                }
                catch (CryptographicException)
                {
                    continue;
                }
            }
        }

        return certificates;
    }
}

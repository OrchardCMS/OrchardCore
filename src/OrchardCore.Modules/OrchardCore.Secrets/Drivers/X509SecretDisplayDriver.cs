using System;
using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.ViewModels;
using static OrchardCore.Secrets.ViewModels.X509SecretViewModel;

namespace OrchardCore.Secrets.Drivers;

public class X509SecretDisplayDriver : DisplayDriver<SecretBase, X509Secret>
{
    protected readonly IStringLocalizer S;

    public X509SecretDisplayDriver(IStringLocalizer<X509SecretDisplayDriver> stringLocalizer) => S = stringLocalizer;

    public override IDisplayResult Display(X509Secret secret)
    {
        return Combine(
            View("X509Secret_Fields_Summary", secret).Location("Summary", "Content"),
            View("X509Secret_Fields_Thumbnail", secret).Location("Thumbnail", "Content"));
    }

    public override Task<IDisplayResult> EditAsync(X509Secret secret, BuildEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(Initialize<X509SecretViewModel>("X509Secret_Fields_Edit", async model =>
        {
            model.StoreLocation = secret.StoreLocation;
            model.StoreName = secret.StoreName;
            model.Thumbprint = secret.Thumbprint;
            model.Context = context;

            foreach (var (certificate, location, name) in await GetAvailableCertificatesAsync())
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
                    ThumbPrint = certificate.Thumbprint,
                    HasPrivateKey = certificate.HasPrivateKey,
                    Archived = certificate.Archived
                });
            }
        })
            .Location("Content"));
    }

    public override async Task<IDisplayResult> UpdateAsync(X509Secret secret, UpdateEditorContext context)
    {
        var model = new X509SecretViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            secret.StoreLocation = model.StoreLocation;
            secret.StoreName = model.StoreName;
            secret.Thumbprint = model.Thumbprint;
        }

        return await EditAsync(secret, context);
    }


    private static Task<ImmutableArray<(X509Certificate2 certificate, StoreLocation location, StoreName name)>> GetAvailableCertificatesAsync()
    {
        var certificates = ImmutableArray.CreateBuilder<(X509Certificate2, StoreLocation, StoreName)>();

        foreach (StoreLocation location in Enum.GetValues(typeof(StoreLocation)))
        {
            foreach (StoreName name in Enum.GetValues(typeof(StoreName)))
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

        return Task.FromResult(certificates.ToImmutable());
    }
}

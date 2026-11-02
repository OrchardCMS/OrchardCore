using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Drivers;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class SecretDisplayDriverTests
{
    [Fact]
    public async Task Rsa_PublicOnlyCannotBecomePairWithoutPrivateKey()
    {
        using var rsa = RSA.Create(2048);
        var secret = new RsaKeySecret { PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()), IncludesPrivateKey = false };
        var updater = CreateUpdater(new RsaSecretViewModel { IncludesPrivateKey = true });

        await new RsaSecretDisplayDriver(Localizer<RsaSecretDisplayDriver>())
            .UpdateAsync(secret, Context(updater.Object));

        Assert.False(updater.Object.ModelState.IsValid);
    }

    [Fact]
    public async Task Rsa_GeneratedPairContainsValidPrivateKey()
    {
        using var rsa = RSA.Create(2048);
        var secret = new RsaKeySecret { IncludesPrivateKey = false };
        var updater = CreateUpdater(new RsaSecretViewModel
        {
            IncludesPrivateKey = true,
            HasNewKeys = true,
            NewPublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            NewPrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
        });

        await new RsaSecretDisplayDriver(Localizer<RsaSecretDisplayDriver>())
            .UpdateAsync(secret, Context(updater.Object));

        Assert.True(updater.Object.ModelState.IsValid);
        Assert.True(secret.IncludesPrivateKey);
        Assert.NotEmpty(secret.PrivateKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("CurrentUser|My|missing")]
    [InlineData("999|My|missing")]
    public async Task X509_RejectsInvalidOrUnavailableCertificateTuple(string selection)
    {
        var secret = new X509Secret { Thumbprint = "original" };
        var updater = CreateUpdater(new X509SecretViewModel { SelectedCertificate = selection });

        await new X509SecretDisplayDriver(Localizer<X509SecretDisplayDriver>())
            .UpdateAsync(secret, Context(updater.Object));

        Assert.False(updater.Object.ModelState.IsValid);
        Assert.Equal("original", secret.Thumbprint);
    }

    private static UpdateEditorContext Context(IUpdateModel updater) =>
        new(Mock.Of<IShape>(), string.Empty, false, string.Empty, null, null, updater);

    private static Mock<IUpdateModel> CreateUpdater<T>(T posted) where T : class
    {
        var updater = new Mock<IUpdateModel>();
        updater.SetupGet(u => u.ModelState).Returns(new ModelStateDictionary());
        updater.Setup(u => u.TryUpdateModelAsync(It.IsAny<T>(), It.IsAny<string>()))
            .Callback((T model, string prefix) =>
            {
                foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
                {
                    property.SetValue(model, property.GetValue(posted));
                }
            })
            .ReturnsAsync(true);
        return updater;
    }

    private static IStringLocalizer<T> Localizer<T>()
    {
        var localizer = new Mock<IStringLocalizer<T>>();
        localizer.Setup(s => s[It.IsAny<string>()]).Returns((string key) => new LocalizedString(key, key));
        return localizer.Object;
    }
}

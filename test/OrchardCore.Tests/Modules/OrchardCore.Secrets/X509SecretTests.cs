using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Localization;
using Moq;
using OrchardCore.Secrets;
using OrchardCore.Secrets.Providers;

namespace OrchardCore.Tests.Modules.OrchardCore.Secrets;

public class X509SecretTests
{
    [Fact]
    public void FromPem_CombinedPem_StoresPrivateKeySeparately()
    {
        var (certificatePem, privateKeyPem) = CreateCertificatePem();

        var secret = X509Secret.FromPem(certificatePem + privateKeyPem);

        Assert.Equal(X509SecretSource.Pem, secret.Source);
        Assert.Contains("BEGIN CERTIFICATE", secret.CertificatePem);
        Assert.DoesNotContain("PRIVATE KEY", secret.CertificatePem);
        Assert.Contains("BEGIN PRIVATE KEY", secret.PrivateKeyPem);
    }

    [Fact]
    public void FromPem_CertificateChain_KeepsAllCertificates()
    {
        var (leafPem, _) = CreateCertificatePem("CN=leaf");
        var (issuerPem, _) = CreateCertificatePem("CN=issuer");

        var secret = X509Secret.FromPem(leafPem + issuerPem);

        Assert.Equal(2, secret.CertificatePem.Split("BEGIN CERTIFICATE").Length - 1);
        Assert.Null(secret.PrivateKeyPem);

        using var certificate = secret.GetCertificate();
        Assert.Equal("CN=leaf", certificate.Subject);
    }

    [Fact]
    public void GetCertificate_PemWithPrivateKey_LoadsCertificateWithPrivateKey()
    {
        var (certificatePem, privateKeyPem) = CreateCertificatePem();

        using var certificate = X509Secret.FromPem(certificatePem, privateKeyPem).GetCertificate();

        Assert.NotNull(certificate);
        Assert.True(certificate.HasPrivateKey);
        using var rsa = certificate.GetRSAPrivateKey();
        Assert.NotNull(rsa);
    }

    [Fact]
    public void GetCertificate_PemWithoutPrivateKey_LoadsPublicCertificate()
    {
        var (certificatePem, _) = CreateCertificatePem();

        using var certificate = X509Secret.FromPem(certificatePem).GetCertificate();

        Assert.NotNull(certificate);
        Assert.False(certificate.HasPrivateKey);
    }

    [Fact]
    public void GetCertificate_MismatchedPrivateKey_ReturnsNull()
    {
        var (certificatePem, _) = CreateCertificatePem();
        var (_, otherPrivateKeyPem) = CreateCertificatePem();

        Assert.Null(X509Secret.FromPem(certificatePem, otherPrivateKeyPem).GetCertificate());
    }

    [Fact]
    public void GetCertificate_EncryptedPrivateKey_ReturnsNull()
    {
        var certificatePem = CreateCertificatePem(out var rsa);
        using (rsa)
        {
            var encryptedPrivateKeyPem = rsa.ExportEncryptedPkcs8PrivateKeyPem("password",
                new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 1000));

            Assert.Null(X509Secret.FromPem(certificatePem, encryptedPrivateKeyPem).GetCertificate());
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not a certificate")]
    [InlineData("-----BEGIN CERTIFICATE-----\nAAAA\n-----END CERTIFICATE-----")]
    public void GetCertificate_InvalidPem_ReturnsNull(string certificatePem)
    {
        Assert.Null(new X509Secret { Source = X509SecretSource.Pem, CertificatePem = certificatePem }.GetCertificate());
    }

    [Fact]
    public void Serialize_PemSecret_RoundTrips()
    {
        var (certificatePem, privateKeyPem) = CreateCertificatePem();
        var provider = new X509SecretTypeProvider(Mock.Of<IStringLocalizer<X509SecretTypeProvider>>());

        var secret = Assert.IsType<X509Secret>(provider.Deserialize(provider.Serialize(X509Secret.FromPem(certificatePem, privateKeyPem))));

        Assert.Equal(X509SecretSource.Pem, secret.Source);
        using var certificate = secret.GetCertificate();
        Assert.True(certificate.HasPrivateKey);
    }

    [Fact]
    public void Deserialize_SecretWithoutSource_ReferencesCertificateStore()
    {
        var provider = new X509SecretTypeProvider(Mock.Of<IStringLocalizer<X509SecretTypeProvider>>());

        var secret = Assert.IsType<X509Secret>(provider.Deserialize("""{"StoreLocation":1,"StoreName":5,"Thumbprint":"thumbprint"}"""));

        Assert.Equal(X509SecretSource.Store, secret.Source);
        Assert.Equal("thumbprint", secret.Thumbprint);
    }

    internal static (string CertificatePem, string PrivateKeyPem) CreateCertificatePem(string subject = "CN=test")
    {
        var pem = CreateCertificatePem(out var rsa, subject);
        using (rsa)
        {
            return (pem, rsa.ExportPkcs8PrivateKeyPem() + '\n');
        }
    }

    private static string CreateCertificatePem(out RSA rsa, string subject = "CN=test")
    {
        rsa = RSA.Create(2048);
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));

        return certificate.ExportCertificatePem() + '\n';
    }
}

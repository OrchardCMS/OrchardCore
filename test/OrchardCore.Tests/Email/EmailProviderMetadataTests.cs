using OrchardCore.Email;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Modules;
using Module = OrchardCore.Modules.Module;

namespace OrchardCore.Tests.Email;

public class EmailProviderMetadataTests
{
    [Theory]
    [InlineData(typeof(SmtpEmailProvider), SmtpEmailProvider.TechnicalName, "Simple Mail Transfer Protocol (SMTP)")]
    [InlineData(typeof(DefaultSmtpEmailProvider), DefaultSmtpEmailProvider.TechnicalName, "Simple Mail Transfer Protocol (Default SMTP)")]
    [InlineData(typeof(AzureEmailProvider), AzureEmailProvider.TechnicalName, "Azure Communication Services")]
    [InlineData(typeof(DefaultAzureEmailProvider), DefaultAzureEmailProvider.TechnicalName, "Default Azure Communication Services")]
    public void EmailProviderTypeOptions_RegisteredModule_ReadsTypeMetadata(Type providerType, string technicalName, string displayName)
    {
        // Arrange
        Assert.True(TypeMetadata.Register(providerType.Assembly));

        // Act
        var options = new EmailProviderTypeOptions(providerType);

        // Assert
        Assert.Equal(technicalName, options.TechnicalName);
        Assert.Equal(displayName, options.DisplayName.Value);
        Assert.Equal(providerType, options.DisplayName.Type);
    }

    [Fact]
    public void TryAddProvider_TypeOptions_UsesTechnicalName()
    {
        // Arrange
        TypeMetadata.Register(typeof(SmtpEmailProvider).Assembly);
        var options = new EmailProviderOptions();

        // Act
        options.TryAddProvider(new EmailProviderTypeOptions(typeof(SmtpEmailProvider)));

        // Assert
        Assert.Equal(typeof(SmtpEmailProvider), options.Providers[SmtpEmailProvider.TechnicalName].Type);
    }

    [Fact]
    public void Module_Loaded_RegistersTypeMetadata()
    {
        // Act
        _ = new Module("OrchardCore.Email.Azure");

        // Assert
        Assert.True(Annotype.Metadata.TryGet<AzureEmailProvider, EmailProviderAttribute>(out var metadata));
        Assert.Equal(AzureEmailProvider.TechnicalName, metadata.TechnicalName);
    }

    [Fact]
    public void Register_AssemblyWithoutMetadata_ReturnsFalse()
    {
        Assert.False(TypeMetadata.Register(typeof(string).Assembly));
    }
}

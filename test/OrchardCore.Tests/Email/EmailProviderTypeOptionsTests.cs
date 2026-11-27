using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Services;

namespace OrchardCore.Tests.Email;

public class EmailProviderTypeOptionsTests
{
    [Theory]
    [InlineData(typeof(SmtpEmailProvider), "Simple Mail Transfer Protocol (SMTP)")]
    [InlineData(typeof(DefaultSmtpEmailProvider), "Simple Mail Transfer Protocol (Default SMTP)")]
    [InlineData(typeof(AzureEmailProvider), "Azure Communication Services")]
    [InlineData(typeof(DefaultAzureEmailProvider), "Default Azure Communication Services")]
    public void Create_Provider_DescribesItWithItsStaticDisplayName(Type providerType, string displayName)
    {
        // Act
        var options = (EmailProviderTypeOptions)typeof(EmailProviderTypeOptions)
            .GetMethod(nameof(EmailProviderTypeOptions.Create))
            .MakeGenericMethod(providerType)
            .Invoke(null, null);

        // Assert
        Assert.Equal(providerType, options.Type);
        Assert.Equal(displayName, options.DisplayName.Value);
        Assert.Equal(providerType, options.DisplayName.Type);
    }
}

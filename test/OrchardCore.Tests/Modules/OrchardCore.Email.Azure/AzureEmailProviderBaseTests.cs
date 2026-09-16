using Azure.Communication.Email;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Email.Azure;

public class AzureEmailProviderBaseTests
{
    [Fact]
    public void GetOrCreateEmailClient_ReturnsCachedClient_WhenConnectionStringIsUnchanged()
    {
        var provider = CreateProvider();
        const string connectionString = "endpoint=https://example.communication.azure.com/;accesskey=test-key";

        var client = provider.GetClient(connectionString);
        var cachedClient = provider.GetClient(connectionString);

        Assert.Same(client, cachedClient);
        Assert.Equal(1, provider.CreatedClientCount);
    }

    [Fact]
    public void GetOrCreateEmailClient_RecreatesClient_WhenConnectionStringChanges()
    {
        var provider = CreateProvider();
        const string oldConnectionString = "endpoint=https://example.communication.azure.com/;accesskey=test-key";
        const string newConnectionString = "endpoint=https://example.communication.azure.com/;accesskey=new-test-key";

        var client = provider.GetClient(oldConnectionString);
        var updatedClient = provider.GetClient(newConnectionString);

        Assert.NotSame(client, updatedClient);
        Assert.Equal(2, provider.CreatedClientCount);
    }

    private static TestAzureEmailProvider CreateProvider()
    {
        var optionsMonitor = Mock.Of<IOptionsMonitor<AzureEmailOptions>>();
        var localizer = new Mock<IStringLocalizer>().Object;

        return new TestAzureEmailProvider(optionsMonitor, localizer);
    }

    private sealed class TestAzureEmailProvider : AzureEmailProviderBase<AzureEmailOptions>
    {
        public TestAzureEmailProvider(IOptionsMonitor<AzureEmailOptions> optionsMonitor, IStringLocalizer stringLocalizer)
            : base(optionsMonitor, NullLogger.Instance, stringLocalizer)
        {
        }

        public int CreatedClientCount { get; private set; }

        public override LocalizedString DisplayName => new(nameof(TestAzureEmailProvider), nameof(TestAzureEmailProvider));

        public EmailClient GetClient(string connectionString) => GetOrCreateEmailClient(connectionString);

        protected override EmailClient CreateEmailClient(string connectionString)
        {
            CreatedClientCount++;
            return base.CreateEmailClient(connectionString);
        }
    }
}

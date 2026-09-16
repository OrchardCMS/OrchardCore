using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Redis;
using OrchardCore.SignalR.Redis;
using StackExchange.Redis;
using SignalRRedisOptions = Microsoft.AspNetCore.SignalR.StackExchangeRedis.RedisOptions;

namespace OrchardCore.Tests.Modules.OrchardCore.SignalR;

public sealed class SignalRRedisOptionsConfigurationTests
{
    [Fact]
    public void Configure_ChannelPrefix_CombinesInstancePrefixTenantAndSignalRSuffix()
    {
        // Arrange
        var configuration = Configure("MyApp:Production:", "Alpha");

        // Assert
        Assert.Equal("MyApp:Production:Alpha:SignalR", configuration.Configuration.ChannelPrefix.ToString());
    }

    [Fact]
    public void Configure_DifferentTenants_ProduceDifferentChannelPrefixes()
    {
        // Act
        var alpha = Configure("MyApp:Production:", "Alpha");
        var beta = Configure("MyApp:Production:", "Beta");

        // Assert
        Assert.NotEqual(
            alpha.Configuration.ChannelPrefix.ToString(),
            beta.Configuration.ChannelPrefix.ToString());
    }

    [Fact]
    public void Configure_SameTenantOnDifferentNodes_ProducesIdenticalChannelPrefix()
    {
        // Two application nodes serving the same tenant must land on the same channel
        // namespace, otherwise the backplane cannot fan a message out across nodes.

        // Act
        var node1 = Configure("MyApp:Production:", "Alpha");
        var node2 = Configure("MyApp:Production:", "Alpha");

        // Assert
        Assert.Equal(
            node1.Configuration.ChannelPrefix.ToString(),
            node2.Configuration.ChannelPrefix.ToString());
    }

    [Fact]
    public void Configure_SameTenantNameUnderDifferentInstancePrefixes_StaysIsolated()
    {
        // Two independent deployments (for example different environments or regions) that
        // happen to host a tenant with the same shell name must not share hub channels.

        // Act
        var eastUs = Configure("MyApp:Production:EastUS:", "Default");
        var westUs = Configure("MyApp:Production:WestUS:", "Default");

        // Assert
        Assert.NotEqual(
            eastUs.Configuration.ChannelPrefix.ToString(),
            westUs.Configuration.ChannelPrefix.ToString());
    }

    [Theory]
    [InlineData("Alpha", "AlphaBeta")]
    [InlineData("Team", "Team2")]
    public void Configure_TenantNamesThatSharePrefix_DoNotCollide(string firstTenant, string secondTenant)
    {
        // The ":SignalR" suffix anchors the end of the channel, so a shorter tenant name can
        // never be mistaken for the beginning of a longer one.

        // Act
        var first = Configure("MyApp:", firstTenant);
        var second = Configure("MyApp:", secondTenant);

        // Assert
        Assert.NotEqual(
            first.Configuration.ChannelPrefix.ToString(),
            second.Configuration.ChannelPrefix.ToString());
    }

    [Fact]
    public void Configure_ClonesTheSharedRedisConnectionOptions()
    {
        // Arrange
        var sharedConfiguration = new ConfigurationOptions
        {
            ClientName = "shared-orchard-connection",
        };

        var redisOptions = new RedisOptions
        {
            ConfigurationOptions = sharedConfiguration,
            InstancePrefix = "MyApp:",
        };

        var target = new SignalRRedisOptions();

        var sut = new SignalRRedisOptionsConfiguration(
            Options.Create(redisOptions),
            new ShellSettings
            {
                Name = "Alpha",
            });

        // Act
        sut.Configure(target);

        // Assert
        Assert.NotSame(sharedConfiguration, target.Configuration);
        Assert.NotEqual("MyApp:Alpha:SignalR", sharedConfiguration.ChannelPrefix.ToString());
        Assert.Equal("MyApp:Alpha:SignalR", target.Configuration.ChannelPrefix.ToString());
    }

    [Fact]
    public void Configure_EmptyInstancePrefix_StillQualifiesByTenant()
    {
        // Act
        var alpha = Configure(string.Empty, "Alpha");
        var beta = Configure(string.Empty, "Beta");

        // Assert
        Assert.Equal("Alpha:SignalR", alpha.Configuration.ChannelPrefix.ToString());
        Assert.NotEqual(
            alpha.Configuration.ChannelPrefix.ToString(),
            beta.Configuration.ChannelPrefix.ToString());
    }

    private static SignalRRedisOptions Configure(string instancePrefix, string tenantName)
    {
        var redisOptions = new RedisOptions
        {
            ConfigurationOptions = new ConfigurationOptions(),
            InstancePrefix = instancePrefix,
        };

        var sut = new SignalRRedisOptionsConfiguration(
            Options.Create(redisOptions),
            new ShellSettings
            {
                Name = tenantName,
            });

        var target = new SignalRRedisOptions();
        sut.Configure(target);

        return target;
    }
}

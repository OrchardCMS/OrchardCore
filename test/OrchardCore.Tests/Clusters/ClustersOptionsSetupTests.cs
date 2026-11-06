using System.Collections.Generic;
using System.Linq;
using System.Threading;
using global::Microsoft.Extensions.Configuration;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Logging.Abstractions;
using global::Microsoft.Extensions.Options;
using OrchardCore.Clusters;

namespace OrchardCore.Tests.Clusters;

public class ClustersOptionsSetupTests
{
    [Fact]
    public void Monitor_ConfigurationChanges_ReloadsClustersOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore:Clusters:Enabled"] = "false",
                ["OrchardCore:Clusters:MaxIdleTime"] = "01:00:00",
                ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
                ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "16383",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddLogging();
        services.AddSingleton<global::Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton<IConfigureOptions<ClustersOptions>, ClustersOptionsSetup>();
        services.AddSingleton<IOptionsChangeTokenSource<ClustersOptions>>(
            new ConfigurationChangeTokenSource<ClustersOptions>(
                Options.DefaultName,
                configuration.GetSection(ClustersOptions.ConfigurationSection)));

        using var serviceProvider = services.BuildServiceProvider();
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<ClustersOptions>>();
        using var changeToken = new ManualResetEventSlim();
        using var registration = monitor.OnChange((_, _) => changeToken.Set());

        var initialOptions = monitor.CurrentValue;

        Assert.False(initialOptions.Enabled);
        Assert.Equal(TimeSpan.FromHours(1), initialOptions.MaxIdleTime);
        Assert.Empty(initialOptions.Clusters);

        configuration["OrchardCore:Clusters:Enabled"] = "true";
        configuration["OrchardCore:Clusters:ProxySecret"] = "secret";
        configuration["OrchardCore:Clusters:Routes:RouteTemplate:ClusterId"] = "cluster1";
        configuration["OrchardCore:Clusters:MaxIdleTime"] = "00:00:30";
        configuration["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "8191";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:0"] = "8192";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:1"] = "16383";
        configuration.Reload();

        Assert.True(changeToken.Wait(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken));

        var reloadedOptions = monitor.CurrentValue;

        Assert.True(reloadedOptions.Enabled);
        Assert.Equal(TimeSpan.FromSeconds(30), reloadedOptions.MaxIdleTime);
        Assert.Equal(2, reloadedOptions.Clusters.Count);

        Assert.Collection(
            reloadedOptions.Clusters.OrderBy(cluster => cluster.ClusterId),
            cluster =>
            {
                Assert.Equal("cluster1", cluster.ClusterId);
                Assert.Equal(0, cluster.SlotMin);
                Assert.Equal(8191, cluster.SlotMax);
            },
            cluster =>
            {
                Assert.Equal("cluster2", cluster.ClusterId);
                Assert.Equal(8192, cluster.SlotMin);
                Assert.Equal(16383, cluster.SlotMax);
            });
    }

    [Fact]
    public void Configure_EnabledWithoutClusters_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
        }));

        Assert.Equal("Tenant clustering is enabled but no clusters are configured.", exception.Message);
    }

    [Fact]
    public void Configure_SlotRangeWithWrongLength_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
        }));

        Assert.Equal("The tenant cluster 'cluster1' must define a SlotRange with exactly two values.", exception.Message);
    }

    [Theory]
    [InlineData("-1", "100")]
    [InlineData("100", "16384")]
    public void Configure_SlotRangeOutsideAllowedSlots_Throws(string slotMin, string slotMax)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = slotMin,
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = slotMax,
        }));

        Assert.Equal($"The tenant cluster 'cluster1' SlotRange must be within 0 and {ClustersOptions.SlotsCount - 1}.", exception.Message);
    }

    [Fact]
    public void Configure_InvertedSlotRange_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "100",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "99",
        }));

        Assert.Equal("The tenant cluster 'cluster1' SlotRange minimum must be less than or equal to the maximum.", exception.Message);
    }

    [Fact]
    public void Configure_OverlappingSlotRanges_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "100",
            ["OrchardCore:Clusters:Clusters:cluster2:SlotRange:0"] = "100",
            ["OrchardCore:Clusters:Clusters:cluster2:SlotRange:1"] = "200",
        }));

        Assert.Contains("SlotRange overlaps with the tenant cluster", exception.Message);
    }

    [Fact]
    public void Configure_EnabledWithoutRouteTemplate_Throws()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "16383",
        }));

        Assert.Equal("Tenant clustering is enabled but the 'RouteTemplate' route is not configured.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Configure_EnabledWithoutProxySecret_Throws(string proxySecret)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:ProxySecret"] = proxySecret,
            ["OrchardCore:Clusters:Routes:RouteTemplate:ClusterId"] = "cluster1",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "16383",
        }));

        Assert.Equal("Tenant clustering is enabled but the ProxySecret is not configured.", exception.Message);
    }

    [Theory]
    [InlineData("with space")]
    [InlineData("caf\u00e9")]
    [InlineData("line\nbreak")]
    public void Configure_ProxySecretNotValidInHeader_Throws(string proxySecret)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "true",
            ["OrchardCore:Clusters:ProxySecret"] = proxySecret,
            ["OrchardCore:Clusters:Routes:RouteTemplate:ClusterId"] = "cluster1",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "16383",
        }));

        Assert.Equal("The tenant clusters ProxySecret must only contain visible ASCII characters.", exception.Message);
    }

    [Fact]
    public void Configure_DisabledWithoutProxySecret_DoesNotThrow()
    {
        Assert.Null(Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:Enabled"] = "false",
        }).ProxySecret);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void Configure_MaxIdleTimeNotPositive_Throws(string enabled)
    {
        foreach (var maxIdleTime in new[] { "00:00:00", "-00:01:00" })
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
            {
                ["OrchardCore:Clusters:Enabled"] = enabled,
                ["OrchardCore:Clusters:MaxIdleTime"] = maxIdleTime,
            }));

            Assert.Equal("The tenant clusters MaxIdleTime must be greater than zero.", exception.Message);
        }
    }

    [Fact]
    public void Configure_DisabledWithInvalidClusters_DoesNotThrow()
    {
        var options = Configure(new Dictionary<string, string>
        {
            ["OrchardCore:Clusters:MaxIdleTime"] = "00:00:30",
            ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
        });

        Assert.False(options.Enabled);
        Assert.Equal(TimeSpan.FromSeconds(30), options.MaxIdleTime);
        Assert.Empty(options.Clusters);
    }

    [Fact]
    public void Configure_InvalidReload_KeepsLastValidOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore:Clusters:Enabled"] = "true",
                ["OrchardCore:Clusters:ProxySecret"] = "secret",
                ["OrchardCore:Clusters:MaxIdleTime"] = "01:00:00",
                ["OrchardCore:Clusters:Routes:RouteTemplate:ClusterId"] = "cluster1",
                ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:0"] = "0",
                ["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "16383",
            })
            .Build();

        var setup = new ClustersOptionsSetup(configuration, NullLogger<ClustersOptionsSetup>.Instance);
        setup.Configure(new ClustersOptions());

        configuration["OrchardCore:Clusters:MaxIdleTime"] = "00:00:30";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:0"] = "0";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:1"] = "100";

        var options = new ClustersOptions();
        setup.Configure(options);

        Assert.True(options.Enabled);
        Assert.Equal(TimeSpan.FromHours(1), options.MaxIdleTime);
        Assert.Equal("cluster1", Assert.Single(options.Clusters).ClusterId);

        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:0"] = "16384";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:1"] = "16384";
        Assert.Equal("cluster1", Assert.Single(ConfigureWith(setup).Clusters).ClusterId);

        configuration["OrchardCore:Clusters:Clusters:cluster1:SlotRange:1"] = "100";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:0"] = "101";
        configuration["OrchardCore:Clusters:Clusters:cluster2:SlotRange:1"] = "16383";
        Assert.Equal(2, ConfigureWith(setup).Clusters.Count);
    }

    private static ClustersOptions ConfigureWith(ClustersOptionsSetup setup)
    {
        var options = new ClustersOptions();
        setup.Configure(options);

        return options;
    }

    private static ClustersOptions Configure(Dictionary<string, string> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return ConfigureWith(new ClustersOptionsSetup(configuration, NullLogger<ClustersOptionsSetup>.Instance));
    }
}

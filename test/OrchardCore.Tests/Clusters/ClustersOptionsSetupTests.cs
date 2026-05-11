using System.Collections.Generic;
using System.Linq;
using System.Threading;
using global::Microsoft.Extensions.Configuration;
using global::Microsoft.Extensions.DependencyInjection;
using global::Microsoft.Extensions.Options;
using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Clusters;

public class ClustersOptionsSetupTests
{
    [Fact]
    public void MonitorReloadsClustersOptionsWhenConfigurationChanges()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["OrchardCore_Clusters:Enabled"] = "false",
                ["OrchardCore_Clusters:MaxIdleTime"] = "01:00:00",
                ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:0"] = "0",
                ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:1"] = "16383",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddOptions();
        services.AddSingleton<global::Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton<IConfigureOptions<ClustersOptions>, ClustersOptionsSetup>();
        services.AddSingleton<IOptionsChangeTokenSource<ClustersOptions>>(
            new ConfigurationChangeTokenSource<ClustersOptions>(
                Options.DefaultName,
                configuration.GetSection("OrchardCore_Clusters")));

        using var serviceProvider = services.BuildServiceProvider();
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<ClustersOptions>>();
        using var changeToken = new ManualResetEventSlim();
        using var registration = monitor.OnChange((_, _) => changeToken.Set());

        var initialOptions = monitor.CurrentValue;

        Assert.False(initialOptions.Enabled);
        Assert.Equal(TimeSpan.FromHours(1), initialOptions.MaxIdleTime);
        Assert.Single(initialOptions.Clusters);
        Assert.Equal("cluster1", initialOptions.Clusters[0].ClusterId);
        Assert.Equal(0, initialOptions.Clusters[0].SlotMin);
        Assert.Equal(16383, initialOptions.Clusters[0].SlotMax);

        configuration["OrchardCore_Clusters:Enabled"] = "true";
        configuration["OrchardCore_Clusters:MaxIdleTime"] = "00:00:30";
        configuration["OrchardCore_Clusters:Clusters:cluster1:SlotRange:1"] = "8191";
        configuration["OrchardCore_Clusters:Clusters:cluster2:SlotRange:0"] = "8192";
        configuration["OrchardCore_Clusters:Clusters:cluster2:SlotRange:1"] = "16383";
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
    public void ConfigureThrowsWhenEnabledWithoutClusters()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore_Clusters:Enabled"] = "true",
        }));

        Assert.Equal("Tenant clustering is enabled but no clusters are configured.", exception.Message);
    }

    [Fact]
    public void ConfigureThrowsWhenEnabledClusterSlotRangeHasWrongLength()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore_Clusters:Enabled"] = "true",
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:0"] = "0",
        }));

        Assert.Equal("The tenant cluster 'cluster1' must define a SlotRange with exactly two values.", exception.Message);
    }

    [Theory]
    [InlineData("-1", "100")]
    [InlineData("100", "16384")]
    public void ConfigureThrowsWhenEnabledClusterSlotRangeIsOutsideAllowedSlots(string slotMin, string slotMax)
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore_Clusters:Enabled"] = "true",
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:0"] = slotMin,
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:1"] = slotMax,
        }));

        Assert.Equal($"The tenant cluster 'cluster1' SlotRange must be within 0 and {ShellSettings.ClusterSlotsCount - 1}.", exception.Message);
    }

    [Fact]
    public void ConfigureThrowsWhenEnabledClusterSlotRangeIsInverted()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore_Clusters:Enabled"] = "true",
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:0"] = "100",
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:1"] = "99",
        }));

        Assert.Equal("The tenant cluster 'cluster1' SlotRange minimum must be less than or equal to the maximum.", exception.Message);
    }

    [Fact]
    public void ConfigureThrowsWhenEnabledClusterSlotRangesOverlap()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Configure(new Dictionary<string, string>
        {
            ["OrchardCore_Clusters:Enabled"] = "true",
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:0"] = "0",
            ["OrchardCore_Clusters:Clusters:cluster1:SlotRange:1"] = "100",
            ["OrchardCore_Clusters:Clusters:cluster2:SlotRange:0"] = "100",
            ["OrchardCore_Clusters:Clusters:cluster2:SlotRange:1"] = "200",
        }));

        Assert.Contains("SlotRange overlaps with the tenant cluster", exception.Message);
    }

    private static ClustersOptions Configure(Dictionary<string, string> values)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var options = new ClustersOptions();
        var setup = new ClustersOptionsSetup(configuration);

        setup.Configure(options);

        return options;
    }
}

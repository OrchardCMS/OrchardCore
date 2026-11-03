using OrchardCore.Clusters;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tests.Clusters;

public class TenantClusterPlacementTests
{
    [Fact]
    public void Crc16XModem_CheckValue_MatchesAlgorithm()
    {
        Assert.Equal(0x31C3, Crc16XModem.Compute("123456789"));
    }

    [Fact]
    public void GetClusterSlot_NoTenantId_IsNegative()
    {
        Assert.Equal(-1, new ShellSettings { Name = "Tenant" }.GetClusterSlot());
    }

    [Fact]
    public void GetClusterSlot_VersionIdChanges_RemainsStable()
    {
        var settings = CreateSettings();

        var slot = settings.GetClusterSlot();

        settings.VersionId = "second-version";

        // A tenant should not move to another cluster each time its settings are updated.
        Assert.Equal("first-version", settings.TenantId);
        Assert.Equal(slot, settings.GetClusterSlot());
        Assert.Equal(slot, new ShellSettings(settings).GetClusterSlot());
        Assert.InRange(slot, 0, ClustersOptions.SlotsCount - 1);
    }

    [Fact]
    public void GetClusterId_SlotInRange_ReturnsCluster()
    {
        var settings = CreateSettings();
        var options = new ClustersOptions
        {
            Clusters =
            [
                new ClusterOptions { ClusterId = "a", SlotMin = settings.GetClusterSlot() + 1, SlotMax = settings.GetClusterSlot() + 1 },
                new ClusterOptions { ClusterId = "b", SlotMin = settings.GetClusterSlot(), SlotMax = settings.GetClusterSlot() },
            ],
        };

        Assert.Equal("b", settings.GetClusterId(options));
    }

    [Fact]
    public void GetClusterId_SlotInGap_ReturnsNull()
    {
        var settings = CreateSettings();
        var options = new ClustersOptions
        {
            Clusters =
            [
                new ClusterOptions { ClusterId = "a", SlotMin = settings.GetClusterSlot() + 1, SlotMax = settings.GetClusterSlot() + 1 },
            ],
        };

        Assert.Null(settings.GetClusterId(options));
    }

    private static ShellSettings CreateSettings()
    {
        var settings = new ShellSettings { Name = "Tenant" };
        settings.VersionId = "first-version";

        return settings;
    }
}

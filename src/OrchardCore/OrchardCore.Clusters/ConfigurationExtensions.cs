using Microsoft.Extensions.Configuration;

namespace OrchardCore.Clusters;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Checks if tenant clusters are configured to be used.
    /// </summary>
    public static bool UseAsClustersProxy(this IConfiguration configuration) =>
        configuration.GetSection("OrchardCore_Clusters").GetValue<bool>("UseAsProxy");

    /// <summary>
    /// Gets the tenant clusters configuration section.
    /// </summary>
    public static IConfigurationSection GetClustersSection(this IConfiguration configuration) =>
        configuration.GetSection("OrchardCore_Clusters");
}

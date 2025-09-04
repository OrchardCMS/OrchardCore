using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration.Internal;

namespace OrchardCore.Environment.Shell.Configuration;

public class ShellConfigurationSources : IShellConfigurationSources
{
    private readonly string _container;
    private readonly ILogger _logger;

    public ShellConfigurationSources(IOptions<ShellOptions> shellOptions, ILogger<ShellConfigurationSources> logger)
    {
        // e.g., App_Data/Sites
        _container = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, shellOptions.Value.ShellsContainerName);
        Directory.CreateDirectory(_container);
        _logger = logger;
    }

    public Task AddSourcesAsync(string tenant, IConfigurationBuilder builder)
    {
        builder.AddTenantJsonFile(Path.Combine(_container, tenant, "appsettings.json"), optional: true);
        return Task.CompletedTask;
    }

    public async Task SaveAsync(string tenant, IDictionary<string, string> data)
    {
        var tenantFolder = Path.Combine(_container, tenant);
        var appsettings = Path.Combine(tenantFolder, "appsettings.json");

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        IDictionary<string, string> configData;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

        if (File.Exists(appsettings))
        {
            using var streamReader = File.OpenRead(appsettings);
            configData = await JsonConfigurationParser.ParseAsync(streamReader);
        }
        else
        {
            configData = new Dictionary<string, string>();
        }

        foreach (var key in data.Keys)
        {
            if (data[key] is not null)
            {
                configData[key] = data[key];
            }
            else
            {
                configData.Remove(key);
            }
        }

        Directory.CreateDirectory(tenantFolder);

        using var streamWriter = File.Create(appsettings);
        await JsonSerializer.SerializeAsync(streamWriter, configData.ToJsonObject(), JOptions.Indented);
    }

    public Task RemoveAsync(string tenant)
    {
        var tenantFolder = Path.Combine(_container, tenant);
        var appsettings = Path.Combine(tenantFolder, "appsettings.json");

        if (File.Exists(appsettings))
        {
            try
            {
                File.Delete(appsettings);
            }
            catch (IOException ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting the '{AppSettings}' file of tenant '{TenantName}'", appsettings, tenant);
            }
        }

        return Task.CompletedTask;
    }
}

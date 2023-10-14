using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell.Configuration.Internal;

namespace OrchardCore.Environment.Shell.Configuration
{
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
            builder
                .AddTenantJsonFile(Path.Combine(_container, tenant, "appsettings.json"), optional: true);

            return Task.CompletedTask;
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            var tenantFolder = Path.Combine(_container, tenant);
            var appsettings = Path.Combine(tenantFolder, "appsettings.json");

            IDictionary<string, string> configData;
            if (File.Exists(appsettings))
            {
                try
                {
                    using var stream = File.OpenRead(appsettings);
                    configData = JsonConfigurationParser.Parse(stream);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Could not parse the JSON file.", ex);
                }
            }
            else
            {
                configData = new Dictionary<string, string>();
            }

            foreach (var key in data.Keys)
            {
                if (data[key] != null)
                {
                    configData[key] = data[key];
                }
                else
                {
                    configData.Remove(key);
                }
            }

            var jConfiguration = configData.ToJObject();

            Directory.CreateDirectory(tenantFolder);

            using var streamWriter = File.CreateText(appsettings);
            using var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };
            await jConfiguration.WriteToAsync(jsonWriter);
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
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                using var streamReader = File.OpenRead(appsettings);
                configData = JsonConfigurationFileParser.Parse(streamReader);
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

            var configuration = new ConfigurationBuilder()
                .Add(new UpdatableDataProvider(configData))
                .Build();

            using var disposable = configuration as IDisposable;
            var jConfiguration = ConfigToJObject(configuration);

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

        private static JToken ConfigToJObject(IConfiguration configuration)
        {
            JArray jArray = null;
            JObject jObject = null;

            foreach (var child in configuration.GetChildren())
            {
                if (int.TryParse(child.Key, out _))
                {
                    jArray ??= new JArray();
                    if (child.GetChildren().Any())
                    {
                        jArray.Add(ConfigToJObject(child));
                    }
                    else
                    {
                        jArray.Add(child.Value);
                    }
                }
                else
                {
                    jObject ??= new JObject();
                    if (child.GetChildren().Any())
                    {
                        jObject.Add(child.Key, ConfigToJObject(child));
                    }
                    else
                    {
                        jObject.Add(child.Key, child.Value);
                    }
                }
            }

            if (jArray is not null)
            {
                if (configuration is IConfigurationRoot)
                {
                    throw new InvalidOperationException("Can't define an array from the root.");
                }

                if (jObject is not null)
                {
                    throw new InvalidOperationException("Can't use a numeric key inside an object.");
                }

                return jArray;
            }

            return jObject;
        }
    }
}

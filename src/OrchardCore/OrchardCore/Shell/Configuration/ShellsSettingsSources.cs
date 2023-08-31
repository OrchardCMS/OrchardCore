using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell.Configuration
{
    public class ShellsSettingsSources : IShellsSettingsSources
    {
        private readonly string _tenants;

        public ShellsSettingsSources(IOptions<ShellOptions> shellOptions)
        {
            _tenants = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "tenants.json");
        }

        public Task AddSourcesAsync(IConfigurationBuilder builder)
        {
            builder.AddJsonFile(_tenants, optional: true);
            return Task.CompletedTask;
        }

        public Task AddSourcesAsync(string tenant, IConfigurationBuilder builder) => AddSourcesAsync(builder);

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            JObject tenantsSettings;
            if (File.Exists(_tenants))
            {
                using var streamReader = File.OpenText(_tenants);
                using var jsonReader = new JsonTextReader(streamReader);
                tenantsSettings = await JObject.LoadAsync(jsonReader);
            }
            else
            {
                tenantsSettings = new JObject();
            }

            var settings = tenantsSettings.GetValue(tenant) as JObject ?? new JObject();

            foreach (var key in data.Keys)
            {
                if (data[key] != null)
                {
                    settings[key] = data[key];
                }
                else
                {
                    settings.Remove(key);
                }
            }

            tenantsSettings[tenant] = settings;

            using var streamWriter = File.CreateText(_tenants);
            using var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };
            await tenantsSettings.WriteToAsync(jsonWriter);
        }

        public async Task RemoveAsync(string tenant)
        {
            if (File.Exists(_tenants))
            {
                JObject tenantsSettings;
                using (var streamReader = File.OpenText(_tenants))
                {
                    using var jsonReader = new JsonTextReader(streamReader);
                    tenantsSettings = await JObject.LoadAsync(jsonReader);
                }

                tenantsSettings.Remove(tenant);

                using var streamWriter = File.CreateText(_tenants);
                using var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };
                await tenantsSettings.WriteToAsync(jsonWriter);
            }
        }
    }
}

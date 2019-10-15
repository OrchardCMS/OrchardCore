using System.Collections.Generic;
using System.IO;
using System.Threading;
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

        // This could be optimized by locking per tenant, but worst case is that 
        // two tenants are blocking each other when the settings are updated. Theorically,
        // as this storing settings in files, this is supposed to be used with few tenants.
        // So we are not optimizing with a distinct semarphore per tenant. 

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public ShellsSettingsSources(IOptions<ShellOptions> shellOptions)
        {
            _tenants = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "tenants.json");
        }

        public void AddSources(IConfigurationBuilder builder)
        {
            builder.AddJsonFile(_tenants, optional: true);
        }

        public async Task SaveAsync(string tenant, IDictionary<string, string> data)
        {
            await _semaphore.WaitAsync();

            try
            {
                JObject tenantsSettings;
                if (File.Exists(_tenants))
                {
                    using (var file = File.OpenText(_tenants))
                    {
                        using (var reader = new JsonTextReader(file))
                        {
                            tenantsSettings = await JObject.LoadAsync(reader);
                        }
                    }
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

                using (var file = File.CreateText(_tenants))
                {
                    using (var writer = new JsonTextWriter(file) { Formatting = Formatting.Indented })
                    {
                        await tenantsSettings.WriteToAsync(writer);
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}

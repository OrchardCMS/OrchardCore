using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell.Configuration
{
    public class ShellSettingsSources : IShellSettingsSources
    {
        private readonly string _tenants;

        public ShellSettingsSources(IOptions<ShellOptions> shellOptions)
        {
            _tenants = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "tenants.json");
        }

        public void AddSources(IConfigurationBuilder builder)
        {
            builder.AddJsonFile(_tenants, optional: true);
        }

        public void Save(string tenant, IDictionary<string, string> data)
        {
            lock (this)
            {
                var tenantsObject = !File.Exists(_tenants) ? new JObject()
                : JObject.Parse(File.ReadAllText(_tenants));

                tenantsObject[tenant] = JObject.FromObject(data);
                File.WriteAllText(_tenants, tenantsObject.ToString());
            }
        }
    }
}
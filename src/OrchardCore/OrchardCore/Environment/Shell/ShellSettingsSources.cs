using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public class ShellSettingsSources : IShellSettingsSources
    {
        private readonly string _tenantsFilePath;

        public ShellSettingsSources(IOptions<ShellOptions> shellOptions)
        {
            _tenantsFilePath = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, "tenants.json");
        }

        public void AddSources(IConfigurationBuilder builder)
        {
            builder.AddJsonFile(_tenantsFilePath, optional: true);
        }

        public void Save(string tenant, IDictionary<string, string> data)
        {
            lock (this)
            {
                var tenantsObject = !File.Exists(_tenantsFilePath) ? new JObject()
                : JObject.Parse(File.ReadAllText(_tenantsFilePath));

                tenantsObject[tenant] = JObject.FromObject(data);
                File.WriteAllText(_tenantsFilePath, tenantsObject.ToString());
            }
        }
    }
}
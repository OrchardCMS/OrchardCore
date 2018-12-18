using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants.Services
{
    public class TenantsConfigurationSource : ITenantsLocalConfigurationSource
    {
        private readonly TenantsManager _tenantsManager;

        public TenantsConfigurationSource(TenantsManager tenantsManager)
        {
            _tenantsManager = tenantsManager;
        }

        public int Order => 100;

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new TenantsConfigurationProvider(_tenantsManager);
        }

        public void SaveSettings(string name, JObject settings)
        {
            _tenantsManager.UpdateAsync(name, settings).GetAwaiter().GetResult();
        }
    }
}
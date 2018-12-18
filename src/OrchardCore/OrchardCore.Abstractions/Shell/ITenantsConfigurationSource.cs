using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public interface ITenantsGlobalConfigurationSource : IConfigurationSource
    {
        int Order { get; }
    }

    public interface ITenantsLocalConfigurationSource : IConfigurationSource
    {
        int Order { get; }
        void SaveSettings(string name, JObject settings);
    }
}

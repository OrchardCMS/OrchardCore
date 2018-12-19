using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Environment.Shell
{
    public interface ITenantsConfigurationSource : IConfigurationSource
    {
        int Order { get; }
    }
}

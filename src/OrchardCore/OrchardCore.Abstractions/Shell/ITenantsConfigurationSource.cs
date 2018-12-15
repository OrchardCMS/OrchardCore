using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public interface ITenantsConfigurationSource : IConfigurationSource
    {
        int Order { get; }
    }
}

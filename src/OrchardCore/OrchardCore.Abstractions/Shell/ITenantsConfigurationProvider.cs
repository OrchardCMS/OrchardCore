using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public interface ITenantsConfigurationProvider
    {
        IConfiguration Configuration { get; }
        int Order { get; }
    }
}

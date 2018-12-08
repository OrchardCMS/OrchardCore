using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public interface IShellSettingsConfigurationProvider
    {
        void Configure(string tenantName, IConfigurationBuilder configurationBuilder);

        int Order { get; }
    }
}

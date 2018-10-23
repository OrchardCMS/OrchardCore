using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public interface IShellSettingsConfigurationProvider
    {
        void AddSource(IConfigurationBuilder builder);
        void AddSource(IConfigurationBuilder builder, string name);
        void SaveToSource(string name, IDictionary<string, string> configuration);

        int Order { get; }
    }
}

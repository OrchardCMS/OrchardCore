using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public interface IShellConfigurationSources
    {
        void AddSources(IConfigurationBuilder builder);
        void AddSources(string tenant, IConfigurationBuilder builder);
        void Save(string tenant, IDictionary<string, string> data);
    }
}

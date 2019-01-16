using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell
{
    public interface IShellSettingsSources
    {
        void AddSources(IConfigurationBuilder builder);
        void Save(string tenant, IDictionary<string, string> data);
    }
}

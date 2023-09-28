using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell.Configuration.Internal
{
    internal class EmptyConfigurationRoot : IConfigurationRoot
    {
        public string this[string key]
        {
            get => string.Empty;
            set { }
        }

        public IEnumerable<IConfigurationProvider> Providers => Enumerable.Empty<IConfigurationProvider>();

        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();

        public IChangeToken GetReloadToken() => NullChangeToken.Singleton;

        public IConfigurationSection GetSection(string key) => new ConfigurationSection(this, key);

        public void Reload() { }
    }
}

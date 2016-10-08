using Microsoft.Extensions.Configuration;
using Orchard.Environment.Extensions.Info.Manifests;

namespace Orchard.Environment.Extensions.Info
{
    public interface IManifestProvider
    {
        int Priority { get; }
        IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string subPath);
    }
}

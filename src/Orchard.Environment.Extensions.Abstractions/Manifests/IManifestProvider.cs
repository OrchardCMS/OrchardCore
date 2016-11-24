using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Manifests
{
    public interface IManifestProvider
    {
        int Priority { get; }
        IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string subPath);
    }
}

using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public interface IManifestProvider
    {
        int Priority { get; }
        IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string subPath);
    }
}

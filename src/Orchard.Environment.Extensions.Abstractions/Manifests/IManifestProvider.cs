using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Manifests
{
    public interface IManifestProvider
    {
        int Order { get; }
        IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string filePath);
    }
}

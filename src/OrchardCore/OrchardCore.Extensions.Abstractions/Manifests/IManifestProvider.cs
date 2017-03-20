using Microsoft.Extensions.Configuration;

namespace OrchardCore.Extensions.Manifests
{
    public interface IManifestProvider
    {
        int Order { get; }
        IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string filePath);
    }
}

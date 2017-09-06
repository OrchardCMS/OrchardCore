using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Extensions.Manifests
{
    public interface IManifestProvider
    {
        int Order { get; }
        IConfigurationBuilder GetManifestConfiguration(IConfigurationBuilder configurationBuilder, string filePath);
    }
}

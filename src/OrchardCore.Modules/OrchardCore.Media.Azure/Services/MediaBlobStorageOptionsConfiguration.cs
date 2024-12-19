using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AzureBlob;

namespace OrchardCore.Media.Azure.Services;

internal sealed class MediaBlobStorageOptionsConfiguration : BlobStorageOptionsConfiguration<MediaBlobStorageOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public MediaBlobStorageOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<MediaBlobStorageOptionsConfiguration> logger)
        : base(shellSettings, logger)
    {
        _shellConfiguration = shellConfiguration;
    }

    protected override MediaBlobStorageOptions GetRawOptions()
        => _shellConfiguration.GetSection("OrchardCore_Media_Azure")
        .Get<MediaBlobStorageOptions>();
}

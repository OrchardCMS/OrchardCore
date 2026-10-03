using Fluid;
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
        FluidParser fluidParser,
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<MediaBlobStorageOptionsConfiguration> logger)
        : base(fluidParser, shellSettings, logger)
    {
        _shellConfiguration = shellConfiguration;
    }

    // The 'OrchardCore_Media_Azure' section is deprecated and will be removed in a future major version, use 'Media:Azure' instead.
    protected override MediaBlobStorageOptions GetRawOptions()
        => _shellConfiguration.GetSectionCompat("Media:Azure", "OrchardCore_Media_Azure")
        .Get<MediaBlobStorageOptions>();

    protected override void FurtherConfigure(MediaBlobStorageOptions rawOptions, MediaBlobStorageOptions options)
    {
        options.CreateContainer = rawOptions.CreateContainer;
        options.RemoveContainer = rawOptions.RemoveContainer;
        options.RemoveFilesFromBasePath = rawOptions.RemoveFilesFromBasePath;
        options.UseHierarchicalNamespace = rawOptions.UseHierarchicalNamespace;
    }
}

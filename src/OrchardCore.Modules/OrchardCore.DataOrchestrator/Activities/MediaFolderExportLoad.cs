using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.Models;
using OrchardCore.FileStorage;
using OrchardCore.Media;

namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Exports the pipeline data to a file in the tenant's media library using the selected format.
/// </summary>
public sealed class MediaFolderExportLoad : EtlFileExportLoad
{
    public override string Name => nameof(MediaFolderExportLoad);

    public override string DisplayText => "Media Folder Export";

    protected override async Task WriteToDestinationAsync(EtlExecutionContext context, string fileName, Stream content, IEtlExportFormat format)
    {
        var mediaFileStore = context.ServiceProvider.GetService<IMediaFileStore>()
            ?? throw new InvalidOperationException("No IMediaFileStore service is available for the media folder destination.");

        var path = mediaFileStore.NormalizePath(fileName);

        await mediaFileStore.CreateFileFromStreamAsync(path, content, overwrite: true);
    }
}

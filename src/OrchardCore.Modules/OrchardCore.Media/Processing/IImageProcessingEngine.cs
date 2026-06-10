using OrchardCore.Media.Models;

namespace OrchardCore.Media.Processing;

internal interface IImageProcessingEngine
{
    Task<ImageProcessingResult> ProcessAsync(Stream input, MediaCommands commands, CancellationToken cancellationToken = default);
}

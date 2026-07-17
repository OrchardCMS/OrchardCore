namespace OrchardCore.Media.Processing;

/// <summary>
/// Transforms images on demand for the media pipeline. Implementations wrap a concrete imaging
/// library (for example NetVips or ImageSharp); the media module ships a NetVips implementation by
/// default and a site can replace it by registering a different <see cref="IImageProcessingEngine"/>.
/// </summary>
public interface IImageProcessingEngine
{
    /// <summary>
    /// Processes the source image according to the given commands and returns the encoded result.
    /// </summary>
    /// <param name="input">The source image stream. The caller owns and disposes this stream.</param>
    /// <param name="commands">The parsed, engine-agnostic transform instructions.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="ImageProcessingResult"/> whose <see cref="ImageProcessingResult.Output"/> stream
    /// the caller owns and must dispose.
    /// </returns>
    Task<ImageProcessingResult> ProcessAsync(Stream input, ImageProcessingCommands commands, CancellationToken cancellationToken = default);
}

using System.IO;

namespace OrchardCore.Media
{
    /// <summary>
    /// Represents an abstraction over a image service, implement it in your module.
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Transform a image by the transformation settings.
        /// </summary>
        /// <param name="stream">The image stream.</param>
        /// <param name="options">The transformation settings, can be any stuff because it is dynamic.</param>
        /// <returns>A new image stream with its dimension.</returns>
        public OutputImage TransformImageStream(Stream stream, dynamic options);
    }
}

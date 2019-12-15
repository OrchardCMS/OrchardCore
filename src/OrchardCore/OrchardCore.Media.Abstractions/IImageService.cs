using System.IO;

namespace OrchardCore.Media
{
    public interface IImageService
    {
        public OutputImage TransformImageStream(Stream stream, dynamic options);
    }
}

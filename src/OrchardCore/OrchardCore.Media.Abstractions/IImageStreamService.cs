using System.IO;

namespace OrchardCore.Media
{
    public interface IImageStreamService
    {
        OutputImage ProcessStream(Stream inputStream, ImageOptions options = null);
    }
}

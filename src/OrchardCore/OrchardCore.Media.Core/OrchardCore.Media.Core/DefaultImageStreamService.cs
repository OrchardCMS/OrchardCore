using System.IO;

namespace OrchardCore.Media.Core
{
    public class DefaultImageStreamService : IImageStreamService
    {
        private readonly IImageService _imageService;
        public DefaultImageStreamService(IImageService imageService)
        {
            _imageService = imageService;
        }
        public OutputImage ProcessStream(Stream inputStream, ImageOptions parameter = null)
        {
            MemoryStream outputStream = new MemoryStream();
            OutputImage outputImage = null;
            int destinationWidth = 0, destinationHeight = 0;

            if (parameter == null || !parameter.NeedTransformImage)
            {
                outputImage.Dimension = new ImageDimension(destinationWidth, destinationHeight);
                outputImage.Stream = inputStream;
                return outputImage;
            }

            outputImage = _imageService.TransformImageStream(inputStream, parameter.Options);
            return outputImage;
        }


    }
}



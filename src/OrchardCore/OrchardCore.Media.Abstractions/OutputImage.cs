using System.IO;

namespace OrchardCore.Media
{
    public class OutputImage
    {
        /// <summary>
        /// The stream of new image.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// The dimension of new image.
        /// </summary>
        public ImageDimension Dimension { get; set; }
    }
}

using System.IO;

namespace OrchardCore.Media
{
    public class OutputStream
    {
        /// <summary>
        /// The output stream.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// The width of new image.
        /// </summary>
        public int? Width { get; set; }

        /// <summary>
        /// The height of new image.
        /// </summary>
        public int? Height { get; set; }
    }
}

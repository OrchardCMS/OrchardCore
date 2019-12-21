using System.IO;

namespace OrchardCore.Media.Events
{
    public class MediaContext
    {       
        public string Path { get; set; }
        public Stream Stream { get; set; }
        /// <summary>
        /// Any dynamic stuff used for transforming the image.
        /// </summary>
        public dynamic Options { get; set; }
    }
}

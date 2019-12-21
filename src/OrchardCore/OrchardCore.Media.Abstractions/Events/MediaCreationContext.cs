using System.IO;

namespace OrchardCore.Media.Events
{
    public class MediaCreationContext : MediaContext
    {
        /// <summary>
        /// The input or output stream.
        /// </summary>
        public Stream Stream { get; set; }       
        public int? X { get; set; }
        public int? Y { get; set; }
        /// <summary>
        /// The width of input stream.
        /// </summary>
        public int? InputWidth { get; set; }

        /// <summary>
        /// The height of input stream..
        /// </summary>
        public int? InputHeight { get; set; }

        /// <summary>
        /// The width of output stream.
        /// </summary>
        public int? OutputWidth { get; set; }

        /// <summary>
        /// The height of output stream.
        /// </summary>
        public int? OutputHeight { get; set; }
        
    }
}

using System.IO;

namespace OrchardCore.Media.Events
{
    /// <summary>
    /// The media context for the event of creating file.
    /// </summary>
    public class MediaCreationContext : MediaContext
    {
        /// <summary>
        /// The input or output stream dependent to the situation.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// The X position of input stream.
        /// </summary>
        public int? X { get; set; }

        /// <summary>
        /// The Y position of input stream.
        /// </summary>
        public int? Y { get; set; }

        /// <summary>
        /// The width of input stream.
        /// </summary>
        public int? InputWidth { get; set; }

        /// <summary>
        /// The height of input stream.
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

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
    }
}

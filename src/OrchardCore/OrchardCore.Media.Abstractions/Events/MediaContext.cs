using System.Security.Claims;

namespace OrchardCore.Media.Events
{
    /// <summary>
    /// The media context
    /// </summary>
    public class MediaContext
    {
        /// <summary>
        /// The file path when this media saved to the application.
        /// </summary>
        public string Path { get; set; }        
        
        /// <summary>
        /// Any dynamic stuff used for transforming the image.
        /// </summary>
        public dynamic Data { get; set; }
    }
}

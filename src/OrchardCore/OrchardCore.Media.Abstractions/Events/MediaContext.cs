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
        /// Who process this media.
        /// </summary>
        public ClaimsPrincipal User { get; set; }

        /// <summary>
        /// Is this media need to be preprocessed?
        /// </summary>
        public bool NeedToBePreprocessed { get; set; }

        /// <summary>
        /// Is this media need to be postprocessed?
        /// </summary>
        public bool NeedToBePostprocessed { get; set; }
        /// <summary>
        /// Any dynamic stuff used for transforming the image.
        /// </summary>
        public dynamic Data { get; set; }
    }
}

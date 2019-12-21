using System.Security.Claims;

namespace OrchardCore.Media.Events
{
    public class MediaContext
    {
        public string Path { get; set; }
        public ClaimsPrincipal User { get; set; }
        public bool NeedPreProcess { get; set; }
        public bool NeedPostProcess { get; set; }
        /// <summary>
        /// Any dynamic stuff used for transforming the image.
        /// </summary>
        public dynamic Data { get; set; }
    }
}

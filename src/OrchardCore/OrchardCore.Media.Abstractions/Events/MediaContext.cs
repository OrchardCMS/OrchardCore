using System.Security.Claims;

namespace OrchardCore.Media.Events
{
    public class MediaContext
    {
        public string Path { get; set; }
        public ClaimsPrincipal User { get; set; }
        public bool NeedPreProcess { get; set; }
        public bool NeedPostProcess { get; set; }
    }
}

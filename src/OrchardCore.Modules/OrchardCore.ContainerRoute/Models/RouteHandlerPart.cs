using OrchardCore.ContentManagement;

namespace OrchardCore.ContainerRoute.Models
{
    public class RouteHandlerPart : ContentPart
    {
        /// <summary>
        /// Route path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether route is relative to parents route.
        /// </summary>
        public bool IsRelative { get; set; } = true;

        /// <summary>
        /// Whether this contained content item can be routed to.
        /// </summary>
        public bool IsRoutable { get; set; } = true;
    }
}

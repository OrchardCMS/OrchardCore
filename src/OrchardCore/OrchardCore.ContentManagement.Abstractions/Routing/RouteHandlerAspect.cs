namespace OrchardCore.ContentManagement.Routing
{
    public class RouteHandlerAspect
    {
        /// <summary>
        /// Route path.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Whether route is absolute to the parent content item route.
        /// </summary>
        public bool Absolute { get; set; }

        /// <summary>
        /// Whether this contained content item can be routed to.
        /// </summary>
        public bool Disabled { get; set; }
    }
}

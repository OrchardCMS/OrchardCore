namespace OrchardCore.ContainerRoute.Routing
{
    public class RouteHandlerAspect
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
        public bool IsRouteable { get; set; } = true;
    }
}

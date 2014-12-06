using Microsoft.AspNet.Routing;

namespace OrchardVNext.Mvc.Routes {
    public class RouteDescriptor {
        public string Name { get; set; }
        public int Priority { get; set; }
        public string Prefix { get; set; }
        public Route Route { get; set; }
    }

    public class HttpRouteDescriptor : RouteDescriptor {
        public string RouteTemplate { get; set; }
        public object Defaults { get; set; }
        public object Constraints { get; set; }
    }
}
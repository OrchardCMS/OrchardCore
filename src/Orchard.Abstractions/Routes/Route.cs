using Microsoft.AspNetCore.Routing;

namespace Orchard.Routes
{
    public class Route
    {
        public Route(
            string routeName,
            string routeTemplate,
            object defaults = null,
            object constraints = null,
            object dataTokens = null)
        {
            RouteName = routeName;
            RouteTemplate = routeTemplate;
            Defaults = new RouteValueDictionary(defaults);
            Constraints = new RouteValueDictionary(constraints);
            DataTokens = new RouteValueDictionary(dataTokens);
        }

        public string RouteName { get; private set; }
        public string RouteTemplate { get; private set; }
        public RouteValueDictionary Defaults { get; private set; }
        public RouteValueDictionary Constraints { get; private set; }
        public RouteValueDictionary DataTokens { get; private set; }
    }
}
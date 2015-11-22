using System.Collections.Generic;
using Microsoft.AspNet.Routing;

namespace Orchard.Hosting.Web.Routing.Routes
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
            Defaults = ObjectToDictionary(defaults);
            Constraints = ObjectToDictionary(constraints);
            DataTokens = ObjectToDictionary(dataTokens);
        }

        public string RouteName { get; private set; }
        public string RouteTemplate { get; private set; }
        public IDictionary<string, object> Defaults { get; private set; }
        public IDictionary<string, object> Constraints { get; private set; }
        public IDictionary<string, object> DataTokens { get; private set; }

        private static IDictionary<string, object> ObjectToDictionary(object value)
        {
            var dictionary = value as IDictionary<string, object>;
            if (dictionary != null)
            {
                return dictionary;
            }

            return new RouteValueDictionary(value);
        }
    }
}
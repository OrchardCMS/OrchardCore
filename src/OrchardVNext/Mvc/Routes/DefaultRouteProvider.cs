namespace OrchardVNext.Mvc.Routes {
    //public class DefaultRouteProvider : IRouteProvider {
    //    public IEnumerable<RouteDescriptor> GetRoutes() {
    //        return new[] {
    //                         new RouteDescriptor {                                                     
    //                                                 Priority = -20,
    //                                                 Route = new Route(
    //                                                     "{controller}/{action}/{id}",
    //                                                     new RouteValueDictionary {
    //                                                                                  {"controller", "home"},
    //                                                                                  {"action", "index"},
    //                                                                                  {"id", ""},
    //                                                                              },
    //                                                     new RouteValueDictionary {
    //                                                                                  {"controller", new HomeOrAccount()}
    //                                                                              },
    //                                                     new MvcRouteHandler())
    //                                             }
    //                     };
    //    }

    //    //TEMP: this is hardcoded to allow base web app controllers to pass
    //    public class HomeOrAccount : IRouteConstraint {
    //        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
    //            object value;
    //            if (values.TryGetValue(parameterName, out value)) {
    //                var parameterValue = Convert.ToString(value);
    //                return string.Equals(parameterValue, "home", StringComparison.OrdinalIgnoreCase) ||
    //                       string.Equals(parameterValue, "account", StringComparison.OrdinalIgnoreCase);
    //            }
    //            return false;
    //        }
    //    }
    //}
}
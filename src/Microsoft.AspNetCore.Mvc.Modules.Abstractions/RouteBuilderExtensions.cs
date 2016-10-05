using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class RouteBuilderExtensions
    {
        // TODO: Remove when https://github.com/aspnet/Mvc/issues/4846 is fixed (1.1.0)
        public static IRouteBuilder MapAreaRoute(this IRouteBuilder routeBuilder,
            string name,
            string area,
            string template,
            string controller,
            string action)
        {
            if (routeBuilder == null)
            {
                throw new ArgumentNullException(nameof(routeBuilder));
            }

            if (string.IsNullOrEmpty(area))
            {
                throw new ArgumentException(nameof(area));
            }

            var defaultsDictionary = new RouteValueDictionary();
            defaultsDictionary["area"] = area;
            defaultsDictionary["controller"] = controller;
            defaultsDictionary["action"] = action;

            var constraintsDictionary = new RouteValueDictionary();
            constraintsDictionary["area"] = new StringRouteConstraint(area);

            routeBuilder.MapRoute(name, template, defaultsDictionary, constraintsDictionary, null);
            return routeBuilder;
        }

        private class StringRouteConstraint : IRouteConstraint
        {
            private readonly string _value;

            public StringRouteConstraint(string value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _value = value;
            }

            public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
            {
                object routeValue;

                if (values.TryGetValue(routeKey, out routeValue) && routeValue != null)
                {
                    var parameterValueString = Convert.ToString(routeValue, CultureInfo.InvariantCulture);

                    return _value.Equals(parameterValueString, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
        }
    }
}

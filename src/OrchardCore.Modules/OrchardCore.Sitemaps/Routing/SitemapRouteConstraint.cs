using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapRouteConstraint : IRouteConstraint
    {
        public static string RouteKey = "sitemaps";

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var sitemapPath = values[routeKey]?.ToString();
            if (String.IsNullOrEmpty(sitemapPath))
                return false;

            //match end of path for .xml
            var match = new Regex(@".+\.xml$").Match(sitemapPath);

            if (!match.Success)
                return false;

            //might be for us
            var sitemapRoute = httpContext.RequestServices.GetService<ISitemapRoute>();

            //hmm should this be an IRouter that supports async?
            return sitemapRoute.MatchSitemapRouteAsync(sitemapPath).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}

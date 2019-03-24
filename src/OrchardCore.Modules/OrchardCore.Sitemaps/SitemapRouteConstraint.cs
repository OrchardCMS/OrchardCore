using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps
{
    public class SitemapRouteConstraint : IRouteConstraint
    {

        public ILogger Logger { get; set; }

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            Logger = httpContext.RequestServices.GetRequiredService<ILogger<SitemapRouteConstraint>>();

            //match for xml first, saves building the route hashset until we need it
            //TODO validation on path form for .xml
            Logger.LogDebug($"Dumping routekey {routeKey}");
            foreach (var value in values)
            {
                Logger.LogDebug($"Dumping RouteValueDictionary Key: {value.Key} Value: {value.Value}");
            }

            var sitemapPath = values[routeKey]?.ToString();
            if (String.IsNullOrEmpty(sitemapPath))
                return false;

            //match end of path for .xml
            var match = new Regex(@".+\.xml$").Match(sitemapPath);

            Logger.LogDebug($"Dumping match {match.Success}");

            if (!match.Success)
                return false;

            //might be for us
            var sitemapSetService = httpContext.RequestServices.GetService<ISitemapSetService>();

            //hmm should this be an IRouter that supports async?
            return sitemapSetService.MatchSitemapRouteAsync(sitemapPath).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}

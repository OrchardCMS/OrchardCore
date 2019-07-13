using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Layers.Services
{
    public class DefaultLayersMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod[] _allMethods;

        public DefaultLayersMethodProvider()
        {
            var isHomepageMethod = new GlobalMethod
            {
                Name = "isHomepage",
                Method = serviceProvider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var requestPath = httpContext.Request.Path;
                    return BooleanBoxes.Box(requestPath == "/" || string.IsNullOrEmpty(requestPath));
                })
            };

            var isAnonymousMethod = new GlobalMethod
            {
                Name = "isAnonymous",
                Method = serviceProvider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return BooleanBoxes.Box(!httpContext.User?.Identity.IsAuthenticated);
                })
            };

            var isAuthenticatedMethod = new GlobalMethod
            {
                Name = "isAuthenticated",
                Method = serviceProvider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return BooleanBoxes.Box(httpContext.User?.Identity.IsAuthenticated);
                })
            };

            var urlMethod = new GlobalMethod
            {
                Name = "url",
                Method = serviceProvider => (Func<string, object>)(url =>
                {
                    if (url.StartsWith("~/"))
                        url = url.Substring(1);

                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    string requestPath = httpContext.Request.Path;

                    // Tenant home page could have an empty string as a request path, where
                    // the default tenant does not.
                    if (string.IsNullOrEmpty(requestPath))
                        requestPath = "/";

                    var result = url.EndsWith("*")
                        ? requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
                        : string.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase);

                    return BooleanBoxes.Box(result);
                })
            };

            var cultureMethod = new GlobalMethod
            {
                Name = "culture",
                Method = serviceProvider => (Func<string, object>)(culture =>
                {
                    var currentCulture = CultureInfo.CurrentCulture;

                    var result = string.Equals(culture, currentCulture.Name, StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(culture, currentCulture.Parent.Name, StringComparison.OrdinalIgnoreCase);

                    return BooleanBoxes.Box(result);
                })
            };

            _allMethods = new [] { isAnonymousMethod, isAuthenticatedMethod, isHomepageMethod, urlMethod, cultureMethod };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return _allMethods;
        }
    }
}

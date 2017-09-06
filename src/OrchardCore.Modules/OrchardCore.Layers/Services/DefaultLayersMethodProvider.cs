using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Scripting;

namespace OrchardCore.Layers.Services
{
    public class DefaultLayersMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _isHomepage;
        private readonly GlobalMethod _isAnonymous;
        private readonly GlobalMethod _isAuthenticated;
        private readonly GlobalMethod _url;

        private readonly GlobalMethod[] _allMethods;

        public DefaultLayersMethodProvider()
        {
            _isHomepage = new GlobalMethod
            {
                Name = "isHomepage",
                Method = serviceprovider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceprovider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var requestPath = httpContext.Request.Path;
                    return requestPath == "/" || string.IsNullOrEmpty(requestPath);
                })
            };

            _isAnonymous = new GlobalMethod
            {
                Name = "isAnonymous",
                Method = serviceprovider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceprovider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return !httpContext.User?.Identity.IsAuthenticated;
                })
            };
            
            _isAuthenticated = new GlobalMethod
            {
                Name = "isAuthenticated",
                Method = serviceprovider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceprovider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return httpContext.User?.Identity.IsAuthenticated;
                })
            };

            _url = new GlobalMethod
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

                    return url.EndsWith("*")
                        ? requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
                        : string.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase); ;
                })
            };
            
            _allMethods = new [] { _isAnonymous, _isAuthenticated, _isHomepage, _url };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return _allMethods;
        }
    }
}

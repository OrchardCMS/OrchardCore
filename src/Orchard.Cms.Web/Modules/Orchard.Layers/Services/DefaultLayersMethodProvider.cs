using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Scripting;

namespace Orchard.Layers.Services
{
    public class DefaultLayersMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _isHomepage;
        private readonly GlobalMethod _isAnonymous;
        private readonly GlobalMethod _isAuthenticated;

        private readonly GlobalMethod[] _allMethods;

        public DefaultLayersMethodProvider()
        {
            _isHomepage = new GlobalMethod
            {
                Name = "isHomepage",
                Method = serviceprovider => (Func<string, object>)(name =>
                {
                    var httpContext = serviceprovider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return httpContext.Request.Path == "/";
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

            _allMethods = new [] { _isAnonymous, _isAuthenticated, _isHomepage };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return _allMethods;
        }
    }
}

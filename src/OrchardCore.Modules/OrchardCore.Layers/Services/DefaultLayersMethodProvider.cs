using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Scripting;

namespace OrchardCore.Layers.Services
{
    public class DefaultLayersMethodProvider : IGlobalMethodProvider
    {
        private readonly GlobalMethod _isHomepage;
        private readonly GlobalMethod _isAnonymous;
        private readonly GlobalMethod _isAuthenticated;
        private readonly GlobalMethod _isInRole;
        private readonly GlobalMethod _url;
        private readonly GlobalMethod _culture;

        private readonly GlobalMethod[] _allMethods;

        public DefaultLayersMethodProvider()
        {
            _isHomepage = new GlobalMethod
            {
                Name = "isHomepage",
                Method = serviceProvider => (Func<bool>)(() =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var requestPath = httpContext.Request.Path.Value;
                    return requestPath == "/" || String.IsNullOrEmpty(requestPath);
                }),
            };

            _isAnonymous = new GlobalMethod
            {
                Name = "isAnonymous",
                Method = serviceProvider => (Func<bool>)(() =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return httpContext.User?.Identity.IsAuthenticated != true;
                }),
            };

            _isAuthenticated = new GlobalMethod
            {
                Name = "isAuthenticated",
                Method = serviceProvider => (Func<bool>)(() =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    return httpContext.User?.Identity.IsAuthenticated == true;
                }),
            };

            _isInRole = new GlobalMethod
            {
                Name = "isInRole",
                Method = serviceProvider => (Func<string, bool>)(role =>
                {
                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var optionsAccessor = serviceProvider.GetRequiredService<IOptions<IdentityOptions>>();
                    var roleClaimType = optionsAccessor.Value.ClaimsIdentity.RoleClaimType;

                    // IsInRole() & HasClaim() are case sensitive.
                    return httpContext.User?.Claims.Any(claim => claim.Type == roleClaimType && claim.Value.Equals(role, StringComparison.OrdinalIgnoreCase)) == true;
                }),
            };

            _url = new GlobalMethod
            {
                Name = "url",
                Method = serviceProvider => (Func<string, bool>)(url =>
                {
                    if (url.StartsWith("~/", StringComparison.Ordinal))
                    {
                        url = url[1..];
                    }

                    var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
                    var requestPath = httpContext.Request.Path.Value;

                    // Tenant home page could have an empty string as a request path, where
                    // the default tenant does not.
                    if (String.IsNullOrEmpty(requestPath))
                    {
                        requestPath = "/";
                    }

                    return url.EndsWith('*')
                        ? requestPath.StartsWith(url.TrimEnd('*'), StringComparison.OrdinalIgnoreCase)
                        : String.Equals(requestPath, url, StringComparison.OrdinalIgnoreCase);
                }),
            };

            _culture = new GlobalMethod
            {
                Name = "culture",
                Method = serviceProvider => (Func<string, bool>)(culture =>
                {
                    var currentCulture = CultureInfo.CurrentCulture;

                    return String.Equals(culture, currentCulture.Name, StringComparison.OrdinalIgnoreCase) ||
                        String.Equals(culture, currentCulture.Parent.Name, StringComparison.OrdinalIgnoreCase);
                }),
            };

            _allMethods = new[] { _isAnonymous, _isAuthenticated, _isInRole, _isHomepage, _url, _culture };
        }

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return _allMethods;
        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Orchard.DependencyInjection.Middleware;

namespace Orchard.Users
{
    public class Middleware : IMiddlewareProvider
    {
        private readonly IdentityOptions _options;

        public Middleware(IOptions<IdentityOptions> options)
        {
            _options = options.Value;
        }

        public IEnumerable<MiddlewareRegistration> GetMiddlewares()
        {
            yield return new MiddlewareRegistration
            {
                Configure = builder => builder
                    .UseCookieAuthentication(_options.Cookies.ApplicationCookie)
                    .UseCookieAuthentication(_options.Cookies.ExternalCookie)
                    .UseCookieAuthentication(_options.Cookies.TwoFactorRememberMeCookie)
                    .UseCookieAuthentication(_options.Cookies.TwoFactorUserIdCookie)
            };
        }
    }
}

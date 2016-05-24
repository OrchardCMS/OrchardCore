using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Orchard.Environment.Shell;
using Orchard.Hosting.Middleware;

namespace Orchard.Users
{
    public class Middleware : IMiddlewareProvider
    {
        private readonly string _tenantName;
        private readonly string _tenantPrefix;

        public Middleware(ShellSettings shellSettings)
        {
            _tenantName = shellSettings.Name;
            _tenantPrefix = shellSettings.RequestUrlPrefix;
        }

        public IEnumerable<MiddlewareRegistration> GetMiddlewares()
        {
            var options = new IdentityCookieOptions();

            yield return new MiddlewareRegistration
            {
                Configure = builder => builder
                    .UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        CookieName = "orchauth_" + _tenantName,
                        CookiePath = _tenantPrefix,
                        AuthenticationScheme = options.ApplicationCookieAuthenticationScheme,
                        LoginPath = new PathString("/Orchard.Users/Account/Login/"),
                        AccessDeniedPath = new PathString("/Orchard.Users/Account/Login/"),
                        AutomaticAuthenticate = true,
                        AutomaticChallenge = true
                    })
                    .UseCookieAuthentication(options.ExternalCookie)
                    .UseCookieAuthentication(options.TwoFactorRememberMeCookie)
                    .UseCookieAuthentication(options.TwoFactorUserIdCookie)
            };
        }
    }
}

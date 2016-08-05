using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.Environment.Shell;
using Orchard.Security;
using Orchard.Users.Indexes;
using Orchard.Users.Models;
using Orchard.Users.Services;

namespace Orchard.Users
{
    public class Startup : StartupBase
    {
        private readonly string _tenantName;
        private readonly string _tenantPrefix;
        private readonly IdentityOptions _options;

        public Startup(ShellSettings shellSettings, IOptions<IdentityOptions> options)
        {
            _options = options.Value;
            _tenantName = shellSettings.Name;
            _tenantPrefix = shellSettings.RequestUrlPrefix;
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            builder.UseIdentity();
            builder
                .UseCookieAuthentication(_options.Cookies.ApplicationCookie)
                .UseCookieAuthentication(_options.Cookies.ExternalCookie)
                .UseCookieAuthentication(_options.Cookies.TwoFactorRememberMeCookie)
                .UseCookieAuthentication(_options.Cookies.TwoFactorUserIdCookie)
                ;
        }

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            /// Adds the default token providers used to generate tokens for reset passwords, change email
            /// and change telephone number operations, and for two factor authentication token generation.

            new IdentityBuilder(typeof(User), typeof(Role), serviceCollection).AddDefaultTokenProviders();

            // Identity services
            serviceCollection.TryAddSingleton<IdentityMarkerService>();
            serviceCollection.TryAddScoped<IUserValidator<User>, UserValidator<User>>();
            serviceCollection.TryAddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
            serviceCollection.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            serviceCollection.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();

            // No interface for the error describer so we can add errors without rev'ing the interface
            serviceCollection.TryAddScoped<IdentityErrorDescriber>();
            serviceCollection.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<User>>();
            serviceCollection.TryAddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, Role>>();
            serviceCollection.TryAddScoped<UserManager<User>>();
            serviceCollection.TryAddScoped<SignInManager<User>>();

            serviceCollection.TryAddScoped<IUserStore<User>, UserStore>();

            serviceCollection.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.CookieName = "orchauth_" + _tenantName;
                options.Cookies.ApplicationCookie.CookiePath = _tenantPrefix;
                options.Cookies.ApplicationCookie.LoginPath = new PathString("/Orchard.Users/Account/Login/");
                options.Cookies.ApplicationCookie.AccessDeniedPath = new PathString("/Orchard.Users/Account/Login/");
            });

            serviceCollection.AddScoped<UserIndexProvider>();
        }
    }
}

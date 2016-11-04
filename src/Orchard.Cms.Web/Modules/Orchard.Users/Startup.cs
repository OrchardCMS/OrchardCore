using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Users.Indexes;
using Orchard.Users.Models;
using Orchard.Users.Services;
using YesSql.Core.Indexes;

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
            _tenantPrefix = "/" + shellSettings.RequestUrlPrefix;
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

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSecurity();

            /// Adds the default token providers used to generate tokens for reset passwords, change email
            /// and change telephone number operations, and for two factor authentication token generation.

            new IdentityBuilder(typeof(User), typeof(Role), services).AddDefaultTokenProviders();

            // Identity services
            services.TryAddSingleton<IdentityMarkerService>();
            services.TryAddScoped<IUserValidator<User>, UserValidator<User>>();
            services.TryAddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
            services.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();

            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<User>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, Role>>();
            services.TryAddScoped<UserManager<User>>();
            services.TryAddScoped<SignInManager<User>>();

            services.TryAddScoped<IUserStore<User>, UserStore>();

            services.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.CookieName = "orchauth_" + _tenantName;
                options.Cookies.ApplicationCookie.CookiePath = _tenantPrefix;
                options.Cookies.ApplicationCookie.LoginPath = new PathString(_tenantPrefix.TrimEnd('/') + "/Orchard.Users/Account/Login/");
                options.Cookies.ApplicationCookie.AccessDeniedPath = new PathString(_tenantPrefix.TrimEnd('/') + "/Orchard.Users/Account/Login/");
            });

            services.AddScoped<IIndexProvider, UserIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();

            services.AddScoped<SetupEventHandler>();
            services.AddScoped<ISetupEventHandler>(sp => sp.GetRequiredService<SetupEventHandler>());

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }
}

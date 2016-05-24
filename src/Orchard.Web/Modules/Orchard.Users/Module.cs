using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.DependencyInjection;
using Orchard.Identity;

namespace Orchard.Users
{
    public class Module : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            /// Adds the default token providers used to generate tokens for reset passwords, change email
            /// and change telephone number operations, and for two factor authentication token generation.

            new IdentityBuilder(typeof(User), typeof(Role), serviceCollection).AddDefaultTokenProviders();

            // Services used by identity
            //serviceCollection.AddAuthentication(options =>
            //{
            //    // This is the Default value for ExternalCookieAuthenticationScheme
            //    options.SignInScheme = new IdentityCookieOptions().ApplicationCookieAuthenticationScheme;
            //});

            // Identity services
            serviceCollection.TryAddSingleton<IdentityMarkerService>();
            serviceCollection.TryAddScoped<IUserValidator<User>, UserValidator<User>>();
            serviceCollection.TryAddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
            serviceCollection.TryAddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            serviceCollection.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            serviceCollection.TryAddScoped<IRoleValidator<Role>, RoleValidator<Role>>();

            // No interface for the error describer so we can add errors without rev'ing the interface
            serviceCollection.TryAddScoped<IdentityErrorDescriber>();
            serviceCollection.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<User>>();
            serviceCollection.TryAddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, Role>>();
            serviceCollection.TryAddScoped<UserManager<User>, UserManager<User>>();
            serviceCollection.TryAddScoped<SignInManager<User>, SignInManager<User>>();
            serviceCollection.TryAddScoped<RoleManager<Role>, RoleManager<Role>>();

            serviceCollection.AddScoped<IUserStore<User>, UserStore>();
            serviceCollection.AddScoped<IRoleStore<Role>, RoleStore>();

            serviceCollection.Configure<IdentityOptions>(options =>
            {
                options.Cookies.ApplicationCookie.AuthenticationScheme = new IdentityCookieOptions().ApplicationCookieAuthenticationScheme;
                //options.Cookies.ExternalCookie.AuthenticationScheme = new IdentityCookieOptions().ExternalCookieAuthenticationScheme;
            });
        }
    }
}

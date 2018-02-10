using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Setup.Events;
using OrchardCore.Users.EntityFrameworkCore.Models;
using OrchardCore.Users.EntityFrameworkCore.Services;

namespace OrchardCore.Users.EntityFrameworkCore
{
    public static class ServiceExtensions
    {
        private static EntityFrameworkUserDataOptions DefaultOptions<TKey>()
        {
            Type keyType = typeof(TKey);
            return new EntityFrameworkUserDataOptions
            {
                UserType = typeof(User<>).MakeGenericType(keyType),
                RoleType = typeof(Role<>).MakeGenericType(keyType),
                UserClaimType = typeof(IdentityUserClaim<>).MakeGenericType(keyType),
                UserRoleType = typeof(IdentityUserRole<>).MakeGenericType(keyType),
                UserLoginType = typeof(IdentityUserLogin<>).MakeGenericType(keyType),
                UserTokenType = typeof(IdentityUserToken<>).MakeGenericType(keyType),
                RoleClaimType = typeof(RoleClaim<>).MakeGenericType(keyType)
            };
        }

        public static IServiceCollection AddEntityFrameworkUserDataStore<TContext,TKey>(this IServiceCollection services, Action<EntityFrameworkUserDataOptions> optionAction = null)
            where TContext : DbContext
            where TKey : IEquatable<TKey>
        {
            Type
                contextType = typeof(TContext),
                keyType = typeof(TKey);

            var options = DefaultOptions<TKey>();
            optionAction?.Invoke(options);

            services.Replace(ServiceDescriptor.Scoped(
                typeof(IUserStore<IUser>),
                typeof(UserStore<,,,,,,,,>).MakeGenericType(
                    options.UserType, 
                    options.RoleType, 
                    contextType, 
                    keyType, 
                    options.UserClaimType, 
                    options.UserRoleType, 
                    options.UserLoginType, 
                    options.UserTokenType, 
                    options.RoleClaimType)));

            var roleStoreType = typeof(RoleStore<,,,,>).MakeGenericType(options.RoleType, contextType, keyType, options.UserRoleType, options.RoleClaimType);
            services.Replace(ServiceDescriptor.Scoped(
                typeof(IRoleStore<IRole>),
                roleStoreType));

            services.Replace(ServiceDescriptor.Scoped(
                typeof(IRoleClaimStore<IRole>),
                roleStoreType));

            var roleUpdater = services.FirstOrDefault(descriptor =>
                descriptor.ImplementationType == typeof(Roles.Services.RoleUpdater));
            if (roleUpdater != null)
                services.Remove(roleUpdater);

            services.AddScoped<IFeatureEventHandler, RoleUpdater>();
            services.AddScoped<ISetupEventHandler, RoleUpdater>();
            return services;
        }
    }

    public class EntityFrameworkUserDataOptions
    {
        public Type UserType { get; set; }
        public Type RoleType { get; set; }
        public Type UserClaimType { get; set; }
        public Type UserRoleType { get; set; }
        public Type UserLoginType { get; set; }
        public Type UserTokenType { get; set; }
        public Type RoleClaimType { get; set; }

    }
}

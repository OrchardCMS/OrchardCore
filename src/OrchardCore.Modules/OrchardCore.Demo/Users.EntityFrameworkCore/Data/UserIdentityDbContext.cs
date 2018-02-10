using System;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Users.EntityFrameworkCore.Models;

namespace OrchardCore.Demo.Users.EntityFrameworkCore.Data
{
    public class UserIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> :
        IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TUser : IdentityUser<TKey>
        where TRole : Role<TKey>
        where TKey : IEquatable<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : RoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
    {
    }

    public class UserIdentityDbContext: UserIdentityDbContext<User<int>, Role<int>, int, IdentityUserClaim<int>, IdentityUserRole<int>, IdentityUserLogin<int>, RoleClaim<int>, IdentityUserToken<int>>
    {
        private readonly IServiceProvider _serviceProvider;

        public UserIdentityDbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var shellSettings = _serviceProvider.GetRequiredService<ShellSettings>();
            

            switch (shellSettings.DatabaseProvider)
            {
                case "SqlConnection":
                    optionsBuilder.UseSqlServer(shellSettings.ConnectionString);
                    break;
                case "Sqlite":
                    var shellOptions = _serviceProvider.GetService<IOptions<ShellOptions>>();
                    var option = shellOptions.Value;
                    var databaseFolder = Path.Combine(option.ShellsApplicationDataPath, option.ShellsContainerName, shellSettings.Name);
                    var databaseFile = Path.Combine(databaseFolder, "yessql.db");
                    optionsBuilder.UseSqlite($"Data Source={databaseFile};Cache=Shared");
                    break;
            }
        }
    }
}
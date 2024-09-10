using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Users;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<UserIndex>(table => table
            .Column<string>("NormalizedUserName") // TODO These should have defaults. on SQL Server they will fall at 255. Exceptions are currently thrown if you go over that.
            .Column<string>("NormalizedEmail")
            .Column<bool>("IsEnabled", c => c.NotNull().WithDefault(true))
            .Column<bool>("IsLockoutEnabled", c => c.NotNull().WithDefault(false))
            .Column<DateTime?>("LockoutEndUtc", c => c.Nullable())
            .Column<int>("AccessFailedCount", c => c.NotNull().WithDefault(0))
            .Column<string>("UserId")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .CreateIndex("IDX_UserIndex_DocumentId",
                "DocumentId",
                "UserId",
                "NormalizedUserName",
                "NormalizedEmail",
                "IsEnabled"
                )
        );

        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .CreateIndex("IDX_UserIndex_Lockout",
                "DocumentId",
                "IsLockoutEnabled",
                "LockoutEndUtc",
                "AccessFailedCount"
                )
        );

        await SchemaBuilder.CreateReduceIndexTableAsync<UserByRoleNameIndex>(table => table
           .Column<string>("RoleName")
           .Column<int>("Count")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserByRoleNameIndex>(table => table
            .CreateIndex("IDX_UserByRoleNameIndex_RoleName",
                "RoleName")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<UserByLoginInfoIndex>(table => table
            .Column<string>("LoginProvider")
            .Column<string>("ProviderKey"));

        await SchemaBuilder.AlterIndexTableAsync<UserByLoginInfoIndex>(table => table
            .CreateIndex("IDX_UserByLoginInfoIndex_DocumentId",
                "DocumentId",
                "LoginProvider",
                "ProviderKey")
        );

        await SchemaBuilder.CreateMapIndexTableAsync<UserByClaimIndex>(table => table
           .Column<string>("ClaimType")
           .Column<string>("ClaimValue"),
            null);

        await SchemaBuilder.AlterIndexTableAsync<UserByClaimIndex>(table => table
            .CreateIndex("IDX_UserByClaimIndex_DocumentId",
                "DocumentId",
                "ClaimType",
                "ClaimValue")
        );

        // Shortcut other migration steps on new content definition schemas.
        return 13;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<UserByLoginInfoIndex>(table => table
            .Column<string>("LoginProvider")
            .Column<string>("ProviderKey"));

        return 2;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<UserByClaimIndex>(table => table
           .Column<string>("ClaimType")
           .Column<string>("ClaimValue"),
            null);

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .AddColumn<bool>("IsEnabled", c => c.NotNull().WithDefault(true)));

        return 4;
    }

    // UserId database migration.
    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .AddColumn<string>("UserId"));

        return 5;
    }

    // UserId column is added. This initializes the UserId property to the UserName for existing users.
    // The UserName property rather than the NormalizedUserName is used as the ContentItem.Owner property matches the UserName.
    // New users will be created with a generated Id.
    // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom5()
#pragma warning restore CA1822 // Mark members as static
    {
        // Defer this until after the subsequent migrations have succeeded as the schema has changed.
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var users = await session.Query<User>().ListAsync();
            foreach (var user in users)
            {
                user.UserId = user.UserName;
                await session.SaveAsync(user);
            }
        });

        return 6;
    }

    // This buggy migration has been removed.
    // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom6()
#pragma warning restore CA1822 // Mark members as static
    {
        return 7;
    }

    // Migrate any user names replacing '@' with '+' as user names can no longer be an email address.
    // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom7()
#pragma warning restore CA1822 // Mark members as static
    {
        // Defer this until after the subsequent migrations have succeeded as the schema has changed.
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var users = await session.Query<User, UserIndex>(u => u.NormalizedUserName.Contains('@')).ListAsync();
            foreach (var user in users)
            {
                user.UserName = user.UserName.Replace('@', '+');
                user.NormalizedUserName = user.NormalizedUserName.Replace('@', '+');
                await session.SaveAsync(user);
            }
        });

        return 8;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom8Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .CreateIndex("IDX_UserIndex_DocumentId",
                "DocumentId",
                "UserId",
                "NormalizedUserName",
                "NormalizedEmail",
                "IsEnabled")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserByLoginInfoIndex>(table => table
            .CreateIndex("IDX_UserByLoginInfoIndex_DocumentId",
                "DocumentId",
                "LoginProvider",
                "ProviderKey")
        );

        await SchemaBuilder.AlterIndexTableAsync<UserByClaimIndex>(table => table
            .CreateIndex("IDX_UserByClaimIndex_DocumentId",
                "DocumentId",
                "ClaimType",
                "ClaimValue")
        );

        return 9;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom9Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserByRoleNameIndex>(table => table
            .CreateIndex("IDX_UserByRoleNameIndex_RoleName",
                "RoleName")
        );

        return 10;
    }

    public async Task<int> UpdateFrom10Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .AddColumn<bool>("IsLockoutEnabled", c => c.NotNull().WithDefault(false)));

        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .AddColumn<DateTime?>("LockoutEndUtc", c => c.Nullable()));

        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .AddColumn<int>("AccessFailedCount", c => c.NotNull().WithDefault(0)));

        return 11;
    }

    public async Task<int> UpdateFrom11Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<UserIndex>(table => table
            .CreateIndex("IDX_UserIndex_Lockout",
                "DocumentId",
                "IsLockoutEnabled",
                "LockoutEndUtc",
                "AccessFailedCount"
                )
        );

        return 12;
    }

#pragma warning disable CA1822 // Mark members as static
    public int UpdateFrom12()
#pragma warning restore CA1822 // Mark members as static
    {
        ShellScope.AddDeferredTask(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var dbConnectionAccessor = scope.ServiceProvider.GetService<IDbConnectionAccessor>();
            var logger = scope.ServiceProvider.GetService<ILogger<Migrations>>();
            var tablePrefix = session.Store.Configuration.TablePrefix;
            var documentTableName = session.Store.Configuration.TableNameConvention.GetDocumentTable();
            var table = $"{session.Store.Configuration.TablePrefix}{documentTableName}";

            logger.LogDebug("Updating User Settings");

            await using var connection = dbConnectionAccessor.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync(session.Store.Configuration.IsolationLevel);
            var dialect = session.Store.Configuration.SqlDialect;

            try
            {
                var quotedTableName = dialect.QuoteForTableName(table, session.Store.Configuration.Schema);
                var quotedContentColumnName = dialect.QuoteForColumnName("Content");
                var quotedTypeColumnName = dialect.QuoteForColumnName("Type");

                var updateCmd = $"UPDATE {quotedTableName} SET {quotedContentColumnName} = REPLACE({quotedContentColumnName}, 'OrchardCore.Users.Models.LoginSettings, OrchardCore.Users', 'OrchardCore.Users.Models.LoginSettings, OrchardCore.Users.Core') WHERE {quotedTypeColumnName} = 'OrchardCore.Deployment.DeploymentPlan, OrchardCore.Deployment.Abstractions'";

                await transaction.Connection.ExecuteAsync(updateCmd, null, transaction);

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                logger.LogError(e, "An error occurred while updating User Settings");

                throw;
            }
        });

        return 13;
    }
}

using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Users
{
    public class Migrations : DataMigration
    {
        private readonly ISession _session;

        public Migrations(ISession session)
        {
            _session = session;
        }

        // This is a sequenced migration. On a new schemas this is complete after UpdateFrom2.
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<UserIndex>(table => table
                .Column<string>("NormalizedUserName") // TODO These should have defaults. on SQL Server they will fall at 255. Exceptions are currently thrown if you go over that.
                .Column<string>("NormalizedEmail")
                .Column<bool>("IsEnabled", c => c.NotNull().WithDefault(true))
                .Column<string>("UserId")
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_IsEnabled", "DocumentId", "IsEnabled")
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_UserId", "DocumentId", "UserId")
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_UserName", "DocumentId", "NormalizedUserName")
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_Email", "DocumentId", "NormalizedEmail")
            );

            SchemaBuilder.CreateReduceIndexTable<UserByRoleNameIndex>(table => table
               .Column<string>("RoleName")
               .Column<int>("Count")
            );

            return UpdateFrom1();
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<UserByLoginInfoIndex>(table => table
                .Column<string>("LoginProvider")
                .Column<string>("ProviderKey"));
            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTable<UserByClaimIndex>(table => table
               .Column<string>(nameof(UserByClaimIndex.ClaimType))
               .Column<string>(nameof(UserByClaimIndex.ClaimValue)),
                null);

            // Return 8 here to skip migrations on new database schemas.
            return 8;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<bool>(nameof(UserIndex.IsEnabled), c => c.NotNull().WithDefault(true)));

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_IsEnabled", "IsEnabled")
            );

            return 4;
        }

        // UserId database migration.
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .AddColumn<string>("UserId"));

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_UserId", "DocumentId", "UserId")
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_UserName", "DocumentId", "NormalizedUserName")
            );

            SchemaBuilder.AlterIndexTable<UserIndex>(table => table
                .CreateIndex("IDX_UserIndex_Email", "DocumentId", "NormalizedEmail")
            );

            return 5;
        }

        // UserId column is added. This initializes the UserId property to the UserName for existing users.
        // The UserName property rather than the NormalizedUserName is used as the ContentItem.Owner property matches the UserName.
        // New users will be created with a generated Id.
        public async Task<int> UpdateFrom5Async()
        {
            var users = await _session.Query<User>().ListAsync();
            foreach(var user in users)
            {
                user.UserId = user.UserName;
                _session.Save(user);
            }

            // Return 7 to shortcut a buggy migration which has been removed.
            return 7;
        }

        // This migration has been removed.
        public int UpdateFrom6()
        {
            return 7;
        }

        // Migrate any user names replacing '@' with '+' as user names can no longer be an email address.
        public async Task<int> UpdateFrom7Async()
        {
            var users = await _session.Query<User, UserIndex>(u => u.NormalizedUserName.Contains("@")).ListAsync();
            foreach(var user in users)
            {
                user.UserName = user.UserName.Replace('@', '+');
                user.NormalizedUserName = user.NormalizedUserName.Replace('@', '+');
                _session.Save(user);
            }

            return 8;
        }
    }
}

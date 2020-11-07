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

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_IsEnabled", "DocumentId", "IsEnabled")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_UserId", "DocumentId", "UserId")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_UserName", "DocumentId", "NormalizedUserName")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
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

            // Return 6 here to skip migrations on new database schemas.
            return 6;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .AddColumn<bool>(nameof(UserIndex.IsEnabled), c => c.NotNull().WithDefault(true)));

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_IsEnabled", "IsEnabled")
            );

            return 4;
        }

        // UserId database migration.
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .AddColumn<string>("UserId"));

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_UserId", "DocumentId", "UserId")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_UserName", "DocumentId", "NormalizedUserName")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_Email", "DocumentId", "NormalizedEmail")
            );

            return 5;
        }

        // UserId column is added. This initializes the UserId property to the NormalizedUserName for existing users.
        // New users will be created with a generated Id.
        public async Task<int> UpdateFrom5Async()
        {
            var users = await _session.Query<User, UserIndex>().ListAsync();
            foreach(var user in users)
            {
                user.UserId = user.NormalizedUserName;
                _session.Save(user);
            }

            return 6;
        }
    }
}

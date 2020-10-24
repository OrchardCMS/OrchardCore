using OrchardCore.Data.Migration;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Users.Indexes;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.Users
{
    public class Migrations : DataMigration
    {
        private readonly ISession _session;
        private readonly IShellFeaturesManager _shellFeatureManager;
        private readonly IExtensionManager _extensionManager;

        public Migrations(ISession session, IShellFeaturesManager shellFeaturesManager, IExtensionManager extensionManager)
        {
            _session = session;
            _shellFeatureManager = shellFeaturesManager;
            _extensionManager = extensionManager;
        }

        // This is a sequenced migration which on new schemas is complete after UpdateFrom2.
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<UserIndex>(table => table
                .Column<string>("NormalizedUserName") // These should have defaults. on SQL Server they will fall at 255. Exceptions are currently thrown if you go over that.
                .Column<string>("NormalizedEmail")
                .Column<bool>("IsEnabled", c => c.NotNull().WithDefault(true))
                .Column<string>("UserId", c => c.WithLength(26))
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
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_Email", "DocumentId", "NormalizedEmail")
            );

            // TODO when we do email login we might want NormalizedUserName and NormalizedEmail.
            // Depends on the query.

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
                // DocumentId added to this index, but it will already have run for most users.
                .CreateIndex("IDX_UserIndex_IsEnabled", "DocumentId", "IsEnabled")
            );

            return 4;
        }

        // UserId database migration.
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .AddColumn<string>("UserId", c => c.WithLength(26)));

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_UserId", "DocumentId", "UserId")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_UserName", "DocumentId", "NormalizedUserName")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                // This index will be used for lookups when logging in.
                .CreateIndex("IDX_UserIndex_Email", "DocumentId", "NormalizedEmail")
            );

            return 5;
        }
    }
}

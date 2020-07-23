using OrchardCore.Data.Migration;
using OrchardCore.Users.Indexes;

namespace OrchardCore.Users
{
    public class Migrations : DataMigration
    {
        // This is a sequenced migration which on new schemas is complete after UpdateFrom2.
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(UserIndex), table => table
                .Column<string>("NormalizedUserName")
                .Column<string>("NormalizedEmail")
                .Column<bool>("IsEnabled", c => c.NotNull().WithDefault(true))
                .Column<string>("UserId")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_IsEnabled", "IsEnabled")
            );

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_UserId", "UserId", "NormalizedUserName")
            );

            SchemaBuilder.CreateReduceIndexTable(nameof(UserByRoleNameIndex), table => table
                .Column<string>("RoleName")
                .Column<int>("Count")
            );

            return UpdateFrom1();
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(UserByLoginInfoIndex), table => table
                .Column<string>("LoginProvider")
                .Column<string>("ProviderKey"));
            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(UserByClaimIndex), table => table
                .Column<string>(nameof(UserByClaimIndex.ClaimType))
                .Column<string>(nameof(UserByClaimIndex.ClaimValue)));

            // Return 5 here to skip migrations on new database schemas.
            return 5;
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

        public int UpdateFrom4()
        {
            // The length is the default as for backwards compatability this value will be the NormalizedUserName
            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .AddColumn<string>(nameof(UserIndex.UserId)));

            SchemaBuilder.AlterTable(nameof(UserIndex), table => table
                .CreateIndex("IDX_UserIndex_UserId", "UserId", "NormalizedUserName")
            );

            return 5;
        }
    }
}

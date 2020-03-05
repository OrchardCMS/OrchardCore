using OrchardCore.Data.Migration;
using OrchardCore.Users.Indexes;

namespace OrchardCore.Users
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(UserIndex), table => table
                .Column<string>("NormalizedUserName")
                .Column<string>("NormalizedEmail")
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
            return 3;
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
    }
}

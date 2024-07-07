using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Queries.Indexes;
using YesSql.Sql;

namespace OrchardCore.Queries.Migrations;

public sealed class QueryMigrations : DataMigration
{
    public const int MaxQuerySourceLength = 100;
    public const int MaxQueryNameLength = 200;

    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<QueryIndex>(table => table
               .Column<string>("Source", column => column.WithLength(MaxQuerySourceLength))
               .Column<string>("Name", column => column.WithLength(MaxQueryNameLength))
            );

        await SchemaBuilder.AlterIndexTableAsync<QueryIndex>(table => table
            .CreateIndex("IDX_QueryIndex_DocumentId_Source",
                "DocumentId",
                "Source",
                "Name")
            );

        await SchemaBuilder.AlterIndexTableAsync<QueryIndex>(table => table
            .CreateIndex("IDX_QueryIndex_DocumentId_Name",
                "DocumentId",
                "Name"
                )
            );

        return 1;
    }
}

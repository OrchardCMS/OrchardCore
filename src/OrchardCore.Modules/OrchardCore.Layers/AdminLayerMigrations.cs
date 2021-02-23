using System;
using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Services;
using OrchardCore.Rules;
using YesSql.Sql;

namespace OrchardCore.Layers
{
    public class AdminLayerMigrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<AdminLayerMetadataIndex>(table => table
               .Column<string>("Zone", c => c.WithLength(64))
            );

            SchemaBuilder.AlterIndexTable<AdminLayerMetadataIndex>(table => table
                .CreateIndex("IDX_AdminLayerMetadata_DocumentId", "DocumentId", "Zone")
            );

            return 1;
        }
    }
}

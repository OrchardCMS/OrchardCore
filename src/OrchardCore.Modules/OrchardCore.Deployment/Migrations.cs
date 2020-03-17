using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Indexes;

namespace OrchardCore.Deployment
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(DeploymentPlanIndex), table => table
                .Column<string>("Name")
            );

            return 1;
        }
    }
}

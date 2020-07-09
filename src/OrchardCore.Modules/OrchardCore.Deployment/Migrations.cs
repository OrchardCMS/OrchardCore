using OrchardCore.Data.Migration;
using OrchardCore.Deployment.Indexes;

namespace OrchardCore.Deployment
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(typeof(DeploymentPlanIndex), table => table
                .Column<string>("Name"),
                null
            );

            return 1;
        }
    }
}

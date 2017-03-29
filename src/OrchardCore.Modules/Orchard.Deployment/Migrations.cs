using Orchard.Data.Migration;
using Orchard.Deployment.Indexes;

namespace Orchard.Deployment
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
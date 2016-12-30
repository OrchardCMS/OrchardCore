using Orchard.Data.Migration;
using Orchard.Recipes.Models;

namespace Orchard.Recipes
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(RecipeResultIndex), table => table
                .Column<string>("ExecutionId")
                .Column<bool>("IsCompleted")
                .Column<int>("TotalSteps")
                .Column<int>("CompletedSteps")
            );

            return 1;
        }
    }
}
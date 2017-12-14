using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes
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
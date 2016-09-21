using Orchard.Data.Migration;
using Orchard.Recipes.Models;

namespace Orchard.Recipes
{
    public class Migrations : DataMigrations
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(RecipeResultIndex), table => table
                .Column<int>("ExecutionId")
            );

            return 1;
        }
    }
}
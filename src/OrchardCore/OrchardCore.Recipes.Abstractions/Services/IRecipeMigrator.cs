using System.Threading.Tasks;
using OrchardCore.Data.Migration;

namespace OrchardCore.Recipes.Services
{
    public interface IRecipeMigrator
    {
        Task<string> ExecuteAsync(string recipeFileName, IDataMigration migration);
    }
}

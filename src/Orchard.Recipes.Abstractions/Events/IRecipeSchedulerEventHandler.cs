using System.Threading.Tasks;

namespace Orchard.Recipes.Events
{
    public interface IRecipeSchedulerEventHandler
    {
        Task ExecuteWorkAsync(string executionId);
    }
}
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeScheduler
    {
        Task ScheduleWorkAsync(string executionId);
    }
}
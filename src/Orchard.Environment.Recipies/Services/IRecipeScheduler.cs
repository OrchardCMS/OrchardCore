using Orchard.DependencyInjection;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeScheduler : IDependency
    {
        void ScheduleWork(string executionId);
    }
}
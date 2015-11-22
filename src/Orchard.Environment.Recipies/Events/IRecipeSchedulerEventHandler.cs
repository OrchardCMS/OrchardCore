using Orchard.Events;

namespace Orchard.Environment.Recipes.Events
{
    public interface IRecipeSchedulerEventHandler : IEventHandler
    {
        void ExecuteWork(string executionId);
    }
}
namespace Orchard.Recipes.Events
{
    public interface IRecipeSchedulerEventHandler
    {
        void ExecuteWork(string executionId);
    }
}
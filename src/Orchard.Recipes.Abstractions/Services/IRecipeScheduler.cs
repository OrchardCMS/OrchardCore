namespace Orchard.Recipes.Services
{
    public interface IRecipeScheduler
    {
        void ScheduleWork(string executionId);
    }
}
namespace Orchard.Recipes.Services
{
    public interface IRecipeInterpreter
    {
        bool TryEvaluate(string token, out string result);
    }
}

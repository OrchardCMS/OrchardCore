using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Recipes.Services
{
    public interface IRecipeEnvironmentProvider
    {
        Task SetEnvironmentAsync(IDictionary<string, object> environment);
    }
}

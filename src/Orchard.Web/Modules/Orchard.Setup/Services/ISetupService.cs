using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Recipes.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Setup.Services
{
    public interface ISetupService : IDependency
    {
        ShellSettings Prime();
        IReadOnlyList<RecipeDescriptor> Recipes();
        Task<string> SetupAsync(SetupContext context);
    }
}
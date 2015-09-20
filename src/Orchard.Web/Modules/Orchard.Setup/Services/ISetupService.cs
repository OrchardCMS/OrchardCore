using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;
using System.Collections.Generic;

namespace Orchard.Setup.Services {
    public interface ISetupService : IDependency {
        IEnumerable<Recipe> Recipes();
        string Setup(SetupContext context);
    }
}
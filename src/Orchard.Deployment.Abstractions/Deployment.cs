using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Recipes.Models;

namespace Orchard.Deployment
{
    /// <summary>
    /// The state of a deployment plan built by sources.
    /// </summary>
    public class DeploymentPlanResult
    {
        public IList<RecipeStepDescriptor> RecipeSteps { get; } = new List<RecipeStepDescriptor>();
    }
}

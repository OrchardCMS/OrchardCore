using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Deployment.Recipes
{
    /// <summary>
    /// This recipe step creates a deployment plan.
    /// </summary>
    public class DeploymentPlansRecipeStep : IRecipeStepHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDeploymentPlanService _deploymentPlanService;

        public DeploymentPlansRecipeStep(
            IServiceProvider serviceProvider,
            IDeploymentPlanService deploymentPlanService)
        {
            _serviceProvider = serviceProvider;
            _deploymentPlanService = deploymentPlanService;
        }

        public Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "deployment", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            var deploymentStepFactories = _serviceProvider.GetServices<IDeploymentStepFactory>().ToDictionary(f => f.Name);

            var model = context.Step.ToObject<DeploymentPlansModel>();

            var unknownTypes = new List<string>();
            var deploymentPlans = new List<DeploymentPlan>();

            foreach (var plan in model.Plans)
            {
                var deploymentPlan = new DeploymentPlan
                {
                    Name = plan.Name
                };

                foreach (var step in plan.Steps)
                {
                    if (deploymentStepFactories.TryGetValue(step.Type, out var deploymentStepFactory))
                    {
                        var deploymentStep = (DeploymentStep)step.Step.ToObject(deploymentStepFactory.Create().GetType());

                        deploymentPlan.DeploymentSteps.Add(deploymentStep);
                    }
                    else
                    {
                        unknownTypes.Add(step.Type);
                    }
                }

                deploymentPlans.Add(deploymentPlan);
            }

            if (unknownTypes.Count != 0)
            {
                var prefix = "No changes have been made. The following types of deployment plans cannot be added:";
                var suffix = "Please ensure that the related features are enabled to add these types of deployment plans.";

                throw new InvalidOperationException($"{prefix} {String.Join(", ", unknownTypes)}. {suffix}");
            }

            return _deploymentPlanService.CreateOrUpdateDeploymentPlansAsync(deploymentPlans);
        }

        private class DeploymentPlansModel
        {
            public DeploymentPlanModel[] Plans { get; set; }
        }

        private class DeploymentPlanModel
        {
            public string Name { get; set; }

            public DeploymentStepModel[] Steps { get; set; }
        }

        private class DeploymentStepModel
        {
            public string Type { get; set; }

            public JObject Step { get; set; }
        }
    }
}

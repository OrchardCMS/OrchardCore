using System;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.OpenId.Services.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    public class OpenIdApplicationStep : IRecipeStepHandler
    {
        private readonly OpenIdApplicationManager _applicationManager;

        /// <summary>
        /// This recipe step adds an OpenID Connect app.
        /// </summary>
        public OpenIdApplicationStep(OpenIdApplicationManager applicationManager)
        {
            _applicationManager = applicationManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdApplication", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<CreateOpenIdApplicationViewModel>();
            await _applicationManager.CreateAsync(model, CancellationToken.None);
        }
    }
}
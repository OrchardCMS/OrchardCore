using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    public class OpenIdScopeStep : IRecipeStepHandler
    {
        private readonly IOpenIdScopeManager _scopeManager;

        /// <summary>
        /// This recipe step adds an OpenID Connect scope.
        /// </summary>
        public OpenIdScopeStep(IOpenIdScopeManager scopeManager)
        {
            _scopeManager = scopeManager;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdScope", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdScopeStepModel>();

            var descriptor = new OpenIdScopeDescriptor
            {
                Description = model.Description,
                Name = model.ScopeName,
                DisplayName = model.DisplayName
            };

            if (model.Resources.Any())
            {
                descriptor.Resources.UnionWith(model.Resources);
            }

            var scope = await _scopeManager.FindByNameAsync(model.ScopeName);
            if (scope != null)
            {
                await _scopeManager.UpdateAsync(scope, descriptor);
            }
            else
            {
                await _scopeManager.CreateAsync(descriptor);
            }  
        }
    }
}

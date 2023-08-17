using System;
using System.Threading.Tasks;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
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
            var scope = await _scopeManager.FindByNameAsync(model.ScopeName);
            var descriptor = new OpenIdScopeDescriptor();
            var isNew = true;

            if (scope != null)
            {
                isNew = false;
                await _scopeManager.PopulateAsync(scope, descriptor);
            }

            descriptor.Description = model.Description;
            descriptor.Name = model.ScopeName;
            descriptor.DisplayName = model.DisplayName;

            if (!string.IsNullOrEmpty(model.Resources))
            {
                descriptor.Resources.Clear();
                descriptor.Resources.UnionWith(
                    model.Resources
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries));
            }

            if (isNew)
            {
                await _scopeManager.CreateAsync(descriptor);
            }
            else
            {
                await _scopeManager.UpdateAsync(scope, descriptor);
            }
        }
    }
}

using System;
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

            var model = context.Step.ToObject<OpenIdScopeStepViewModel>();
            var descriptor = new OpenIdScopeDescriptor
            {
                Description = model.Description,
                Name = model.ScopeName,
                DisplayName = model.DisplayName
            };

            if (!string.IsNullOrEmpty(model.Resources))
            {
                descriptor.Resources.UnionWith(model.Resources.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries));
            }

            await _scopeManager.CreateAsync(descriptor);
        }
    }
}
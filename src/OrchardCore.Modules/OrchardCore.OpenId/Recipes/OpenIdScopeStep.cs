using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.OpenId.Recipes
{
    public class OpenIdScopeStep : IRecipeStepHandler
    {
        private readonly IOpenIdScopeManager _scopeManager;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// This recipe step adds an OpenID Connect scope.
        /// </summary>
        public OpenIdScopeStep(
            IOpenIdScopeManager scopeManager,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _scopeManager = scopeManager;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!string.Equals(context.Name, "OpenIdScope", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<OpenIdScopeStepModel>(_jsonSerializerOptions);
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

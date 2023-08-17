using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailFilterEngineModelBinder : IModelBinder
    {
        private readonly IAuditTrailAdminListFilterParser _parser;

        public AuditTrailFilterEngineModelBinder(IAuditTrailAdminListFilterParser parser)
        {
            _parser = parser;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name q=
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Success(_parser.Parse(String.Empty));

                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // When value is null or empty the parser returns an empty result.
            bindingContext.Result = ModelBindingResult.Success(_parser.Parse(value));

            return Task.CompletedTask;
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding
{
    public class CheckMarkModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType == typeof(bool))
            {
                var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (valueProviderResult == ValueProviderResult.None)
                {
                    return Task.CompletedTask;
                }

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                if (valueProviderResult.Values == "✓")
                {
                    bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName);
                }
                else if (valueProviderResult.Values == "✗")
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                }
            }

            return Task.CompletedTask;
        }
    }
}

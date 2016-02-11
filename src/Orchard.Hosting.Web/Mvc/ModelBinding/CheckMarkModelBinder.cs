using Microsoft.AspNet.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Orchard.Hosting.Mvc.ModelBinding
{
    public class CheckMarkModelBinder : IModelBinder
    {
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            if(bindingContext.ModelType == typeof(bool))
            {
                var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (valueProviderResult == ValueProviderResult.None)
                {
                    return ModelBindingResult.NoResultAsync;
                }

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

                if (valueProviderResult.Values == "✓")
                {
                    return ModelBindingResult.SuccessAsync(bindingContext.ModelName, true);
                }
                else if(valueProviderResult.Values == "✗")
                {
                    return ModelBindingResult.SuccessAsync(bindingContext.ModelName, false);
                }
            }

            return ModelBindingResult.NoResultAsync;
        }
    }
}

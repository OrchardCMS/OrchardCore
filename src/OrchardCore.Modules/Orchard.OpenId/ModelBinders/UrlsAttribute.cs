using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.OpenId.ModelBinders
{
    public class UrlsBinder : IModelBinder
    {

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
            if (valueProviderResult.Length > 0)
            {
                var valueAsString = valueProviderResult.FirstValue;

                if (string.IsNullOrEmpty(valueAsString))
                {
                    return TaskCache.CompletedTask;
                }

                if (!string.IsNullOrEmpty(valueAsString))
                {
                    var origins = valueAsString
                        .Split(new[]{","}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(origin=>origin.Trim())
                        .ToArray();
                    if (origins.All(origin => Uri.IsWellFormedUriString(origin, UriKind.Absolute)))
                    {
                        bindingContext.Result = ModelBindingResult.Success(origins);
                    }
                    else
                    {
                        bindingContext.Result = ModelBindingResult.Success(origins);
                        bindingContext.ModelState.AddModelError(bindingContext.FieldName,"Please specify Url(s) in correct format");
                        
                    }
                }
            }
            return TaskCache.CompletedTask;
        }
    }
}
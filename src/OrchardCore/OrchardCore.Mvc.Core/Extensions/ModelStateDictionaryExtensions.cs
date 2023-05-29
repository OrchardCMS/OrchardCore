using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding
{
    public static class ModelStateDictionaryExtensions
    {
        public static void AddModelError(this ModelStateDictionary modelState, ModelError error)
        {
            if (modelState is null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (error is null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            modelState.AddModelError(error.Key, error.Message);
        }

        public static void AddModelErrors(this ModelStateDictionary modelState, IEnumerable<ModelError> errors)
        {
            if (modelState is null)
            {
                throw new ArgumentNullException(nameof(modelState));
            }

            if (errors is null)
            {
                throw new ArgumentNullException(nameof(errors));
            }

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    modelState.AddModelError(error);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Mvc.ModelBinding
{
    public static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Adds the specified error message to the errors collection for the model-state dictionary that is associated with the specified key.
        /// </summary>
        /// <param name="modelState">The model state.</param>
        /// <param name="prefix">The prefix of the key.</param>
        /// <param name="key">The key.</param>
        /// <param name="errorMessage">The error message.</param>
        public static void AddModelError(this ModelStateDictionary modelState, string prefix, string key, string errorMessage)
        {
            var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";
            modelState.AddModelError(fullKey, errorMessage);
        }

        /// <summary>
        /// Adds the specified error message to the errors collection for the model-state dictionary that is associated with the specified key.
        /// </summary>
        /// <param name="modelState">The model state.</param>
        /// <param name="prefix">The prefix of the key.</param>
        /// <param name="validationResults">The <see cref="ValidationResult"/>.</param>
        public static void BindValidationResults(this ModelStateDictionary modelState, string prefix, IEnumerable<ValidationResult> validationResults)
        {
            if (validationResults != null)
            {
                foreach (var item in validationResults)
                {
                    modelState.BindValidationResult(prefix, item);
                }
            }
        }

        public static void BindValidationResult(this ModelStateDictionary modelState, string prefix, ValidationResult item)
        {
            if (!item.MemberNames.Any())
            {
                modelState.AddModelError(prefix, string.Empty, item.ErrorMessage);
            }
            else
            {
                foreach (var memberName in item.MemberNames)
                {
                    modelState.AddModelError(prefix, memberName, item.ErrorMessage);
                }
            }
        }
    }
}



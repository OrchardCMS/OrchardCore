using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}

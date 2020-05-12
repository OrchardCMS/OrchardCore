using System;
using System.Linq.Expressions;
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
        /// Adds the specified error message to the errors collection for the model-state dictionary that is associated with the specified property of Model.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="modelState"></param>
        /// <param name="model"></param>
        /// <param name="prefix"></param>
        /// <param name="action"></param>
        /// <param name="errorMessage"></param>
        public static void AddModelError<TModel>(this ModelStateDictionary modelState, TModel model, string prefix, Expression<Func<TModel, object>> action, string errorMessage) where TModel : class
        {
            var expression = (MemberExpression)action.Body;
            string key = expression.Member.Name;
            modelState.AddModelError(prefix, key, errorMessage);
        }
    }
}

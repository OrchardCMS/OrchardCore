using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Data.QueryParser
{
    public class TermModelBinder<T> : IModelBinder where T : class
    {
        // TODO what if we want multiple query parsers for ContentItem
        // Need a better registration with T and TParser.
        // Or just base this, so it's possible, and defaults to just this T
        // Two :
        // TermModelBinder<T> : TermModelBinder<T, IQueryParser<T>>
        // TermModelBinder<T, IQueryParser<T>>
        private readonly IQueryParser<T> _parser;

        public TermModelBinder(IQueryParser<T> parser)
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
                bindingContext.Result = ModelBindingResult.Success(new TermList<T>());

                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                bindingContext.Result = ModelBindingResult.Success(new TermList<T>());
                
                return Task.CompletedTask;
            }

            var termList = _parser.Parse(value);

            bindingContext.Result = ModelBindingResult.Success(termList);
            
            return Task.CompletedTask;
        }
    }
}
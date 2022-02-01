using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Tests.Utilities
{
    public class ModelValidator
    {
        public static bool Validate(object model, out ICollection<ValidationResult> errors, IServiceProvider serviceProvider = null)
        {
            var validationContext = new ValidationContext(model);

            if (serviceProvider != null)
            {
                validationContext.InitializeServiceProvider(t => serviceProvider.GetService(t));
            }

            if (model is not IValidatableObject)
            {
                throw new InvalidOperationException($"The '{nameof(model)}' should implement {nameof(IValidatableObject)}.");
            }

            errors = new List<ValidationResult>();

            return Validator.TryValidateObject(model, validationContext, errors);
        }
    }
}

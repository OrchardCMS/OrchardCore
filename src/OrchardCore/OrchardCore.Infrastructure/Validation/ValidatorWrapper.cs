using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Validation
{
    /// <summary>
    /// Represents a wrapper class for <see cref="Validator"/> that can be used to validate objects.
    /// </summary>
    public static class ValidatorWrapper
    {
        /// <summary>
        /// Determines whether the specified object is valid using the validation context and validation results collection.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validationResults">A collection to hold each failed validation.</param>
        public static bool TryValidateObject(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
            => Validator.TryValidateObject(instance, validationContext, validationResults);

        /// <summary>
        /// Determines whether the specified object is valid using the validation context and validation results collection.
        /// </summary>
        /// <param name="instance">The object to validate.</param>
        /// <param name="validationContext">The context that describes the object to validate.</param>
        /// <param name="validationResults">A collection to hold each failed validation.</param>
        public static async Task<bool> TryValidateObjectAsync(object instance, ValidationContext validationContext, ICollection<ValidationResult> validationResults)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (validationContext != null && instance != validationContext.ObjectInstance)
            {
                throw new ArgumentException($"The {instance} must match {validationContext} instance.", nameof(instance));
            }

            if (instance is IAsyncValidatableObject validatable)
            {
                var results = await validatable.ValidateAsync(validationContext);

                if (results != null)
                {
                    foreach (var result in results.Where(r => r != ValidationResult.Success))
                    {
                        validationResults.Add(result);
                    }
                }
            }

            return validationResults.Count == 0;
        }
    }
}

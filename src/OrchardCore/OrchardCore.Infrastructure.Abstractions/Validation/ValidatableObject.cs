using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OrchardCore.Validation
{
    /// <summary>
    /// Represents a base class for validating objects.
    /// </summary>
    public abstract class ValidatableObject : IAsyncValidatableObject
    {
        /// <inheritdoc/>
        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationTask = ValidateAsync(validationContext, CancellationToken.None);

            validationTask.Wait();

            return validationTask.Result;
        }

        /// <inheritdoc/>
        public virtual Task<IEnumerable<ValidationResult>> ValidateAsync(ValidationContext validationContext)
            => Task.FromResult((IEnumerable<ValidationResult>)new List<ValidationResult>());
    }
}

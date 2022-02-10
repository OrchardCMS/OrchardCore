using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace OrchardCore.Validation
{
    /// <summary>
    /// Represents a contract to validate an object in async fashion.
    /// </summary>
    public interface IAsyncValidatableObject : IValidatableObject
    {
        /// <summary>
        /// Determines whether the specified object is valid.
        /// </summary>
        /// <param name="validationContext">The <see cref="ValidationContext"/>.</param>
        /// <param name="cancellation">The <see cref="CancellationToken"/>.</param>
        /// <returns>A collection that holds failed-validation information.</returns>
        Task<IEnumerable<ValidationResult>> ValidateAsync(ValidationContext validationContext, CancellationToken cancellation);
    }
}

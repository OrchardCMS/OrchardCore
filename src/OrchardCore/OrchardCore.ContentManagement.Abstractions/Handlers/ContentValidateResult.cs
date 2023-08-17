using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ContentValidateResult
    {
        private readonly List<ValidationResult> _errors = new();

        public IReadOnlyList<ValidationResult> Errors => _errors;

        /// <summary>
        /// Success may be altered by a handler during the validated async event.
        /// </summary>
        public bool Succeeded { get; set; } = true;

        public void Fail(ValidationResult error)
        {
            Succeeded = false;
            _errors.Add(error);
        }

    }
}

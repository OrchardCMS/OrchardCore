using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ContentValidateResult
    {
        private readonly List<string> _errors = new List<string>();

        public IReadOnlyList<string> Errors => _errors;

        /// <summary>
        /// Success may be altered by a handler during the validated async event.
        /// </summary>
        public bool Succeeded { get; set; } = true;

        public void Fail(params string[] errors)
        {
            Succeeded = false;
            _errors.AddRange(errors);
        }
    }
}

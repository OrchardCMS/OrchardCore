using System.Collections.Generic;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Lucene
{
    public class SearchPermissionResult
    {
        /// <summary>
        /// Returns an <see cref="SearchPermissionResult"/>indicating a successful  operation.
        /// </summary>
        public static SearchPermissionResult Success { get; } = new SearchPermissionResult { Succeeded = true };

        /// <summary>
        /// An <see cref="IEnumerable{LocalizedString}"/> containing an errors that occurred during the operation.
        /// </summary>
        public IEnumerable<LocalizedString> Errors { get; protected set; }

        /// <summary>
        /// Whether if the operation succeeded or not.
        /// </summary>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// Indicating a failed operation, with a list of errors if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="LocalizedString"/> which caused the operation to fail.</param>
        public static SearchPermissionResult Fail(params LocalizedString[] errors) => new SearchPermissionResult { Succeeded = false, Errors = errors, Failed = true, Forbidden = false };

        public bool Failed { get; protected set; }
        public bool Forbidden { get; protected set; }
        public static SearchPermissionResult Forbid() => new SearchPermissionResult { Succeeded = false, Forbidden = true, Failed = false };
    }
}



using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.Core
{
    public class DataAnnotations
    {
        private readonly IStringLocalizer<DataAnnotations> S;

        /// <summary>
        /// Creates a new instance of the <see cref="DataAnnotations"/>.
        /// </summary>
        /// <param name="localizer">The <see cref="IStringLocalizer"/>.</param>
        public DataAnnotations(IStringLocalizer<DataAnnotations> localizer)
        {
            S = localizer;
        }

        public void Localize()
        {
            var RequiredAttribute_ValidationError = S["The {0} field is required."];
        }
    }
}

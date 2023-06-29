using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.ContentManagement.GraphQL.Settings
{
    public class GraphQLContentTypePartSettings : IValidatableObject
    {
        public bool Collapse { get; set; }

        public bool Hidden { get; set; }

        public PreventFieldNameCollisionMethods PreventFieldNameCollisionMethod { get; set; }

        public string PreventFieldNameCollisionCustomValue { get; set; }

        [BindNever]
        public List<SelectListItem> AvailablePreventFieldNameCollisionMethods { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<GraphQLContentTypePartSettings>>();

            if (Collapse &&
                (PreventFieldNameCollisionMethod == PreventFieldNameCollisionMethods.AddCustomPrefix ||
                PreventFieldNameCollisionMethod == PreventFieldNameCollisionMethods.AddCustomSuffix) &&
                string.IsNullOrWhiteSpace(PreventFieldNameCollisionCustomValue))
            {
                yield return new ValidationResult(S["This field is required for selected method."], new[] { nameof(PreventFieldNameCollisionCustomValue) });
            }
        }
    }

    public enum PreventFieldNameCollisionMethods
    {
        None,
        AddPartNameSuffix,
        AddCustomPrefix,
        AddCustomSuffix,
        AddOridinalNumberSuffix
    }
}

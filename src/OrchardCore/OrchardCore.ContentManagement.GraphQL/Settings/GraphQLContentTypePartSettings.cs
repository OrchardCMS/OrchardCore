using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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
            if (!Collapse)
            {
                yield break;
            }

            var S = validationContext.GetService<IStringLocalizer<GraphQLContentTypePartSettings>>();

            switch (PreventFieldNameCollisionMethod)
            {
                case PreventFieldNameCollisionMethods.AddCustomPrefix:
                    if (!Regex.IsMatch(PreventFieldNameCollisionCustomValue, "^[_a-zA-Z][_a-zA-Z0-9]*$"))
                    {
                        yield return new ValidationResult(S["This field value must match '{0}' pattern for selected method.", "^[_a-zA-Z][_a-zA-Z0-9]*$"], new[] { nameof(PreventFieldNameCollisionCustomValue) });
                    }
                    break;
                case PreventFieldNameCollisionMethods.AddCustomSuffix:
                    if (!Regex.IsMatch(PreventFieldNameCollisionCustomValue, "^[_a-zA-Z0-9]*$"))
                    {
                        yield return new ValidationResult(S["This field value must match '{0}' pattern for selected method.", "^[_a-zA-Z0-9]*$"], new[] { nameof(PreventFieldNameCollisionCustomValue) });
                    }
                    break;
            }
        }
    }

    public enum PreventFieldNameCollisionMethods
    {
        None,
        AddPartNamePrefix,
        AddCustomPrefix,
        AddCustomSuffix
    }
}

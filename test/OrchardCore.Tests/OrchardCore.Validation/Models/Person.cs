using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Validation.Tests
{
    public class Person : ValidatableObject
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (String.IsNullOrEmpty(FirstName))
            {
                yield return new ValidationResult("The first name is required.");
            }

            if (String.IsNullOrEmpty(LastName))
            {
                yield return new ValidationResult("The last name is required.");
            }
        }
    }
}

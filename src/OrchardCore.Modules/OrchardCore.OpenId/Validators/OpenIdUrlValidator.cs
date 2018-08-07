using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OrchardCore.OpenId.Validators
{
    public class OpenIdUrlValidator
    {
        public IEnumerable<ValidationResult> ValidateUrls(string memberName, string member)
        {
            var attribute = new UrlAttribute();
            foreach (var url in member.Split(' '))
            {
                if (!attribute.IsValid(url))
                {
                    yield return new ValidationResult(attribute.FormatErrorMessage($"{memberName} ({url})"), new []{ memberName });
                }
            }
        }
    }
}
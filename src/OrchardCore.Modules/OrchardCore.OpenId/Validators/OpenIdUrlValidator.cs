using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.Validators
{
    public static class OpenIdUrlValidator
    {
        public static IEnumerable<ValidationResult> ValidateUrls(string memberName, string member)
        {
            if (member != null)
            {
                foreach (var url in member.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) || !Uri.TryCreate(url, UriKind.Absolute, out var createdUri))
                    {
                        yield return new ValidationResult($"{url} is not wellformed", new[] { memberName });
                    }
                }
            }
        }
    }
}
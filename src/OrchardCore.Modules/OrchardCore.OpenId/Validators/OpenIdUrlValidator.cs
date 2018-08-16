using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.Validators
{
    public class OpenIdUrlValidator
    {
        public IEnumerable<ValidationResult> ValidateUrls(string memberName, string member)
        {
            foreach (var url in member.Split(new []{ ' ',','},StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) || !Uri.TryCreate(url, UriKind.Absolute, out var createdUri))
                {
                    yield return new ValidationResult($"{url} is not wellformed", new[] { memberName });
                }
            }
        }
    }
}
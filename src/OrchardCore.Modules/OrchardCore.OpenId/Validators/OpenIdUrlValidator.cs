using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace OrchardCore.OpenId.Validators
{
    public static class OpenIdUrlValidator
    {
        private static IStringLocalizer S;
        public static IEnumerable<ValidationResult> ValidateUrls(this ValidationContext context, string memberName, string member)
        {
            if (member != null)
            {
                foreach (var url in member.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                    {                        
                        S ??= (IStringLocalizer)context.GetService(typeof(IStringLocalizer<Startup>));
                        yield return new ValidationResult(S["{0} is not wellformed", url], new[] { memberName });
                    }
                }
            }
        }
    }
}

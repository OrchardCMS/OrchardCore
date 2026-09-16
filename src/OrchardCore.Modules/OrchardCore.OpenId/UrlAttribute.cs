using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId;

public class UrlAttribute : ValidationAttribute
{
    private static readonly char[] s_urlSeparators = [' ', ','];

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value != null)
        {
            var urls = value.ToString();

            foreach (var url in urls.Split(s_urlSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || !uri.IsWellFormedOriginalString())
                {
                    return new ValidationResult(ErrorMessage, new[] { urls });
                }
            }
        }

        return ValidationResult.Success;
    }
}

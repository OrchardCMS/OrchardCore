using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ValidateContentContext : ContentContextBase
    {
        public ValidateContentContext(ContentItem contentItem) : base(contentItem)
        {
        }

        public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();

        public string BuildPrefix<TPart>(string memberName)
        {
            if (string.IsNullOrWhiteSpace(memberName))
                return string.Empty;
            return $"{typeof(TPart).Name}.{memberName}";
        }
    }

    public static class ValidateContentContextExtensions
    {
        public static void Fail(this ValidateContentContext context, params ValidationResult[] errors)
        {
            context.ContentValidateResult.Fail(errors);
        }
        public static void Fail<TPart>(this ValidateContentContext context, string errorMessage, params string[] memberNames) where TPart : ContentPart
        {
            if (memberNames.Any())
            {
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage, memberNames.Select(x => context.BuildPrefix<TPart>(x))));
            }
            else
            {
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage));
            }
        }
    }
}

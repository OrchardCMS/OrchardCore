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

    }

    public static class ValidateContentContextExtensions
    {

        public static void Fail(this ValidateContentContext context, params ValidationResult[] errors)
        {
            context.ContentValidateResult.Fail(errors);
        }

        public static void Fail(this ValidateContentContext context, string errorMessage, params string[] memberNames)
        {
            if (memberNames.Any())
            {
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage, memberNames));
            }
            else
            {
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage));
            }
        }
    }
}

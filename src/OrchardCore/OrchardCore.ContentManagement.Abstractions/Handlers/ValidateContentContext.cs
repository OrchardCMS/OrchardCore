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
        public static void Fail(this ValidateContentContext context, ValidationResult error)
        {
            context.ContentValidateResult.Fail(error);
        }

        public static void Fail(this ValidateContentContext context, string errorMessage, params string[] memberNames)
        {
            if (memberNames != null && memberNames.Any())
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

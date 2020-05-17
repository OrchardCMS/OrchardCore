using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ValidateContentContext : ContentContextBase
    {
        public ValidateContentContext(ContentItem contentItem) : base(contentItem)
        {
        }

        public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();

        public string Prefix { get; internal set; }
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
                var prefix = context.Prefix;
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage, memberNames.Select(x => String.IsNullOrWhiteSpace(prefix) ? x : $"{prefix}.{x}")));
            }
            else
            {
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage));
            }
        }
    }
}

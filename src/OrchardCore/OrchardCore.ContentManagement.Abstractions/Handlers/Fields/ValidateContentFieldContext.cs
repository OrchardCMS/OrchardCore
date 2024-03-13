using System.ComponentModel.DataAnnotations;

namespace OrchardCore.ContentManagement.Handlers;

public class ValidateContentFieldContext : ContentFieldContextBase
{
    public ValidateContentFieldContext(
        ContentItem contentItem)
        : base(contentItem)
    {
    }

    public ContentValidateResult ContentValidateResult { get; } = new ContentValidateResult();
}

public static class ValidateFieldContentContextExtensions
{
    public static void Fail(this ValidateContentFieldContext context, ValidationResult error)
    {
        context.ContentValidateResult.Fail(error);
    }

    public static void Fail(this ValidateContentFieldContext context, string errorMessage, string propertyName)
    {
        if (propertyName == null)
        {
            context.ContentValidateResult.Fail(new ValidationResult(errorMessage));

            return;
        }

        var memberName = $"{context.PartName}.{context.ContentPartFieldDefinition.Name}.{propertyName}";

        context.ContentValidateResult.Fail(new ValidationResult(errorMessage, new[] { memberName }));
    }
}

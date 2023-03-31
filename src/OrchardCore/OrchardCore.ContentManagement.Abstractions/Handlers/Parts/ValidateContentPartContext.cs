using System.ComponentModel.DataAnnotations;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata.Models;

public class ValidateContentPartContext : ValidateContentContext
{
    public ValidateContentPartContext(ContentItem contentItem)
        : base(contentItem)
    {
    }

    public ContentTypePartDefinition ContentTypePartDefinition { get; set; }
}

public static class ValidateContentPartContextExtensions
{
    public static void Fail(this ValidateContentPartContext context, ValidationResult error)
    {
        context.ContentValidateResult.Fail(error);
    }

    public static void Fail(this ValidateContentPartContext context, string errorMessage, string propertyName)
    {
        if (propertyName == null)
        {
            context.ContentValidateResult.Fail(new ValidationResult(errorMessage));
        }
        else
        {
            var memberName = $"{context.ContentTypePartDefinition.Name}.{propertyName}";

            context.ContentValidateResult.Fail(new ValidationResult(errorMessage, new[] { memberName }));
        }
    }
}

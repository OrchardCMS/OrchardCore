using System.ComponentModel.DataAnnotations;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ValidateFieldContentContext : ValidateContentContext
    {
        public ValidateFieldContentContext(
            ContentItem contentItem,
            ContentPartFieldDefinition contentPartFieldDefinition,
            string partName) : base(contentItem)
        {
            ContentPartFieldDefinition = contentPartFieldDefinition;
            PartName = partName;
        }

        public ContentPartFieldDefinition ContentPartFieldDefinition { get; set; }

        public string PartName { get; set; }
    }

    public static class ValidateFieldContentContextExtensions
    {

        public static void Fail(this ValidateFieldContentContext context, ValidationResult error)
        {
            context.ContentValidateResult.Fail(error);
        }

        public static void Fail(this ValidateFieldContentContext context, string errorMessage, string propertyName)
        {
            if (propertyName == null)
            {
                context.ContentValidateResult.Fail(new ValidationResult(errorMessage));
            }
            else
            {
                var memberName = $"{context.PartName}.{context.ContentPartFieldDefinition.Name}.{propertyName}";

                context.ContentValidateResult.Fail(new ValidationResult(errorMessage, new[] { memberName }));
            }
        }

        public static void Fail(this ValidateContentContext context, string errorMessage, string propertyName)
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
}

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Drivers
{
    public class FormContentDisplayDriver : ContentDisplayDriver
    {
        public override IDisplayResult Display(ContentItem model, BuildDisplayContext context)
        {
            var formItemShape = context.Shape;
            // If the content item contains FormPart add Form Wrapper only in Display type Detail.
            var formPart = model.As<FormPart>();
            if (formPart != null && context.DisplayType == "Detail")
            {
                // Add wrapper for content type if template is not available it will fall back to Form_Wrapper.
                formItemShape.Metadata.Wrappers.Add($"Form_Wrapper__{model.ContentType}");
            }

            // We don't need to return a shape result.
            return null;
        }
    }
}

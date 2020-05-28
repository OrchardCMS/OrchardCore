using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Drivers
{
    public class FormContentDisplayDriver : ContentDisplayDriver
    {
        public override Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var formItemShape = context.Shape;
            // If the content item contains FormPart add Form Wrapper.
            var formPart = model.As<FormPart>();
            if (formPart != null)
            {
                formItemShape.Metadata.Wrappers.Add("Form_Wrapper");
            }

            // We don't need to return a shape result
            return Task.FromResult<IDisplayResult>(null);
        }

        public override IDisplayResult Edit(ContentItem model)
        {

            if (model.ContentType == "Label" ||
                model.ContentType == "Button" ||
                model.ContentType == "TextArea" ||
                model.ContentType == "Input" ||
                model.ContentType == "Validation")
            {
                return Dynamic($"ContentCard_Inline", shape =>
                {
                    shape.ContentItem = model;
                    shape.Metadata.Alternates.Add($"ContentCard_Inline__{model.ContentType}");
                }).Location("Inline");
            }
            // We don't need to return a shape result
            return null;
        }
    }
}

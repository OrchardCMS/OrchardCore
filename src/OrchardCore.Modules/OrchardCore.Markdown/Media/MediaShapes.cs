using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.Markdown.Media
{
    [RequireFeatures("OrchardCore.Media")]
    public class MediaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("MarkdownBodyPart_Edit")
                .OnDisplaying(displaying =>
                {
                    var editor = displaying.Shape;

                    if (editor.Metadata.Type == "MarkdownBodyPart_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__MarkdownBodyPart");
                    }
                });

            builder.Describe("MarkdownField_Edit")
                .OnDisplaying(displaying =>
                {
                    var editor = displaying.Shape;

                    if (editor.Metadata.Type == "MarkdownField_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__MarkdownBodyPart");
                    }
                });
        }
    }
}

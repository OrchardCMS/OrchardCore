using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Html.Media
{
    public class MediaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("HtmlBodyPart_Edit")
                .OnDisplaying(displaying =>
                {
                    var editor = displaying.Shape;

                    if (editor.Metadata.Type == "HtmlBodyPart_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__HtmlBodyPart");
                    }

                    if (editor.Metadata.Type == "HtmlBodyPart_Edit__Trumbowyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__HtmlBodyPart");
                    }
                });
        }
    }
}

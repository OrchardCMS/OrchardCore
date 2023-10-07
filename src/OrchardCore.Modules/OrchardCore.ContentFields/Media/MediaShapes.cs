using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Media
{
    [RequireFeatures("OrchardCore.Media")]
    public class MediaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("HtmlField_Edit")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Type == "HtmlField_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__HtmlField");
                    }

                    if (editor.Metadata.Type == "HtmlField_Edit__Trumbowyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__HtmlField");
                    }
                });
        }
    }
}

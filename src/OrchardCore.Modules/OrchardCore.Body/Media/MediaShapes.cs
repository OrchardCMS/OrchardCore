using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Body.Media
{
    public class MediaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("HtmlBody_Editor")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Alternates.Contains("HtmlBody_Editor__Wysiwyg"))
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__HtmlBody");
                    }
                });
        }
    }
}

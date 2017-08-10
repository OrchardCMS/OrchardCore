using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;

namespace Orchard.ContentFields.Media
{
    public class MediaShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("HtmlField_Editor")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Alternates.Contains("HtmlField_Editor__Wysiwyg"))
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__Body");
                    }
                });
        }
    }
}

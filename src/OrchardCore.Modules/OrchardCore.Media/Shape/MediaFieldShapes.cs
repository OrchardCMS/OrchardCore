using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Media.Shape
{
    public class MediaFieldShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("MediaField_Edit")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Type == "MediaField_Edit")
                    {
                        editor.Metadata.Wrappers.Add("Resource_Wrapper__MediaField");
                    }

                    if (editor.Metadata.Type == "MediaField_Edit__Attached")
                    {
                        editor.Metadata.Wrappers.Add("Resource_Wrapper__MediaField");
                    }
                });
        }
    }
}
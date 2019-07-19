using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Markdown.Media
{
    public class MediaShapes : IShapeTableProvider
    {
        public Task DiscoverAsync(ShapeTableBuilder builder)
        {
            builder.Describe("MarkdownBodyPart_Edit")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Type == "MarkdownBodyPart_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__MarkdownBodyPart");
                    }
                });

            builder.Describe("MarkdownField_Edit")
                .OnDisplaying(displaying =>
                {
                    IShape editor = displaying.Shape;

                    if (editor.Metadata.Type == "MarkdownField_Edit__Wysiwyg")
                    {
                        editor.Metadata.Wrappers.Add("Media_Wrapper__MarkdownBodyPart");
                    }
                });

            return Task.CompletedTask;
        }
    }
}

using System.Collections.Generic;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ViewModels
{
    // Models an implicit part (part name == content part name). Fields are the concern of the zone / holding shape
    public class ContentPartViewModel : ShapeViewModel
    {
        public ContentPartViewModel()
        {
        }

        public ContentPartViewModel(ContentPart contentPart)
        {
            ContentPart = contentPart;
        }

        public ContentPart ContentPart { get; set; }
    }

    // Models an explicit part (part name != content type name)
    public class ExplicitContentPartViewModel : Shape
    {
        public ExplicitContentPartViewModel()
        {
        }

        public ExplicitContentPartViewModel(ContentPart contentPart)
        {
            ContentPart = contentPart;
        }

        public ContentPart ContentPart { get; set; }

        public IEnumerable<dynamic> Fields => Items;
    }
}

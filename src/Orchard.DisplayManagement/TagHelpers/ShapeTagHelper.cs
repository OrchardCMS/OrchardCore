using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.AspNet.Razor.TagHelpers;
using Orchard.DisplayManagement.Implementation;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("shape", Attributes = nameof(Type))]
    public class ShapeTagHelper : TagHelper
    {
        private readonly IShapeFactory _shapeFactory;
        private readonly IDisplayHelperFactory _displayHelperFactory;

        public string Type { get; set; }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public ShapeTagHelper(IShapeFactory shapeFactory, IDisplayHelperFactory displayHelperFactory)
        {
            _shapeFactory = shapeFactory;
            _displayHelperFactory = displayHelperFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ProcessAsync(context, output).Wait();
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var display = (DisplayHelper)_displayHelperFactory.CreateHelper(ViewContext);

            // Extract all attributes from the tag helper to
            var properties = output.Attributes
                .Where(x => x.Name != "type")
                .ToDictionary(x => x.Name, x => (object)x.Value.ToString())
                ;

            var shape = _shapeFactory.Create(Type ?? output.TagName, Arguments.From(properties));
            output.Content.SetContent(await display.ShapeExecuteAsync(shape));

            // We don't want any encapsulating tag around the shape
            output.TagName = null;
        }
    }
}
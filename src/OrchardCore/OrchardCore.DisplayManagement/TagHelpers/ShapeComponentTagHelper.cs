using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    [HtmlTargetElement("shape2", Attributes = nameof(Type))]
    [HtmlTargetElement("shape2", Attributes = PropertyPrefix + "*")]
    public class ShapeComponentTagHelper : BaseComponentShapeTagHelper
    {
        public ShapeComponentTagHelper(IShapeFactory shapeFactory, IDisplayHelper displayHelper)
            : base(shapeFactory, displayHelper)
        {
        }

        public override ValueTask<IShape> BuildComponentAsync()
        {
            // if (_options.HasComponentModel)
            // {
            //     return componentModel;
            // }
            // else
            // {
                return _shapeFactory.CreateAsync(Type, Arguments.From(NormalizedProperties));
            // }
            // return Task.CompletedTask;
        }
    }
}

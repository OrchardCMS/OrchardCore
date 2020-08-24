using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.TagHelpers;

namespace OrchardCore.Contents.TagHelpers
{
    [HtmlTargetElement("part", Attributes = "part,contentitem")]
    public class ContentPartComponentTagHelper : BaseComponentShapeTagHelper
    {
        [HtmlAttributeName("part")]

        public string ContentPart { get; set; }
        [HtmlAttributeName("contentitem")]
        public ContentItem ContentItem { get; set; }

        // private readonly ContentPartOptions
        private readonly IContentPartComponentManager _contentPartComponentManager;

        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _contentPartFactory;
        public ContentPartComponentTagHelper(
            ITypeActivatorFactory<ContentPart> contentPartFactory,
            IContentDefinitionManager contentDefinitionManager,
            IContentPartComponentManager contentPartComponentManager,
            IShapeFactory shapeFactory,
            IUpdateModelAccessor updateModelAccessor,
            IDisplayHelper displayHelper)
            : base(shapeFactory, displayHelper)
        {
            _contentPartFactory = contentPartFactory;
            _contentDefinitionManager = contentDefinitionManager;
            _contentPartComponentManager = contentPartComponentManager;
            _updateModelAccessor = updateModelAccessor;
        }

        public override async ValueTask<IShape> BuildComponentAsync()
        {
            // Here we want to resolve, it, create it via the shape factory.
            // and we need to pass it the part.

            // Here we need a func to produce it, unless it's di registered.

            // Should there be a component driver, I guess is the question I'm asking.
            // is it too much. we are essentially going for simple.
            // or should the component model, expose a BuildComponentAsync()
            // or do we just have a Func that does this stuff on

            // ok so other option, what if this just was like the displaymanager, but only for a part.
            // and it still returned a whole bunch of shapes, in zones, and we rendered all the zones in the shape.

            // that would give us listpart, and listpartfeed, and title in header, and contentsmetadata
            // Then we just iterate all of those, no special if header etc.

            // if (_options.HasComponentModel)
            // {
            //     return componentModel;
            // }
            // else
            // {

                var ctd = _contentDefinitionManager.GetTypeDefinition(ContentItem.ContentType);
                var contentTypePartDefinition = ctd.Parts.FirstOrDefault(x => x.Name == ContentPart);
                var partName = contentTypePartDefinition.Name;
                var partTypeName = contentTypePartDefinition.PartDefinition.Name;
                var partActivator = _contentPartFactory.GetTypeActivator(partTypeName);
                var part = ContentItem.Get(partActivator.Type, partName) as ContentPart;




                // var contentItem = contentPart.ContentItem.As<ContentPart>();
                return await _contentPartComponentManager.BuildDisplayAsync(part, _updateModelAccessor.ModelUpdater, "Detail");
            // }
            // return Task.CompletedTask;
        }
    }
}

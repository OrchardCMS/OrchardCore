using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// Provides a concrete content part display driver implementation for explicit parts that are dynamically defined
    /// </summary>
    public class DefaultContentPartDisplayDriver : ContentPartDisplayDriver<ContentPart>, IContentPartDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public DefaultContentPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        protected override bool CanHandleContentPart(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, out ContentPart handleable) {
            handleable = contentPart;

            if (contentPart.GetType() != typeof(ContentPart))
                return false;
            if (typePartDefinition.PartDefinition.Name == typePartDefinition.ContentTypeDefinition.Name)
                return false;// ignore implicit parts
            
            return true;
        }

        public override IDisplayResult Display(ContentPart part)
        {
            var uniqueShapeName = _typePartDefinition.Name;
            return Initialize<ExplicitContentPartViewModel>(uniqueShapeName, model => model.ContentPart = part)
                .Displaying(ctx =>
                {
                    ctx.ShapeMetadata.Alternates.Add("ContentPart");// default template for all dynamically defined, explicit content parts 
                    ctx.ShapeMetadata.Alternates.Add(uniqueShapeName);// but override "ContentPart" alternate if a specialised template exists
                }).Location("Content");
        }

    }

}

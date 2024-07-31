using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Editors
{
    public class ContentTypePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override Task<IDisplayResult> EditAsync(ContentTypePartDefinition model, BuildEditorContext context)
        {
            return Task.FromResult<IDisplayResult>(
                Shape("ContentTypePartSettings_Edit", new ShapeViewModel<ContentTypePartDefinition>(model)).Location("Content")
            );
        }
    }
}

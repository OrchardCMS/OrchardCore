using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.GraphQL.Editors
{
    public class GraphQLContentTypePartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition model, IUpdateModel updater)
        {
            return Shape("GraphQLContentTypePartSettings_Edit", new ShapeViewModel<ContentTypePartDefinition>(model)).Location("Content");
        }
    }
}
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Settings
{
    public class GraphQLContentTypePartSettings
    {
        public bool CollapseFieldsToParent { get; set; }
    }

    public class GraphQLContentTypePartDefinition : ContentDefinition
    {
        public GraphQLContentTypePartDefinition(string name, ContentPartDefinition contentPartDefinition, JObject settings)
        {
            Name = name;
            PartDefinition = contentPartDefinition;
            Settings = settings;
        }
    }
}
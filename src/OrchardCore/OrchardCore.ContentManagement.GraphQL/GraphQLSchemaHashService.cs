using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentManagement.GraphQL
{
    public class GraphQLSchemaHashService : IGraphQLSchemaHashService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public GraphQLSchemaHashService(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public Task<int> GetHash()
        {
            return _contentDefinitionManager.GetTypesHashAsync();
        }
    }
}

using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.Contents.GraphQL
{
    public class GraphQLSchemaHashService : IGraphQLSchemaHashService
    {
        public Task<int> GetHash()
        {
            return Task.FromResult(0);
        }
    }
}

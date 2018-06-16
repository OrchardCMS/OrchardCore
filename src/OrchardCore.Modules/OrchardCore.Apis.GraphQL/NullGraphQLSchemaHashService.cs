using System.Threading.Tasks;

namespace OrchardCore.Apis.GraphQL
{
    public class NullGraphQLSchemaHashService : IGraphQLSchemaHashService
    {
        public Task<int> GetHash()
        {
            return Task.FromResult(0);
        }
    }
}

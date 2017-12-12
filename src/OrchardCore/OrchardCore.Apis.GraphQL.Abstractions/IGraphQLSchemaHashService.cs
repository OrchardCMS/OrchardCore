using System.Threading.Tasks;

namespace OrchardCore.Apis.GraphQL
{
    public interface IGraphQLSchemaHashService
    {
        Task<int> GetHash();
    }
}

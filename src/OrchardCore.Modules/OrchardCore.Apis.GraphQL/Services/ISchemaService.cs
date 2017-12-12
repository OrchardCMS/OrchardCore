using System.Threading.Tasks;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Services
{
    public interface ISchemaService
    {
        Task<ISchema> GetSchema();
    }
}

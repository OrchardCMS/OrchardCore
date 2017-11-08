using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public interface IDynamicQueryFieldTypeProvider
    {
        Task<IEnumerable<FieldType>> GetFields();
    }
}
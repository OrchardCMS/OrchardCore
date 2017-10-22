using System.Collections.Generic;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public interface IDynamicQueryFieldTypeProvider
    {
        IEnumerable<FieldType> GetFields();
    }
}
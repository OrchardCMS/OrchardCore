using System.Collections.Generic;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Schema
{
    public class ObjectGraphTypeProvider : IObjectGraphTypeProvider
    {
        private readonly IEnumerable<ContentPart> _contentParts;

        public ObjectGraphTypeProvider(
            IEnumerable<ContentPart> contentParts)

        {
            _contentParts = contentParts;
        }

        public void Register(global::GraphQL.Types.Schema schema)
        {
            foreach (var contentPart in _contentParts)
            {
                schema.RegisterType(new ContentPartAutoRegisteringObjectGraphType(contentPart));
            }
        }
    }
}

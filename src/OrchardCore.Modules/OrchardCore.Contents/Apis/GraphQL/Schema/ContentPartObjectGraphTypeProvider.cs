using System.Collections.Generic;
using System.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Schema
{
    public class ContentPartObjectGraphTypeProvider : IObjectGraphTypeProvider
    {
        private readonly IEnumerable<ContentPart> _contentParts;

        public ContentPartObjectGraphTypeProvider(
            IEnumerable<ContentPart> contentParts)
        {
            _contentParts = contentParts;
        }

        public void Register(global::GraphQL.Types.Schema schema)
        {
            foreach (var contentPart in _contentParts)
            {
                var input = new InputContentPartAutoRegisteringObjectGraphType(contentPart);
                var filter = new ContentPartAutoRegisteringObjectGraphType(contentPart);

                if (input.Fields.Any())
                {
                    schema.RegisterType(input);
                }
                if (filter.Fields.Any())
                {
                    schema.RegisterType(filter);
                }
            }
        }
    }
}

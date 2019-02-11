using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLContentTypeOption
    {
        public string ContentType { get; set; }

        public bool Collapse { get; set; }

        public bool Ignore { get; set; }

        public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<GraphQLContentPartOption>();
    }


}
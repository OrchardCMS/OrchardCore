using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLContentTypeOption
    {
        public GraphQLContentTypeOption(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            ContentType = contentType;
        }

        public string ContentType { get; }

        public bool Collapse { get; set; }

        public bool Hidden { get; set; }

        public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<GraphQLContentPartOption>();

        public GraphQLContentTypeOption ConfigurePart<TContentPart>(Action<GraphQLContentPartOption> action)
            where TContentPart : ContentPart
        {
            var option = new GraphQLContentPartOption<TContentPart>();

            action(option);

            PartOptions = PartOptions.Union(new[] { option });

            return this;
        }

        public GraphQLContentTypeOption ConfigurePart(string partName, Action<GraphQLContentPartOption> action)
        {
            var option = new GraphQLContentPartOption(partName);

            action(option);

            PartOptions = PartOptions.Union(new[] { option });

            return this;
        }
    }
}

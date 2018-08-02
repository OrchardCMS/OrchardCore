using System.Collections.Generic;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.ContentManagement.GraphQL.Mutations.Types
{
    public class CreateContentItemInputType : InputObjectGraphType<ContentItem>
    {
        public CreateContentItemInputType()
        {
            Field(ci => ci.ContentType, false);
            Field(ci => ci.Author, true);
            Field(ci => ci.Owner, true);

            Field(ci => ci.Published, true);
            Field(ci => ci.Latest, true);

            Field<ContentPartsInputType>("ContentParts");
        }
    }

    public class ContentPartsInputType : InputObjectGraphType
    {
        public ContentPartsInputType(
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<ContentPart> contentParts,
            ITypeActivatorFactory<ContentPart> typeActivator)
        {
            Name = "ContentPartsInput";

            foreach (var part in contentParts)
            {
                var partName = part.GetType().Name;
                var activator = typeActivator.GetTypeActivator(partName);

                var inputGraphType =
                    typeof(InputObjectGraphType<>).MakeGenericType(activator.Type);

                var inputGraphTypeResolved = (IInputObjectGraphType)httpContextAccessor.HttpContext
                    .RequestServices.GetService(inputGraphType);

                if (inputGraphTypeResolved != null)
                {
                    Field(
                        inputGraphTypeResolved.GetType(),
                        partName);
                }
            }
        }
    }
}

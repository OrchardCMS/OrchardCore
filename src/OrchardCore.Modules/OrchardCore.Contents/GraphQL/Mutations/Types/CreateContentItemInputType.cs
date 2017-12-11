using System;
using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.GraphQL.Mutations.Types
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
            IServiceProvider serviceProvider,
            IEnumerable<ContentPart> _contentParts)
        {
            Name = "ContentPartsInput";

            foreach (var contentPart in _contentParts)
            {
                var inputGraphType =
                    typeof(InputObjectGraphType<>).MakeGenericType(contentPart.GetType());

                var inputGraphTypeResolved = (IInputObjectGraphType)serviceProvider.GetService(inputGraphType);

                if (inputGraphTypeResolved != null)
                {
                    var name = contentPart.GetType().Name; // About

                    Field(
                        inputGraphTypeResolved.GetType(),
                        name);
                }
            }
        }
    }
}

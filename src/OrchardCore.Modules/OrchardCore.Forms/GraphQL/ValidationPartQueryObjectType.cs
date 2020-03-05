using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL
{
    public class ValidationPartQueryObjectType : ObjectGraphType<ValidationPart>
    {
        public ValidationPartQueryObjectType()
        {
            Name = "ValidationPart";

            Field(x => x.For, nullable: true);
        }
    }
}

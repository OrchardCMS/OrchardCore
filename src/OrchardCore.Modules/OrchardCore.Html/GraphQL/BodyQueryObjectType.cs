using GraphQL.Types;
using OrchardCore.Body.Model;

namespace OrchardCore.Body.GraphQL
{
    public class BodyQueryObjectType : ObjectGraphType<BodyPart>
    {
        public BodyQueryObjectType()
        {
            Name = "BodyPart";

            Field(x => x.Body);
        }
    }
}

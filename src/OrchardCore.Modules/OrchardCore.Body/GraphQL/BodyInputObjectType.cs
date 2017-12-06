using GraphQL.Types;
using OrchardCore.Body.Model;

namespace OrchardCore.Body.GraphQL
{
    public class BodyInputObjectType : InputObjectGraphType<BodyPart>
    {
        public BodyInputObjectType()
        {
            Name = "BodyPartInput";

            Field(x => x.Body, false);
        }
    }
}

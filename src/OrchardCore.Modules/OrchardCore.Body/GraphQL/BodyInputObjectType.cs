using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Body.Model;

namespace OrchardCore.Body.GraphQL
{
    public class BodyInputObjectType : QueryArgumentObjectGraphType<BodyPart>
    {
        public BodyInputObjectType()
        {
            Name = "BodyPartInput";

            AddInputField("body", x => x.Body, true);
        }
    }
}

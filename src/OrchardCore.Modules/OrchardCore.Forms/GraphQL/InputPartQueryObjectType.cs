using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL
{
    public class InputPartQueryObjectType : ObjectGraphType<InputPart>
    {
        public InputPartQueryObjectType()
        {
            Name = "InputPart";

            Field(x => x.Type, nullable: true);
            Field(x => x.Placeholder, nullable: true);
            Field(x => x.DefaultValue, nullable: true);
        }
    }
}

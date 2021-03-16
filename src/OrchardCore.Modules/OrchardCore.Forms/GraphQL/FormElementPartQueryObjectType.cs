using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL
{
    public class FormElementPartQueryObjectType : ObjectGraphType<FormElementPart>
    {
        public FormElementPartQueryObjectType()
        {
            Name = "FormElementPart";

            Field(x => x.Id, nullable: true);
        }
    }
}

using GraphQL.Types;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.GraphQL
{
    public class FormPartQueryObjectType : ObjectGraphType<FormPart>
    {
        public FormPartQueryObjectType()
        {
            Name = "FormPart";

            Field(x => x.WorkflowTypeId, nullable: true);
            Field(x => x.Action, nullable: true);
            Field(x => x.Method, nullable: true);
        }
    }
}

using GraphQL.Types;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.GraphQL
{
    public class FlowMetadataQueryObjectType : ObjectGraphType<FlowMetadata>
    {
        public FlowMetadataQueryObjectType()
        {
            Name = "FlowMetadata";

            Field(x => x.Size, nullable: true);
            Field<FlowAlignmentEnum>("alignment");
        }
    }
}

using GraphQL.Types;

namespace OrchardCore.Flows.GraphQL
{
    public class FlowAlignmentEnum : EnumerationGraphType
    {
        public FlowAlignmentEnum()
        {
            Name = "FlowAlignment";

            Description = "The widget alignment.";
            AddValue("Left", "Left alignment.", 0);
            AddValue("Center", "Center alignment.", 1);
            AddValue("Right", "Right alignment.", 2);
            AddValue("Justify", "Justify alignment.", 3);
        }
    }
}

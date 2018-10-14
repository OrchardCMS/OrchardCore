using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class TimeFieldQueryObjectType : ObjectGraphType<TimeField>
    {
        public TimeFieldQueryObjectType()
        {
            Name = "TimeField";

            Field(x => x.Value);
        }
    }
}

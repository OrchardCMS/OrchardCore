using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Apis.GraphQL.Types
{
    public class ContentPartInterface : InterfaceGraphType<ContentElement>
    {
        public ContentPartInterface()
        {
            Name = "ContentPart";
        }
    }
}

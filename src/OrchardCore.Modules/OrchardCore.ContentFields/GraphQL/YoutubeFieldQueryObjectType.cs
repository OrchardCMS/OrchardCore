using GraphQL.Types;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL
{
    public class YoutubeFieldQueryObjectType : ObjectGraphType<YoutubeField>
    {
        public YoutubeFieldQueryObjectType()
        {
            Name = "YoutubeField";

            Field(x => x.EmbeddedAddress);
            Field(x => x.RawAddress);
        }
    }
}

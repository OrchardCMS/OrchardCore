using GraphQL.Types;
using OrchardCore.Title.Model;

namespace OrchardCore.Apis.GraphQL.Types
{
    public class TitlePartType : AutoRegisteringObjectGraphType<TitlePart>
    {
        public TitlePartType()
        {
            Name = typeof(TitlePart).Name;

            Interface<ContentPartInterface>();

            IsTypeOf = value => value is TitlePart;
        }
    }
}

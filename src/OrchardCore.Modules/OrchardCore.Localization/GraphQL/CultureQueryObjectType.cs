using GraphQL.Types;

namespace OrchardCore.Localization.GraphQL
{
    public class CultureQueryObjectType : ObjectGraphType<SiteCulture>
    {
        public CultureQueryObjectType()
        {
            Name = "SiteCulture";

            Field<StringGraphType>("culture", "The culture code.", resolve: context => context.Source.Culture);
            Field<BooleanGraphType>("default", "Whether this is the default culture.", resolve: context => context.Source.IsDefault);
        }
    }
}

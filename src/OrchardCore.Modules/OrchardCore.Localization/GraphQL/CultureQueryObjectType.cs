using GraphQL.Types;

namespace OrchardCore.Localization.GraphQL
{
    public class CultureQueryObjectType : ObjectGraphType<SiteCulture>
    {
        public CultureQueryObjectType()
        {
            Name = "SiteCulture";

            Field<StringGraphType>("locale", "The language locale code.", resolve: context => context.Source.Locale);
            Field<BooleanGraphType>("default", "Whether this is the default culture.", resolve: context => context.Source.IsDefault);
        }
    }
}
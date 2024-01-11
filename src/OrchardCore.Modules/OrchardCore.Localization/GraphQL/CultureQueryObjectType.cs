using GraphQL.Types;

namespace OrchardCore.Localization.GraphQL
{
    /// <summary>
    /// Represents a culture object for Graph QL.
    /// </summary>
    public class CultureQueryObjectType : ObjectGraphType<SiteCulture>
    {
        /// <summary>
        /// Creates a new instance of <see cref="CultureQueryObjectType"/>.
        /// </summary>
        public CultureQueryObjectType()
        {
            Name = "SiteCulture";

            Field<StringGraphType>("culture", "The culture code.", resolve: context => context.Source.Culture);
            Field<BooleanGraphType>("default", "Whether this is the default culture.", resolve: context => context.Source.IsDefault);
        }
    }
}

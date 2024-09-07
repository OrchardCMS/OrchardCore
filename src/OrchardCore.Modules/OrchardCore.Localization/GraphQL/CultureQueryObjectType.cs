using GraphQL.Types;

namespace OrchardCore.Localization.GraphQL;

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

        Field<StringGraphType>("culture").Description("The culture code.").Resolve(context => context.Source.Culture);
        Field<BooleanGraphType>("default").Description("Whether this is the default culture.").Resolve(context => context.Source.IsDefault);
    }
}

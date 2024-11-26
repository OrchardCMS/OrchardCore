using GraphQL.Types;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.GraphQL;

/// <summary>
/// Represents a culture object for Graph QL.
/// </summary>
public class CultureQueryObjectType : ObjectGraphType<SiteCulture>
{
    /// <summary>
    /// Creates a new instance of <see cref="CultureQueryObjectType"/>.
    /// </summary>
    public CultureQueryObjectType(IStringLocalizer<CultureQueryObjectType> S)
    {
        Name = "SiteCulture";

        Field<StringGraphType>("culture")
            .Description(S["The culture code."])
            .Resolve(context => context.Source.Culture);

        Field<BooleanGraphType>("default")
            .Description(S["Whether this is the default culture."])
            .Resolve(context => context.Source.IsDefault);
    }
}

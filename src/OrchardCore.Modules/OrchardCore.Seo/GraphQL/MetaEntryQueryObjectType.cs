using GraphQL.Types;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Seo.GraphQL;

public class MetaEntryQueryObjectType : ObjectGraphType<MetaEntry>
{
    public MetaEntryQueryObjectType(IStringLocalizer<MetaEntryQueryObjectType> S)
    {
        Name = nameof(MetaEntry);
        Description = S["Meta entry"];

        Field(x => x.Name)
            .Description(S["Name of the meta entry"]);

        Field(x => x.Property)
            .Description(S["Property of the meta entry"]);

        Field(x => x.Content)
            .Description(S["Content of the meta entry"]);

        Field(x => x.HttpEquiv)
            .Description(S["HttpEquiv of the meta entry"]);

        Field(x => x.Charset)
            .Description(S["Charset of the meta entry"]);

        Field<StringGraphType>("Tag")
            .Description(S["The generated tag of the meta entry"])
            .Resolve(ctx =>
            {
                using var writer = new StringWriter();
                ctx.Source.GetTag().WriteTo(writer, NullHtmlEncoder.Default);
                return writer.ToString();
            });
    }
}

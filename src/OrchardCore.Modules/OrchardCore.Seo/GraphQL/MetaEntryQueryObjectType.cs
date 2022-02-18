using System.IO;
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
            .Description("Name of the meta entry");
        Field(x => x.Property)
            .Description("Property of the meta entry");
        Field(x => x.Content)
            .Description("Content of the meta entry");
        Field(x => x.HttpEquiv)
            .Description("HttpEquiv of the meta entry");
        Field(x => x.Charset)
            .Description("Charset of the meta entry");
        Field<StringGraphType>()
            .Name("Tag")
            .Description("The generated tag of the meta entry")
            .Resolve(ctx =>
            {
                using var writer = new StringWriter();
                ctx.Source.GetTag().WriteTo(writer, NullHtmlEncoder.Default);
                return writer.ToString();
            });
    }
}

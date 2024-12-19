using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentFields.Fields;

namespace OrchardCore.ContentFields.GraphQL.Types;

public class LinkFieldQueryObjectType : ObjectGraphType<LinkField>
{
    public LinkFieldQueryObjectType(IStringLocalizer<LinkFieldQueryObjectType> S)
    {
        Name = nameof(LinkField);

        Field(x => x.Url, nullable: true)
            .Description(S["the url of the link"]);

        Field(x => x.Text, nullable: true)
            .Description(S["the text of the link"]);

        Field(x => x.Target, nullable: true)
            .Description(S["the target of the link"]);
    }
}

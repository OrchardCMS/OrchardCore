using GraphQL.Types;
using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public sealed class PublicationStatusGraphType : EnumerationGraphType
{
    public PublicationStatusGraphType(IStringLocalizer<PublicationStatusGraphType> S)
    {
        Name = "Status";
        Description = S["publication status"];
        Add("PUBLISHED", PublicationStatusEnum.Published, S["published content item version"]);
        Add("DRAFT", PublicationStatusEnum.Draft, S["draft content item version"]);
        Add("LATEST", PublicationStatusEnum.Latest, S["the latest version, either published or draft"]);
        Add("ALL", PublicationStatusEnum.All, S["all historical versions"]);
    }
}

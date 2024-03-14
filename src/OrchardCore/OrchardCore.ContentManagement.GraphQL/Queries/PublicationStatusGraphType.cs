using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

public class PublicationStatusGraphType : EnumerationGraphType
{

    public PublicationStatusGraphType()
    {
        Name = "Status";
        Description = "publication status";
        Add("PUBLISHED", PublicationStatusEnum.Published, "published content item version");
        Add("DRAFT", PublicationStatusEnum.Draft, "draft content item version");
        Add("LATEST", PublicationStatusEnum.Latest, "the latest version, either published or draft");
        Add("ALL", PublicationStatusEnum.All, "all historical versions");
    }
}

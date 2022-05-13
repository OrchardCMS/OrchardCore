using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public enum PublicationStatusEnum
    {
        PublishedOrLatest,
        Published,
        Draft,
        Latest,
        All
    }

    public class PublicationStatusGraphType : EnumerationGraphType
    {
        public PublicationStatusGraphType()
        {
            Name = "Status";
            Description = "publication status";
            AddValue("PublishedOrLatest", "published or latest version content item", PublicationStatusEnum.PublishedOrLatest);
            AddValue("PUBLISHED", "published content item version", PublicationStatusEnum.Published);
            AddValue("DRAFT", "draft content item version", PublicationStatusEnum.Draft);
            AddValue("LATEST", "the latest version, either published or draft", PublicationStatusEnum.Latest);
            AddValue("ALL", "all historical versions", PublicationStatusEnum.All);
        }
    }
}

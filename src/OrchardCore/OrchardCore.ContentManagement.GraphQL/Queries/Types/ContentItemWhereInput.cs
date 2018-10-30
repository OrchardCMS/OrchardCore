using System;
using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentItemWhereInputModel
    {
        public PublicationStatusEnum Status { get; set; }

        public string ContentItemId { get; set; }

        public string ContentItemVersionId { get; set; }

        public DateTime? ModifiedUtc { get; set; }

        public DateTime? PublishedUtc { get; set; }

        public DateTime? CreatedUtc { get; set; }

        public string Owner { get; set; }

        public string Author { get; set; }

        public string DisplayText { get; set; }
    }

    public class ContentItemWhereInput : InputObjectGraphType<ContentItemWhereInputModel>
    {
        public ContentItemWhereInput()
        {
            Name = nameof(ContentItemWhereInput);

            Field(x => x.Status, true, typeof(PublicationStatusGraphType))
                .Description("publication status of the content item")
                .DefaultValue();

            Field(x => x.ContentItemId, true).Description("content item id");
            Field(x => x.DisplayText, true).Description("the display text of the content item");
            Field(x => x.CreatedUtc, true).Description("the date and time of creation");
            Field(x => x.ModifiedUtc, true).Description("the date and time of modification");
            Field(x => x.PublishedUtc, true).Description("the date and time of publication");
            Field(x => x.Owner, true).Description("the owner of the content item");
            Field(x => x.Author, true).Description("the author of the content item");
            Field(x => x.ContentItemVersionId, true).Description("the id of the version");
        }
    }
}
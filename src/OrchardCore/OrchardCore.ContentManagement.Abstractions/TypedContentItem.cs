using System;

namespace OrchardCore.ContentManagement
{
    /// <summary>
    /// Implement to use a strongly typed content type model.
    /// All proxied properties must be virtual, and content fields only support get.
    /// </summary>
    public abstract class TypedContentItem
    {
        public TypedContentItem(ContentItem contentItem)
        {
            ContentItem = contentItem;
        }

        // ContentItem is not proxied, or settable.
        public ContentItem ContentItem { get; }

        // Well known properties of a content item.
        public virtual int Id { get; set; }
        public virtual string ContentItemId { get; set; }
        public virtual string ContentItemVersionId { get; set; }
        public virtual string ContentType { get; set; }
        public virtual bool Published { get; set; }
        public virtual bool Latest { get; set; }
        public virtual DateTime? ModifiedUtc { get; set; }
        public virtual DateTime? PublishedUtc { get; set; }
        public virtual DateTime? CreatedUtc { get; set; }
        public virtual string Owner { get; set; }
        public virtual string Author { get; set; }
        public virtual string DisplayText { get; set; }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(DisplayText) ? $"{ContentType} ({ContentItemId})" : DisplayText;
        }
    }
}

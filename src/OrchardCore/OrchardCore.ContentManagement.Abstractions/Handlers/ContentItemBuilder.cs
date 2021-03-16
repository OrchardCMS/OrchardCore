using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Handlers
{
    /// <summary>
    /// Builds a content item based on its the type definition (<seealso cref="ContentTypeDefinition"/>).
    /// </summary>
    public class ContentItemBuilder
    {
        private readonly ContentItem _item;

        /// <summary>
        /// Constructs a new Content Item Builder instance.
        /// </summary>
        /// <param name="definition">The definition for the content item to be built.</param>
        public ContentItemBuilder(ContentTypeDefinition definition)
        {
            // TODO: could / should be done on the build method ?
            _item = new ContentItem
            {
                ContentType = definition.Name
            };
        }

        public ContentItem Build()
        {
            return _item;
        }

        /// <summary>
        /// Welds a part to the content item.
        /// </summary>
        public ContentItemBuilder Weld(ContentPart contentPart)
        {
            _item.Weld(contentPart.GetType().Name, contentPart);
            return this;
        }

        /// <summary>
        /// Welds a named part to the content item.
        /// </summary>
        public ContentItemBuilder Weld(string name, ContentPart contentPart)
        {
            _item.Weld(name, contentPart);
            return this;
        }
    }
}

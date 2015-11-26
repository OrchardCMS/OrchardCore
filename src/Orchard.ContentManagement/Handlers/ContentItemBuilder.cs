using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Handlers
{
    /// <summary>
    /// Builds a contentitem based on its the type definition (<seealso cref="ContentTypeDefinition"/>).
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
        /// Welds a new part to the content item. If a part of the same type is already welded nothing is done.
        /// </summary>
        /// <typeparam name="TPart">The type of the part to be welded.</typeparam>
        /// <returns>A new Content Item Builder with the item having the new part welded.</returns>
        public ContentItemBuilder Weld<TPart>() where TPart : ContentPart, new()
        {
            // if the part hasn't be weld yet
            if (!_item.Has<TPart>())
            {
                var partName = typeof(TPart).Name;

                // build and weld the part
                var part = new TPart();
                _item.Weld(part);
            }

            return this;
        }

        /// <summary>
        /// Welds a part to the content item.
        /// </summary>
        public ContentItemBuilder Weld(ContentPart contentPart)
        {
            _item.Weld(contentPart);
            return this;
        }
    }
}
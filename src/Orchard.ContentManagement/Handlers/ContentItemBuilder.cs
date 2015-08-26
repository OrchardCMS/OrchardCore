using System.Linq;
using Orchard.ContentManagement.MetaData.Models;

namespace Orchard.ContentManagement.Handlers {
    /// <summary>
    /// Builds a contentitem based on its the type definition (<seealso cref="ContentTypeDefinition"/>).
    /// </summary>
    public class ContentItemBuilder {
        private readonly ContentTypeDefinition _definition;
        private readonly ContentItem _item;

        /// <summary>
        /// Constructs a new Content Item Builder instance.
        /// </summary>
        /// <param name="definition">The definition for the content item to be built.</param>
        public ContentItemBuilder(ContentTypeDefinition definition) {
            _definition = definition;

            // TODO: could / should be done on the build method ?
            _item = new ContentItem {
                ContentType = definition.Name,
                TypeDefinition = definition
            };
        }

        public ContentItem Build() {
            return _item;
        }

        /// <summary>
        /// Welds a new part to the content item. If a part of the same type is already welded nothing is done.
        /// </summary>
        /// <typeparam name="TPart">The type of the part to be welded.</typeparam>
        /// <returns>A new Content Item Builder with the item having the new part welded.</returns>
        public ContentItemBuilder Weld<TPart>() where TPart : ContentPart, new() {

            // if the part hasn't be weld yet
            if (_item.Parts.FirstOrDefault(part => part.GetType().Equals(typeof(TPart))) == null) {
                var partName = typeof(TPart).Name;

                // obtain the type definition for the part
                var typePartDefinition = _definition.Parts.FirstOrDefault(p => p.PartDefinition.Name == partName);
                if (typePartDefinition == null) {
                    // If the content item's type definition does not define the part; use an empty type definition.
                    typePartDefinition = new ContentTypePartDefinition(
                        new ContentPartDefinition(partName),
                        new SettingsDictionary());
                }

                // build and weld the part
                var part = new TPart { TypePartDefinition = typePartDefinition };
                _item.Weld(part);
            }

            return this;
        }

        /// <summary>
        /// Welds a part to the content item.
        /// </summary>
        public ContentItemBuilder Weld(ContentPart contentPart) {
            _item.Weld(contentPart);
            return this;
        }
    }
}

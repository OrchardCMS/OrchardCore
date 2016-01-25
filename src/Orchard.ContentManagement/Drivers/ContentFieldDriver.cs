using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers
{
    public abstract class ContentFieldDriver<TField> : IContentFieldDriver where TField : class, IContent, new()
    {
        private static readonly ContentFieldInfo _contentFieldInfo;

        static ContentFieldDriver()
        {
            _contentFieldInfo = new ContentFieldInfo
            {
                FieldTypeName = typeof(TField).Name,
                Factory = partFieldDefinition => new TField()
            };
            
        }

        public void GetContentItemMetadata(IContent contentPart, IContent contentField, ContentItemMetadataContext context)
        {
            var field = contentField as TField;

            if (field == null)
            {
                return;
            }

            GetContentItemMetadata(contentPart, field, context.Metadata);
        }

        public ContentFieldInfo GetFieldInfo()
        {
            return _contentFieldInfo;
        }

        protected virtual void GetContentItemMetadata(IContent part, TField field, ContentItemMetadata metadata) { }
        
    }
}
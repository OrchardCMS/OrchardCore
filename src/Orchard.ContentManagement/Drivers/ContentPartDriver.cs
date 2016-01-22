using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;

namespace Orchard.ContentManagement.Drivers
{
    public abstract class ContentPartDriver<TPart> : IContentPartDriver where TPart : class, IContent, new()
    {
        private static readonly ContentPartInfo _contentPartInfo;

        static ContentPartDriver()
        {
            _contentPartInfo = new ContentPartInfo
            {
                PartName = typeof(TPart).Name,
                Factory = typePartDefinition => new TPart()
            };
        }

        public void GetContentItemMetadata(IContent contentPart, ContentItemMetadataContext context)
        {
            var part = contentPart as TPart;

            if (part == null)
            {
                return;
            }

            GetContentItemMetadata(part, context.Metadata);
        }

        public ContentPartInfo GetPartInfo()
        {
            return _contentPartInfo;
        }

        protected virtual void GetContentItemMetadata(TPart part, ContentItemMetadata metadata) { }

    }
}
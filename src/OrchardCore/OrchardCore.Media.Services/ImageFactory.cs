using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Media.Models;

namespace OrchardCore.Media
{

    public class ImageFactorySelector : IMediaFactorySelector
    {
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ImageFactorySelector(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public MediaFactorySelectorResult GetMediaFactory(Stream stream, string fileName, string mimeType, string contentType)
        {
            if (!mimeType.StartsWith("image/"))
            {
                return null;
            }

            if (!String.IsNullOrEmpty(contentType))
            {
                var contentDefinition = _contentDefinitionManager.GetTypeDefinition(contentType);
                if (contentDefinition == null || contentDefinition.Parts.All(x => x.PartDefinition.Name != typeof(ImageMediaPart).Name))
                {
                    return null;
                }
            }

            return new MediaFactorySelectorResult
            {
                Priority = -5,
                MediaFactory = new ImageFactory(_contentManager)
            };
        }
    }

    public class ImageFactory : IMediaFactory
    {
        private readonly IContentManager _contentManager;

        public ImageFactory(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<IContent> CreateMediaAsync(Stream stream, string path, string mimeType, long length, string contentType)
        {
            if (String.IsNullOrEmpty(contentType))
            {
                contentType = "Image";
            }

            var media = await _contentManager.NewAsync(contentType);

            media.Alter<ImageMediaPart>(imagePart =>
            {
                imagePart.Length = length;
                imagePart.MimeType = mimeType;
                imagePart.Path = path;
            });

            return media;
        }
    }
}
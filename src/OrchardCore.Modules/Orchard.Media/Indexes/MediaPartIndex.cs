using System.IO;
using Orchard.ContentManagement;
using Orchard.Media.Models;
using YesSql.Core.Indexes;

namespace Orchard.Media.Indexes
{
    public class MediaPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string MimeType { get; set; }
        public string Folder { get; set; }
        public string FileName { get; set; }
        public long Length { get; set; }
    }

    public class MediaPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<MediaPartIndex>()
                .Map(contentItem =>
                {

                    if(!contentItem.Latest)
                    {
                        return null;
                    }

                    var imagePart = contentItem.As<ImagePart>();

                    if (imagePart != null)
                    {
                        // Calling Path.GetDirectoryName would convert '/' to '\'
                        var filename = Path.GetFileName(imagePart.Path);
                        var folder = imagePart.Path.Substring(0, imagePart.Path.Length - filename.Length).TrimEnd('/');

                        return new MediaPartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            MimeType = imagePart.MimeType.ToLowerInvariant(),
                            Folder = folder,
                            FileName = filename,
                            Length = imagePart.Length
                        };
                    }

                    return null;
                });
        }
    }
}
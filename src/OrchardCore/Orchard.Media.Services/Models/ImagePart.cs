using Orchard.ContentManagement;

namespace Orchard.Media.Models
{
    public class ImagePart : ContentPart
    {
        public string MimeType { get; set; }
        public long Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Path { get; set; }
    }
}

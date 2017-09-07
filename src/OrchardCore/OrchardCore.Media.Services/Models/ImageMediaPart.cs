using OrchardCore.ContentManagement;

namespace OrchardCore.Media.Models
{
    public class ImageMediaPart : ContentPart
    {
        public string MimeType { get; set; }
        public long Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Path { get; set; }
    }
}

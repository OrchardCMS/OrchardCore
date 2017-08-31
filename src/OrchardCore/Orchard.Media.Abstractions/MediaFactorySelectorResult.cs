namespace Orchard.Media
{
    public class MediaFactorySelectorResult
    {
        public int Priority { get; set; }
        public IMediaFactory MediaFactory { get; set; }
    }
}
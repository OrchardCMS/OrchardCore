namespace OrchardCore.Media
{
    public class MediaFactorySelectorResult
    {
        public int Priority { get; set; }
        public IMediaFactory MediaFactory { get; set; }
    }
}
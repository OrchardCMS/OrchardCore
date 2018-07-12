namespace OrchardCore.ContentFields.Settings
{
    public class VideoFieldSettings
    {
        public string Hint { get; set; }
        public string Label { get; set; }
        public string Editor { get; set; }
        public VideoStreamingType StreamingType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}

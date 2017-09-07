namespace OrchardCore.Media.Settings
{
    public class MediaFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; } = true;
    }
}
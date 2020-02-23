namespace OrchardCore.Media.Events
{
    public class MediaMoveContext
    {
        public string NewPath { get; set; }
        public string OldPath { get; set; }
    }
}

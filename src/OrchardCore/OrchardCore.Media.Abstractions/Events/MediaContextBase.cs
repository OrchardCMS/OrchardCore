namespace OrchardCore.Media.Events
{
    public class MediaContextBase
    {
        /// <summary>
        /// The path of the file for the current filestore operation.
        /// </summary>
        public string Path { get; set; }
    }
}

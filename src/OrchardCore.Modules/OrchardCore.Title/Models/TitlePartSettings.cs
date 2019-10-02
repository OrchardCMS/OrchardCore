namespace OrchardCore.Title.Models
{
    public class TitlePartSettings
    {
        /// <summary>
        /// Gets or sets whether a user can define a custom title
        /// </summary>
        public bool? AllowCustomTitle { get; set; }

        /// <summary>
        /// The pattern used to build the Title.
        /// </summary>
        public string Pattern { get; set; }
    }
}
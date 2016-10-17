namespace Orchard.Autoroute.Models
{
    public class AutoroutePartSettings
    {
        /// <summary>
        /// Gets or sets whether a user can define a custom path.
        /// </summary>
        public bool AllowCustomPath { get; set; }

        /// <summary>
        /// The pattern used to build the Path.
        /// </summary>
        public string Pattern { get; set; }
    }
}
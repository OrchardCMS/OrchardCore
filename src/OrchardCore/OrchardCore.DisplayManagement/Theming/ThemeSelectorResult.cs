namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// Represents a result for a selected theme.
    /// </summary>
    public class ThemeSelectorResult
    {
        /// <summary>
        /// Gets or sets the priority for the selected theme.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the theme name.
        /// </summary>
        /// <remarks>Avoid to use the theme name acciedently. <see cref="ThemeSelectorResult"/> is using the theme identifier instead.</remarks>
        public string ThemeName { get; set; }
    }
}

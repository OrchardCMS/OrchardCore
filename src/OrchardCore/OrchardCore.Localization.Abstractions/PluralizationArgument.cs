namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a pluralization argument.
    /// </summary>
    public struct PluralizationArgument
    {
        /// <summary>
        /// Gets or sets the number to be used for selecting the proper pluralization form.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the pluralization forms.
        /// </summary>
        public string[] Forms { get; set; }

        /// <summary>
        /// Gets or sets the parameters that could be used in localization.
        /// </summary>
        public object[] Arguments { get; set; }
    }
}

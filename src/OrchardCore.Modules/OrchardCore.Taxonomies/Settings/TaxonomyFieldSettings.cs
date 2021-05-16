namespace OrchardCore.Taxonomies.Settings
{
    public class TaxonomyFieldSettings
    {
        public string Hint { get; set; }

        /// <summary>
        /// Whether a selection is required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// The content item id of the taxonomy to choose from
        /// </summary>
        public string TaxonomyContentItemId { get; set; }

        /// <summary>
        /// Whether the user can select only one term or not
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Whether the user can only select leaves in the taxonomy
        /// </summary>
        public bool LeavesOnly { get; set; }

        /// <summary>
        /// Whether the field allows the user to add new Terms to the taxonomy (similar to tags)
        /// </summary>
        public bool Open { get; set; }
    }
}

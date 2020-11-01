namespace OrchardCore.Markdown
{
    public class MarkdownOptions
    {
        /// <summary>
        /// Gets or sets a list of the features to be used in <see cref="Markdig.MarkdownPipelineBuilder"/>.
        /// </summary>
        /// <remarks>
        /// "nohtml" will call DisableHtml()
        /// "advanced" will call UseAdvancedExtensions()
        /// </remarks>
        public string Features { get; set; } = "nohtml+advanced";
    }
}

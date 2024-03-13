namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public interface IContentIndexSettings
    {
        public bool Included { get; set; }

        public DocumentIndexOptions ToOptions();
    }
}

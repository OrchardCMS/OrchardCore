namespace OrchardVNext.ContentManagement {
    public interface IContent {
        ContentItem ContentItem { get; }

        /// <summary>
        /// The ContentItem's identifier.
        /// </summary>
        int Id { get; }
    }
}
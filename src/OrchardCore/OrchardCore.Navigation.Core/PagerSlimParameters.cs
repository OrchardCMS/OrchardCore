namespace OrchardCore.Navigation
{
    /// <summary>
    /// Represents the paging parameters of a safe navigation that doesn't
    /// require counting, and doesn't support page size alteration.
    /// </summary>
    public class PagerSlimParameters
    {
        /// <summary>
        /// Gets or sets the first item displayed on the page.
        /// </summary>
        public string Before { get; set; }

        /// <summary>
        /// Gets or sets the last item displayed on the page.
        /// </summary>
        public string After { get; set; }
    }
}

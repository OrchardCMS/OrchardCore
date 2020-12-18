namespace OrchardCore.Navigation
{
    public class PagerSlim
    {
        /// <summary>
        /// Constructs a new pager.
        /// </summary>
        /// <param name="pagerParameters">The pager parameters.</param>
        /// <param name="pageSize">The page size parameter.</param>
        public PagerSlim(PagerSlimParameters pagerParameters, int pageSize)
            : this(pagerParameters.Before, pagerParameters.After, pageSize)
        {
        }

        /// <summary>
        /// Constructs a new pager.
        /// </summary>
        /// <param name="before">The identifier of the first element in the page.</param>
        /// <param name="after">The identifier of the last element in the page.</param>
        /// <param name="pageSize">The page size parameter.</param>
        public PagerSlim(string before, string after, int pageSize)
        {
            Before = before;
            After = after;
            PageSize = pageSize;
        }

        /// <summary>
        /// Gets or sets the current page size or the site default size if none is specified.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the the first element in the page or <code>null</code>  if none is specified.
        /// </summary>
        public string Before { get; set; }

        /// <summary>
        /// Gets or sets the the last element in the page or <code>null</code>  if none is specified.
        /// </summary>
        public string After { get; set; }
    }
}

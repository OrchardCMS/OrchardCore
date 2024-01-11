namespace OrchardCore.Navigation
{
    public class Pager
    {
        /// <summary>
        /// The default page number.
        /// </summary>
        public const int PageDefault = 1;

        /// <summary>
        /// Constructs a new pager.
        /// </summary>
        /// <param name="pagerParameters">The pager parameters.</param>
        /// <param name="defaultPageSize">The default page size.</param>
        public Pager(PagerParameters pagerParameters, int defaultPageSize)
            : this(pagerParameters.Page, pagerParameters.PageSize, defaultPageSize)
        {
        }

        /// <summary>
        /// Constructs a new pager.
        /// </summary>
        /// <param name="page">The page parameter.</param>
        /// <param name="pageSize">The page size parameter.</param>
        /// <param name="defaultPageSize">The default page size.</param>
        public Pager(int? page, int? pageSize, int defaultPageSize)
        {
            Page = (int)(page != null ? (page > 0 ? page : PageDefault) : PageDefault);
            PageSize = pageSize ?? defaultPageSize;
        }

        /// <summary>
        /// Gets or sets the current page number or the default page number if none is specified.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the current page size or the site default size if none is specified.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the current page start index.
        /// </summary>
        /// <param name="page">The current page number.</param>
        /// <returns>The index in which the page starts.</returns>
        public int GetStartIndex(int? page = null)
        {
            return ((page ?? Page) - PageDefault) * PageSize;
        }
    }
}

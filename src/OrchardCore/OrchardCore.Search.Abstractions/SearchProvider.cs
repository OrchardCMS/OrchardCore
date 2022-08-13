namespace OrchardCore.Search.Abstractions
{
    /// <summary>
    /// Provides a way to determine registered SearchProviders for a tenant
    /// </summary>
    public class SearchProvider
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="SearchProvider"/>.
        /// </summary>
        protected SearchProvider(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the technical name of the SearchProvider.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Area name of the SearchProvider.
        /// </summary>
        public string AreaName { get; protected set; }
    }
}

namespace OrchardCore.Search.Abstractions
{
    /// <summary>
    /// Provides a way to determine registered SearchProviders for a tenant
    /// </summary>
    public abstract class SearchProvider
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="SearchProvider"/>.
        /// </summary>
        /// <param name="name"></param>
        protected SearchProvider(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the technical name of the SearchProvider.
        /// </summary>
        public string Name { get; }
    }
}

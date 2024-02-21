namespace OrchardCore.Queries
{
    /// <summary>
    /// Represents a query.
    /// </summary>
    public class Query
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="Query"/>.
        /// </summary>
        /// <param name="source"></param>
        protected Query(string source)
        {
            Source = source;
        }

        /// <summary>
        /// Gets or sets the technical name of the query.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the source for this query.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Gets or sets the return schema of the query.
        /// This is used runtime determination of the results returned when Content Items are not returned.
        /// </summary>
        public string Schema { get; set; }

        public virtual bool ResultsOfType<T>() => typeof(T) == typeof(object);
    }
}

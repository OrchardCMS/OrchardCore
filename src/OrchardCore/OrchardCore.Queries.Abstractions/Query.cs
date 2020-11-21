namespace OrchardCore.Queries
{
    public class Query
    {
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
    }
}

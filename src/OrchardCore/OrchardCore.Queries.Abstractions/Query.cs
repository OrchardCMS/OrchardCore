using Newtonsoft.Json;

namespace OrchardCore.Queries
{
    public class Query
    {
        protected Query(string source)
        {
            Source = source;
        }

        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

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

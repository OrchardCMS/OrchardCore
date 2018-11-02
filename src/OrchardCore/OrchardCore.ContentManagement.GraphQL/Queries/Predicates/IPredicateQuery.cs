using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    public interface IPredicateQuery
    {
        ISqlDialect Dialect { get; set; } 

        /// <summary>
        /// Adds a new query parameter to the current query.
        /// </summary>
        /// <param name="value">The value of the query parameter.</param>
        /// <returns>The name of the new query parameter.</returns>
        string NewQueryParameter(object value);
    }
}
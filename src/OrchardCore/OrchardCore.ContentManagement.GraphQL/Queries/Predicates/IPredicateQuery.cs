using System.Collections.Generic;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// Represents a predicate query instance that can build SQL to execute against a database.
    /// </summary>
    public interface IPredicateQuery
    {
        ISqlDialect Dialect { get; set; }

        IDictionary<string, object> Parameters { get; }

        /// <summary>
        /// Adds a new query parameter to the current query.
        /// </summary>
        /// <param name="value">The value of the query parameter.</param>
        /// <returns>The name of the new query parameter.</returns>
        string NewQueryParameter(object value);

        /// <summary>
        /// Creates an alias for a specified path.
        /// </summary>
        /// <param name="path">The path of the property.</param>
        /// <param name="alias">The alias name.</param>
        void CreateAlias(string path, string alias);

        /// <summary>
        /// Creates an actual sql table alias for a specified path.
        /// </summary>
        /// <param name="path">The path of the property.</param>
        /// <param name="tableAlias">The sql table alias name.</param>
        void CreateTableAlias(string path, string tableAlias);

        /// <summary>
        /// search used alias for the given property path.
        /// </summary>
        /// <param name="propertyPath">The path of the property.</param>
        void SearchUsedAlias(string propertyPath);

        /// <summary>
        /// Gets the formatted column name for the given property path.
        /// </summary>
        /// <param name="propertyPath">The path of the property.</param>
        /// <returns>The SQL formatted name of the column.</returns>
        string GetColumnName(string propertyPath);

        /// <summary>
        /// Gets all the aliases that were used in the current query.
        /// </summary>
        /// <returns>The collection of alias names.</returns>
        IEnumerable<string> GetUsedAliases();
    }
}

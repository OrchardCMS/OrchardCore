namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    public interface IPredicate
    {
        /// <summary>
        /// Render a sql string for the expression.
        /// </summary>
        /// <returns>A string that contains a valid Sql fragment.</returns>
        string ToSqlString(IPredicateQuery predicateQuery);

        void SearchUsedAlias(IPredicateQuery predicateQuery);
    }
}

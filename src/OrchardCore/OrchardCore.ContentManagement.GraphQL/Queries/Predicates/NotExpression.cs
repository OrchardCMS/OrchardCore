namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate" /> that negates another <see cref="IPredicate" />.
    /// </summary>
    public class NotExpression : IPredicate
    {
        private readonly IPredicate _predicate;

        /// <summary>
        /// Initialize a new instance of the <see cref="NotExpression" /> class for an <see cref="IPredicate" />
        /// </summary>
        /// <param name="predicate">The <see cref="IPredicate" /> to negate.</param>
        public NotExpression(IPredicate predicate)
        {
            _predicate = predicate;
        }

        public void SearchUsedAlias(IPredicateQuery predicateQuery)
        {
            _predicate.SearchUsedAlias(predicateQuery);
        }

        public string ToSqlString(IPredicateQuery predicateQuery)
        {
            var expression = _predicate.ToSqlString(predicateQuery);
            return $"not ({expression})";
        }
    }
}

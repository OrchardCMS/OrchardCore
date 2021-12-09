namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate" /> that combines two <see cref="IPredicate" />s
    /// with a operator (either "<c>and</c>" or "<c>or</c>") between them.
    /// </summary>
    public abstract class LogicalExpression : IPredicate
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="LogicalExpression" /> class that
        /// combines two other <see cref="IPredicate" />s.
        /// </summary>
        /// <param name="left">The left hand side <see cref="IPredicate" />.</param>
        /// <param name="right">The right hand side <see cref="IPredicate" />.</param>
        protected LogicalExpression(IPredicate left, IPredicate right)
        {
            Left = left;
            Right = right;
        }

        /// <summary>
        /// Gets the <see cref="IPredicate" /> that will be on the Left Hand Side of the Op.
        /// </summary>
        protected IPredicate Left { get; }

        /// <summary>
        /// Gets the <see cref="IPredicate" /> that will be on the Right Hand Side of the Op.
        /// </summary>
        protected IPredicate Right { get; }

        /// <summary>
        /// Get the Sql operator to put between the two <see cref="Expression" />s.
        /// </summary>
        protected abstract string Operator { get; }

        public void SearchUsedAlias(IPredicateQuery predicateQuery)
        {
            Left.SearchUsedAlias(predicateQuery);
            Right.SearchUsedAlias(predicateQuery);
        }

        public string ToSqlString(IPredicateQuery predicateQuery)
        {
            var left = Left.ToSqlString(predicateQuery);
            var right = Right.ToSqlString(predicateQuery);

            return $"({left} {Operator} {right})";
        }
    }
}

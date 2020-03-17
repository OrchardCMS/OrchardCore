namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate" /> that combines two <see cref="IPredicate" />s with an
    /// <c>"or"</c> between them.
    /// </summary>
    public class OrExpression : LogicalExpression
    {
        /// <summary>
        /// Initialize a new instance of the <see cref="OrExpression" /> class for
        /// two <see cref="IPredicate" />s.
        /// </summary>
        /// <param name="left">The left hand side <see cref="IPredicate" />.</param>
        /// <param name="right">The right hand side <see cref="IPredicate" />.</param>
        public OrExpression(IPredicate left, IPredicate right) : base(left, right)
        {
        }

        /// <inheritdoc />
        /// <value>Returns "<c>or</c>"</value>
        protected override string Operator => "or";
    }
}

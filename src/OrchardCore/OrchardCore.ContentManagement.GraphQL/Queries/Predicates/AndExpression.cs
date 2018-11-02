namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// An <see cref="LogicalExpression"/> that combines two <see cref="IPredicate"/>s
    /// with an <c>and</c> between them.
    /// </summary>
    public class AndExpression : LogicalExpression
    {
        /// <inheritdoc />
        protected override string Operator => "and";

        /// <summary>
        /// Initializes a new instance of the <see cref="AndExpression"/> class
        /// that combines two <see cref="IPredicate"/>.
        /// </summary>
        /// <param name="left">The left hand side <see cref="IPredicate"/>.</param>
        /// <param name="right">The right hand side <see cref="IPredicate"/>.</param>
        public AndExpression(IPredicate left, IPredicate right) : base(left, right)
        {
        }
    }
}
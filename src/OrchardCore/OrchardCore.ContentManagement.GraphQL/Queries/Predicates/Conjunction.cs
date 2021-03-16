namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate" /> that combines together multiple <see cref="IPredicate" />s with an <c>and</c>
    /// </summary>
    public class Conjunction : Junction
    {
        /// <inheritdoc />
        protected override string Operation => " and ";

        /// <inheritdoc />
        protected override string EmptyExpression => "1=1";
    }
}

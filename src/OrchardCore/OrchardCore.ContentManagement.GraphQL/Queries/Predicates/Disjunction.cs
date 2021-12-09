namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// An <see cref="IPredicate" /> that combines together multiple <see cref="IPredicate" />s with an <c>or</c>
    /// </summary>
    public class Disjunction : Junction
    {
        /// <inheritdoc />
        protected override string Operation => " or ";

        /// <inheritdoc />
        protected override string EmptyExpression => "1=0";
    }
}

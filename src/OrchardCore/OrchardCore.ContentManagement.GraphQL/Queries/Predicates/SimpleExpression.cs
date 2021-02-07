namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// The base class for an <see cref="IPredicate" /> that compares a single property
    /// to a value.
    /// </summary>
    public class SimpleExpression : IPredicate
    {
        private readonly string _propertyName;
        private readonly object _value;

        /// <summary>
        /// Initialize a new instance of the <see cref="SimpleExpression" /> class for a named
        /// property and its value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        /// <param name="operation">The SQL operation.</param>
        public SimpleExpression(string propertyName, object value, string operation)
        {
            _propertyName = propertyName;
            _value = value;
            Operation = operation;
        }

        /// <summary>
        /// Get the Sql operator to use for the specific
        /// subclass of <see cref="SimpleExpression" />.
        /// </summary>
        protected virtual string Operation { get; }

        public void SearchUsedAlias(IPredicateQuery predicateQuery)
        {
            predicateQuery.SearchUsedAlias(_propertyName);
        }

        /// <summary>
        /// Converts the SimpleExpression to a SQL <see cref="string" />.
        /// </summary>
        /// <returns>A string that contains a valid Sql fragment.</returns>
        public string ToSqlString(IPredicateQuery predicateQuery)
        {
            var columnName = predicateQuery.GetColumnName(_propertyName);
            var parameter = predicateQuery.NewQueryParameter(_value);

            return $"({columnName}{Operation}{parameter})";
        }
    }
}

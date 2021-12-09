namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// Represents the built-in <see cref="IPredicate" /> expressions used for building SQL expressions.
    /// </summary>
    /// <seealso cref="IPredicate" />
    public class Expression
    {
        /// <summary>Constructs a new instance of <see cref="Expression"></see>.</summary>
        protected Expression()
        {
        }

        /// <summary>
        /// Apply an "equal" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        public static SimpleExpression Equal(string propertyName, object value)
        {
            return new SimpleExpression(propertyName, value, " = ");
        }

        /// <summary>
        /// Apply a "like" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        /// <param name="matchOptions">The match options for the like expression.</param>
        /// <returns>A <see cref="LikeExpression" />.</returns>
        public static IPredicate Like(string propertyName, string value, MatchOptions matchOptions)
        {
            return new LikeExpression(propertyName, value, matchOptions);
        }

        /// <summary>
        /// Apply a "greater than" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        public static SimpleExpression GreaterThan(string propertyName, object value)
        {
            return new SimpleExpression(propertyName, value, " > ");
        }

        /// <summary>
        /// Apply a "greater than or equal" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        public static SimpleExpression GreaterThanOrEqual(string propertyName, object value)
        {
            return new SimpleExpression(propertyName, value, " >= ");
        }

        /// <summary>
        /// Apply a "less than" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        public static SimpleExpression LessThan(string propertyName, object value)
        {
            return new SimpleExpression(propertyName, value, " < ");
        }

        /// <summary>
        /// Apply a "less than or equal" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value for the property.</param>
        public static SimpleExpression LessThanOrEqual(string propertyName, object value)
        {
            return new SimpleExpression(propertyName, value, " <= ");
        }

        /// <summary>
        /// Apply an "in" constraint to the named property
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="values">An array of values.</param>
        /// <returns>An <see cref="InExpression" />.</returns>
        public static IPredicate In(string propertyName, object[] values)
        {
            return new InExpression(propertyName, values);
        }

        /// <summary>
        /// Return the conjunction of two expressions
        /// </summary>
        /// <param name="left">The left hand side expression.</param>
        /// <param name="right">The right hand side expression.</param>
        /// <returns>An <see cref="AndExpression" />.</returns>
        public static IPredicate And(IPredicate left, IPredicate right)
        {
            return new AndExpression(left, right);
        }

        /// <summary>
        /// Return the disjunction of two expressions
        /// </summary>
        /// <param name="left">The left hand side expression.</param>
        /// <param name="right">The right hand side expression.</param>
        /// <returns>An <see cref="OrExpression" />.</returns>
        public static IPredicate Or(IPredicate left, IPredicate right)
        {
            return new OrExpression(left, right);
        }

        /// <summary>
        /// Return the negation of an expression
        /// </summary>
        /// <param name="expression">The expression to negate.</param>
        /// <returns>A <see cref="NotExpression" />.</returns>
        public static IPredicate Not(IPredicate expression)
        {
            return new NotExpression(expression);
        }

        /// <summary>
        /// Group expressions together in a single conjunction (A and B and C...)
        /// </summary>
        public static Conjunction Conjunction()
        {
            return new Conjunction();
        }

        /// <summary>
        /// Group expressions together in a single disjunction (A or B or C...)
        /// </summary>
        public static Disjunction Disjunction()
        {
            return new Disjunction();
        }
    }
}

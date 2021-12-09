using System.Collections.Generic;
using System.Text;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    /// <summary>
    /// A sequence of logical <see cref="IPredicate" />s combined by some associative
    /// logical operator.
    /// </summary>
    public abstract class Junction : IPredicate
    {
        private readonly IList<IPredicate> _predicates = new List<IPredicate>();

        /// <summary>
        /// Get the Sql operator to put between multiple <see cref="IPredicate" />s.
        /// </summary>
        protected abstract string Operation { get; }

        /// <summary>
        /// The <see cref="string" /> corresponding to an instance with no added sub-criteria.
        /// </summary>
        protected abstract string EmptyExpression { get; }

        public void SearchUsedAlias(IPredicateQuery predicateQuery)
        {
            if (_predicates.Count == 0) return;


            for (var i = 0; i < _predicates.Count; i++)
            {
                _predicates[i].SearchUsedAlias(predicateQuery);
            }

        }

        public string ToSqlString(IPredicateQuery predicateQuery)
        {
            if (_predicates.Count == 0) return EmptyExpression;

            var sqlBuilder = new StringBuilder();

            sqlBuilder.Append('(');

            for (var i = 0; i < _predicates.Count - 1; i++)
            {
                sqlBuilder.Append(_predicates[i].ToSqlString(predicateQuery));
                sqlBuilder.Append(Operation);
            }

            sqlBuilder.Append(_predicates[_predicates.Count - 1].ToSqlString(predicateQuery));

            sqlBuilder.Append(')');

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// Adds an <see cref="IPredicate" /> to the list of <see cref="IPredicate" />s
        /// to junction together.
        /// </summary>
        /// <param name="predicate">The <see cref="IPredicate" /> to add.</param>
        /// <returns>
        /// This <see cref="Junction" /> instance.
        /// </returns>
        public Junction Add(IPredicate predicate)
        {
            _predicates.Add(predicate);
            return this;
        }
    }
}

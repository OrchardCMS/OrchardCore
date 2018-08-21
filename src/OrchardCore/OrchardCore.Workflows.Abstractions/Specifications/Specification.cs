using System;
using System.Linq.Expressions;

namespace OrchardCore.Workflows.Specifications
{
    public abstract class Specification<T>
    {
        public virtual Expression<Func<T, bool>> PredicateExpression { get; }
        public virtual Expression<Func<T, object>> OrderByExpression { get; }
        public virtual Expression<Func<T, object>> OrderByDescendingExpression { get; }
    }
}

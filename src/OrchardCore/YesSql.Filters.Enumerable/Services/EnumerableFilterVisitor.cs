using YesSql.Filters.Abstractions.Nodes;
using YesSql.Filters.Abstractions.Services;

namespace YesSql.Filters.Enumerable.Services;

public class EnumerableFilterVisitor<T>
    : IFilterVisitor<EnumerableExecutionContext<T>, Func<IEnumerable<T>, ValueTask<IEnumerable<T>>>>
    where T : class
{
    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        TermOperationNode node,
        EnumerableExecutionContext<T> argument
    ) => node.Operation.Accept(this, argument);

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        AndTermNode node,
        EnumerableExecutionContext<T> argument
    )
    {
        var predicates = new List<Func<IEnumerable<T>, ValueTask<IEnumerable<T>>>>();
        foreach (var child in node.Children)
        {
            Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> predicate = (q) =>
                child.Operation.Accept(this, argument)(q);
            predicates.Add(predicate);
        }

        var result =
            (Func<IEnumerable<T>, ValueTask<IEnumerable<T>>>)Delegate.Combine(predicates.ToArray());

        return result;
    }

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        UnaryNode node,
        EnumerableExecutionContext<T> argument
    )
    {
        var currentQuery = argument.CurrentTermOption.MatchPredicate;
        if (!node.UseMatch)
        {
            currentQuery = argument.CurrentTermOption.NotMatchPredicate;
        }

        return result => currentQuery(node.Value, argument.Item, argument);
    }

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        NotUnaryNode node,
        EnumerableExecutionContext<T> argument
    )
    {
        Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> except = async (q) =>
        {
            var not = await node.Operation.Accept(this, argument)(q);

            return not;
        };

        return except;
    }

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        OrNode node,
        EnumerableExecutionContext<T> argument
    )
    {
        Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> union = async (q) =>
        {
            var l = await node.Left.Accept(this, argument)(q);
            var r = await node.Right.Accept(this, argument)(q);

            return l.Union(r);
        };

        return union;
    }

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        AndNode node,
        EnumerableExecutionContext<T> argument
    )
    {
        Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> intersect = async (q) =>
        {
            var l = await node.Left.Accept(this, argument)(q);
            var r = await node.Right.Accept(this, argument)(q);

            return l.Intersect(r);
        };

        return intersect;
    }

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        GroupNode node,
        EnumerableExecutionContext<T> argument
    ) => node.Operation.Accept(this, argument);

    public Func<IEnumerable<T>, ValueTask<IEnumerable<T>>> Visit(
        TermNode node,
        EnumerableExecutionContext<T> argument
    ) => node.Accept(this, argument);
}

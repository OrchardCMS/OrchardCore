using GraphQL;
using GraphQL.Resolvers;

namespace OrchardCore.Apis.GraphQL.Resolvers;

public class LockedAsyncFieldResolver<TReturnType> : IFieldResolver
{
    private readonly Func<IResolveFieldContext, ValueTask<TReturnType>> _resolver;

    public LockedAsyncFieldResolver(Func<IResolveFieldContext, ValueTask<TReturnType>> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _resolver = resolver;
    }

    public async ValueTask<object> ResolveAsync(IResolveFieldContext context)
    {
        var graphContext = (GraphQLUserContext)context.UserContext;
        await graphContext.ExecutionContextLock.WaitAsync();

        try
        {
            return await _resolver(context).ConfigureAwait(false);
        }
        finally
        {
            graphContext.ExecutionContextLock.Release();
        }
    }
}

public class LockedAsyncFieldResolver<TSourceType, TReturnType> : IFieldResolver
{
    private readonly Func<IResolveFieldContext<TSourceType>, ValueTask<TReturnType>> _resolver;

    public LockedAsyncFieldResolver(Func<IResolveFieldContext<TSourceType>, ValueTask<TReturnType>> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _resolver = resolver;
    }

    public async ValueTask<object> ResolveAsync(IResolveFieldContext context)
    {
        var graphContext = (GraphQLUserContext)context.UserContext;
        await graphContext.ExecutionContextLock.WaitAsync();

        try
        {
            return await _resolver(context.As<TSourceType>()).ConfigureAwait(false);
        }
        finally
        {
            graphContext.ExecutionContextLock.Release();
        }
    }
}

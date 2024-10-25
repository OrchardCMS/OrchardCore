using GraphQL;
using GraphQL.Resolvers;

namespace OrchardCore.Apis.GraphQL.Resolvers;

public class LockedAsyncFieldResolver<TReturnType> : FuncFieldResolver<TReturnType>
{
    public LockedAsyncFieldResolver(Func<IResolveFieldContext, ValueTask<TReturnType>> resolver) : base(resolver)
    {

    }

    public new async ValueTask<object> ResolveAsync(IResolveFieldContext context)
    {
        var graphContext = (GraphQLUserContext)context.UserContext;
        await graphContext.ExecutionContextLock.WaitAsync();

        try
        {
            return await base.ResolveAsync(context);
        }
        finally
        {
            graphContext.ExecutionContextLock.Release();
        }
    }
}

public class LockedAsyncFieldResolver<TSourceType, TReturnType> : FuncFieldResolver<TSourceType, TReturnType>
{
    public LockedAsyncFieldResolver(Func<IResolveFieldContext<TSourceType>, ValueTask<TReturnType>> resolver) : base(resolver)
    {
    }

    public new async Task<object> ResolveAsync(IResolveFieldContext context)
    {
        var graphContext = (GraphQLUserContext)context.UserContext;
        await graphContext.ExecutionContextLock.WaitAsync();

        try
        {
            return await base.ResolveAsync(context);
        }
        finally
        {
            graphContext.ExecutionContextLock.Release();
        }
    }
}

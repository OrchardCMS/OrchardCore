using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;

namespace OrchardCore.Apis.GraphQL.Resolvers
{
    public class LockedAsyncFieldResolver : IFieldResolver
    {
        private readonly Func<IResolveFieldContext, Task<object>> _resolver;

        public LockedAsyncFieldResolver(Func<IResolveFieldContext, Task<object>> resolver)
        {
            _resolver = resolver;
        }

        public async ValueTask<object> ResolveAsync(IResolveFieldContext context)
        {
            var graphContext = (GraphQLUserContext)context.UserContext;

            await graphContext.ExecutionContextLock.WaitAsync();

            try
            {
                return _resolver(context);
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
}

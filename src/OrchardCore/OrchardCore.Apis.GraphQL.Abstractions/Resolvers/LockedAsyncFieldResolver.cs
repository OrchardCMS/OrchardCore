using System;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Resolvers
{
    public class LockedAsyncFieldResolver<TReturnType> : IFieldResolver<Task<TReturnType>>
    {
        private readonly Func<ResolveFieldContext, Task<TReturnType>> _resolver;

        public LockedAsyncFieldResolver(Func<ResolveFieldContext, Task<TReturnType>> resolver)
        {
            _resolver = resolver;
        }

        public async Task<TReturnType> Resolve(ResolveFieldContext context)
        {
            var graphContext = (GraphQLContext)context.UserContext;

            await graphContext.ExecutionContextLock.WaitAsync();

            try
            {
                return await _resolver(context);
            }
            finally
            {
                graphContext.ExecutionContextLock.Release();
            }
        }

        object IFieldResolver.Resolve(ResolveFieldContext context)
        {
            return Resolve(context);
        }
    }

    public class LockedAsyncFieldResolver<TSourceType, TReturnType> : AsyncFieldResolver<TSourceType, TReturnType>, IFieldResolver<Task<TReturnType>>
    {
        public LockedAsyncFieldResolver(Func<ResolveFieldContext<TSourceType>, Task<TReturnType>> resolver) : base(resolver)
        {
        }

        public new async Task<TReturnType> Resolve(ResolveFieldContext context)
        {
            var graphContext = (GraphQLContext)context.UserContext;
            await graphContext.ExecutionContextLock.WaitAsync();

            try
            {
                return await base.Resolve(context);
            }
            finally
            {
                graphContext.ExecutionContextLock.Release();
            }
        }

        object IFieldResolver.Resolve(ResolveFieldContext context)
        {
            return Resolve(context);
        }
    }
}

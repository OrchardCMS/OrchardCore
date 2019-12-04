using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Builders;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL
{
    public static class ResolveLockedExtensions
    {
        static SemaphoreSlim f = new SemaphoreSlim(1, 1);

        public static FieldBuilder<TSourceType, TReturnType> ResolveLockedAsync<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, Func<ResolveFieldContext<TSourceType>, Task<TReturnType>> resolve)
        {
            return builder.Resolve(new PermissionsAsyncFieldResolver<TSourceType, TReturnType>(resolve));
        }

        internal class PermissionsAsyncFieldResolver<TSourceType, TReturnType> : AsyncFieldResolver<TSourceType, TReturnType>, IFieldResolver<Task<TReturnType>>
        {
            public PermissionsAsyncFieldResolver(Func<ResolveFieldContext<TSourceType>, Task<TReturnType>> resolver) : base(resolver)
            {
            }

            public new async Task<TReturnType> Resolve(ResolveFieldContext context)
            {
                await f.WaitAsync();
                try { 
                    return await base.Resolve(context);
                }
                finally
                {
                    f.Release();
                }
            }

            object IFieldResolver.Resolve(ResolveFieldContext context)
            {

                return Resolve(context);

            }
        }
    }
}

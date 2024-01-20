using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Resolvers;

namespace OrchardCore.Apis.GraphQL
{
    public static class FieldBuilderResolverExtensions
    {
        public static FieldBuilder<TSourceType, TReturnType> ResolveLockedAsync<TSourceType, TReturnType>(this FieldBuilder<TSourceType, TReturnType> builder, Func<IResolveFieldContext<TSourceType>, ValueTask<TReturnType>> resolve)
        {
            return builder.Resolve(new LockedAsyncFieldResolver<TSourceType, TReturnType>(resolve));
        }

        public static FieldType AddField<TFieldGraphType, TSourceType, TOutType>(this ComplexGraphType<TSourceType> graphType, string name, Func<IResolveFieldContext<TSourceType>, TOutType> resolve, string description = null)
        {

            var fieldType = new FieldType()
            {
                Name = name,
                Type = typeof(IntGraphType),
                Resolver = new FuncFieldResolver<TSourceType, TOutType>(resolve)
            };

            if (description != null)
            {
                fieldType.Description = description;
            }

            graphType.AddField(fieldType);
            return fieldType;
        }
    }
}

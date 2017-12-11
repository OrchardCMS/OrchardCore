using System;
using System.Linq.Expressions;
using GraphQL.Builders;
using GraphQL.Resolvers;
using OrchardCore.Apis.GraphQL;

namespace GraphQL.Types
{
    public static class IInputObjectGraphTypeExtensions
    {
        public static FieldBuilder<TSourceType, TProperty> AddInputField<TSourceType, TProperty>(
            this InputObjectGraphType<TSourceType> graphType,
            string name,
            Expression<Func<TSourceType, TProperty>> expression,
            bool nullable = false,
            Type type = null)
        {
            if (type == null)
                type = typeof(TProperty).GetGraphTypeFromType(nullable);

            var builder = FieldBuilder.Create<TSourceType, TProperty>(type)
                .Resolve(new ExpressionFieldResolver<TSourceType, TProperty>(expression))
                .Type(type.BuildNamedType())
                .Name(name);

            graphType.AddField(builder.FieldType);

            return builder;
        }
    }
}

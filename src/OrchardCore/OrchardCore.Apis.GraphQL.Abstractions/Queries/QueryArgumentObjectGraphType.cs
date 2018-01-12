using System;
using System.Linq.Expressions;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public abstract class QueryArgumentObjectGraphType<TSourceType> :
        InputObjectGraphType<TSourceType>,
        IQueryArgumentObjectGraphType
    {
        public FieldBuilder<TSourceType, TProperty> AddInputField<TProperty>(
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

            AddField(builder.FieldType);

            return builder;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Apis.GraphQL.Queries.Types
{
    public class InputContentPartAutoRegisteringObjectGraphType : InputObjectGraphType
    {
        public InputContentPartAutoRegisteringObjectGraphType(ContentPart contentPart)
        {
            var type = contentPart.GetType();

            Name = "Input_" + type.Name;
            
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string))
                            .Where(p => !p.PropertyType.IsEnum);

            foreach (var propertyInfo in properties)
            {
                var graphType = propertyInfo.PropertyType.GetGraphTypeFromType(true);// propertyInfo.PropertyType.IsNullable());

                var field = new FieldType
                {
                    Type = graphType,
                    Name = propertyInfo.Name,
                    ResolvedType = graphType.BuildNamedType(),
                    Resolver = new FuncFieldResolver<object, object>((context) =>
                    {
                        var values = context.Source.As<ContentElement>().AsDictionary();

                        return values.First(x => x.Key == context.FieldName).Value;
                    })
                };

                AddField(field);
            }
        }
    }

    public class FilterContentPartAutoRegisteringObjectGraphType : ObjectGraphType
    {
        public FilterContentPartAutoRegisteringObjectGraphType(ContentPart contentPart)
        {
            var type = contentPart.GetType();

            Name = type.Name;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string))
                            .Where(p => !p.PropertyType.IsEnum);

            foreach (var propertyInfo in properties)
            {
                var graphType = propertyInfo.PropertyType.GetGraphTypeFromType(true);// propertyInfo.PropertyType.IsNullable());

                var field = new FieldType
                {
                    Type = graphType,
                    Name = propertyInfo.Name,
                    ResolvedType = graphType.BuildNamedType(),
                    Resolver = new FuncFieldResolver<object, object>((context) =>
                    {
                        var values = context.Source.As<ContentElement>().AsDictionary();

                        return values.First(x => x.Key == context.FieldName).Value;
                    })
                };

                AddField(field);
            }

            IsTypeOf = obj => obj.GetType() == type;
        }
    }
}
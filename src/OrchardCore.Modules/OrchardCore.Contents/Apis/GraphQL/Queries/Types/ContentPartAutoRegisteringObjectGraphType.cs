using System;
using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Apis.GraphQL.Queries.Types
{
    public class ContentPartAutoRegisteringObjectGraphType : ObjectGraphType
    {
        public ContentPartAutoRegisteringObjectGraphType(ContentPart contentPart)
        {
            var type = contentPart.GetType();

            Name = type.Name;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.PropertyType.GetTypeInfo().IsValueType || p.PropertyType == typeof(string))
                            .Where(p => !p.PropertyType.IsEnum);

            foreach (var propertyInfo in properties)
            {
                var graphType = propertyInfo.PropertyType.GetGraphTypeFromType(true);// propertyInfo.PropertyType.IsNullable());

                Field(
                    graphType,
                    propertyInfo.Name,
                    resolve: new Func<ResolveFieldContext<object>, object>(context => {
                        var values = context.Source.As<ContentElement>().AsDictionary();

                        return values.First(x => x.Key == context.FieldName).Value;
                    }));
            }

            IsTypeOf = obj => obj.GetType() == type;
        }
    }
}
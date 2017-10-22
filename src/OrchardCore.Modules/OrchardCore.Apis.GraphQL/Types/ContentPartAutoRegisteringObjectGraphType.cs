using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.Apis.GraphQL.Types
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
                Field(
                    type: propertyInfo.PropertyType.GetGraphTypeFromType(propertyInfo.PropertyType.IsNullable()), 
                    name: propertyInfo.Name.ToGraphQLStringFormat(),
                    resolve: context => {
                        var values = context.Source.As<ContentElement>().AsDictionary();

                        return values.First(x => x.Key.ToGraphQLStringFormat() == context.FieldName);
                    });
            }
            
            IsTypeOf = obj => obj.GetType() == type;
        }
    }
}
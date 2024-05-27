using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartInputGraphType : WhereInputObjectGraphType<ContentPart>
{
    public DynamicPartInputGraphType(IHttpContextAccessor httpContextAccessor, ContentTypePartDefinition part)
    {
        Name = $"{part.Name}WhereInput";

        var serviceProvider = httpContextAccessor.HttpContext.RequestServices;
        var contentFieldProviders = serviceProvider.GetServices<IContentFieldProvider>().ToList();

        foreach (var field in part.PartDefinition.Fields)
        {
            foreach (var fieldProvider in contentFieldProviders)
            {
                var fieldType = fieldProvider.GetField(field, part.Name);

                if (fieldType == null)
                {
                    continue;
                }

                AddScalarFilterFields(fieldType.Type, fieldType.Name, fieldType.Description);
                break;
            }
        }
    }
}

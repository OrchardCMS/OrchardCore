using System.Linq;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartInputGraphType : InputObjectGraphType<ContentPart>
{
    public DynamicPartInputGraphType(
        IHttpContextAccessor httpContextAccessor,
        ContentTypePartDefinition part)
    {
        Name = part.Name;

        var serviceProvider = httpContextAccessor.HttpContext.RequestServices;
        var contentFieldProviders = serviceProvider.GetServices<IContentFieldProvider>().ToList();

        foreach (var field in part.PartDefinition.Fields)
        {
            foreach (var fieldProvider in contentFieldProviders)
            {
                var fieldType = fieldProvider.GetField(field);
                if (fieldType != null && typeof(ScalarGraphType).IsAssignableFrom(fieldType.Type))
                {
                    AddField(fieldType.WithPartNameMetaData(part.Name));
                    break;
                }
            }
        }
    }
}

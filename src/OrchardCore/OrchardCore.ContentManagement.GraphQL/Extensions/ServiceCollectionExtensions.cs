using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.GraphQL.Options;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class GraphQLContentOptionsExtensions
    {
        public static IServiceCollection SetPartAsCollapsed<TContentPart>(this IServiceCollection services) where TContentPart : ContentPart
        {
            return services.Configure<GraphQLContentOptions>(options => {
                options.PartOptions = options.PartOptions.Union(new[] { new GraphQLContentPartOption {
                    Name = typeof(TContentPart).Name,
                    Collapse = true
                } });
            });
        }
    }
}

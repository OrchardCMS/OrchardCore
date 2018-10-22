using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Mutations.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.GraphQL.Mutations
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class CreateContentItemMutationBuilder : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClock _clock;

        public CreateContentItemMutationBuilder(IHttpContextAccessor httpContextAccessor,
            IClock clock)
        {
            _httpContextAccessor = httpContextAccessor;
            _clock = clock;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();
            var contentTypeBuilders = serviceProvider.GetServices<IContentTypeBuilder>();
            var contentTypeMutationBuilders = serviceProvider.GetServices<IContentTypeMutationBuilder>();

            foreach (var typeDefinition in contentDefinitionManager.ListTypeDefinitions())
            {
                var typeType = new ContentItemType
                {
                    Name = typeDefinition.Name
                };

                var typeInputType = new CreateContentItemInputType
                {
                    Name = typeDefinition.Name
                };

                var mutation = new CreateContentItemMutation(_httpContextAccessor, _clock)
                {
                    Name = "Create" + typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType)
                };

                mutation.Arguments = new QueryArguments {
                    new QueryArgument(new ListGraphType(typeInputType))
                };

                foreach (var builder in contentTypeBuilders)
                {
                    builder.BuildAsync(mutation, typeDefinition, typeType);
                }

                foreach (var builder in contentTypeMutationBuilders)
                {
                    builder.BuildAsync(mutation, typeDefinition, typeInputType);
                }

                schema.Mutation.AddField(mutation);
            }

            return Task.FromResult(contentDefinitionManager.ChangeToken);
        }
    }
}

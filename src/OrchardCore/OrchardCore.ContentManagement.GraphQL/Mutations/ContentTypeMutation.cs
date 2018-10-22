using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.GraphQL.Mutations.Types;
using OrchardCore.Modules;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Mutations
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class ContentTypeMutation : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClock _clock;

        public ContentTypeMutation(IHttpContextAccessor httpContextAccessor,
            IClock clock)
        {
            _httpContextAccessor = httpContextAccessor;
            _clock = clock;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();
            var contentTypeBuilders = serviceProvider.GetServices<MutationFieldType>().ToList();

            foreach (var mutation in contentTypeBuilders) {
                schema.Mutation.AddField(mutation);
            }

            return Task.FromResult(contentDefinitionManager.ChangeToken);
        }
    }
}

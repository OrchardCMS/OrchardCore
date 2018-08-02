using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Mutations;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Subscriptions;

namespace OrchardCore.Apis.GraphQL.Services
{
    public class SchemaService : ISchemaService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IGraphQLSchemaHashService _hashService;
        private readonly IServiceProvider _serviceProvider;

        public SchemaService(
            IMemoryCache memoryCache,
            IGraphQLSchemaHashService hashService,
            IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _hashService = hashService;
            _serviceProvider = serviceProvider;
        }

        public async Task<ISchema> GetSchema()
        {
            var schemaHash = await _hashService.GetHash();

            return _memoryCache.GetOrCreate("GraphQL.Schema_" + schemaHash, f =>
            {
                f.SetSlidingExpiration(TimeSpan.FromHours(1));

                return new ContentSchema
                (
                    _serviceProvider.GetService<MutationsSchema>(),
                    _serviceProvider.GetService<QueriesSchema>(),
                    _serviceProvider.GetService<SubscriptionSchema>(),
                    _serviceProvider.GetServices<IInputObjectGraphType>(),
                    _serviceProvider.GetServices<IObjectGraphType>(),
                    _serviceProvider.GetService<IDependencyResolver>()
                );
            });
        }
    }
}

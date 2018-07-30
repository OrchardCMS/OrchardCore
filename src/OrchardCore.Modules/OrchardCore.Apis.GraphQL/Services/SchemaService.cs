using System;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

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
                return _serviceProvider.GetService<ISchema>();
            });
        }
    }
}

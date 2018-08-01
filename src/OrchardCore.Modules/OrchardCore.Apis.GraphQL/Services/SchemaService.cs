using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;

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

            return await _memoryCache.GetOrCreateAsync("GraphQL.Schema_" + schemaHash, async f =>
            {
                f.SetSlidingExpiration(TimeSpan.FromHours(1));

                var schema = new Schema();

                var query = new ObjectGraphType { Name = "Query" };

                // TODO: Remove QueryFieldType and create an interface that will populate the query fields directly
                // This service should also return a Token for when it's content is invalidated
                var queryFieldTypes = _serviceProvider.GetServices<QueryFieldType>();

                foreach (var field in queryFieldTypes)
                {
                    query.AddField(field);
                }

                var queryFieldTypeProviders = _serviceProvider.GetServices<IQueryFieldTypeProvider>();

                foreach (var p in queryFieldTypeProviders)
                {
                    foreach (var field in await p.GetFields(query))
                    {
                        query.AddField(field);
                    }
                }

                schema.Query = query;

                // TODO: Remove SubscriptionFieldType and create an interface that will populate the subscription fields directly
                var subscriptionFieldTypes = _serviceProvider.GetServices<SubscriptionFieldType>();

                if (subscriptionFieldTypes.Any())
                {
                    var subscription = new ObjectGraphType() { Name = "Subscription" };

                    foreach(var field in subscriptionFieldTypes)
                    {
                        subscription.AddField(field);
                    }

                    schema.Subscription = subscription;
                }

                // TODO: Remove MutationFieldType and create an interface that will populate the mutation fields directly
                var mutationFieldTypes = _serviceProvider.GetServices<MutationFieldType>();

                if (mutationFieldTypes.Any())
                {
                    var mutation = new ObjectGraphType() { Name = "Mutations" };

                    foreach (var field in mutationFieldTypes)
                    {
                        mutation.AddField(field);
                    }

                    schema.Mutation = mutation;
                }

                foreach (var type in _serviceProvider.GetServices<IInputObjectGraphType>())
                {
                    schema.RegisterType(type);
                }

                foreach (var type in _serviceProvider.GetServices<IObjectGraphType>())
                {
                    schema.RegisterType(type);
                }

                schema.DependencyResolver = _serviceProvider.GetService<IDependencyResolver>();

                return schema;
            });
        }
    }
}

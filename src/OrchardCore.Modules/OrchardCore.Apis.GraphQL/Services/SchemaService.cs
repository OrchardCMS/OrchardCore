using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Services
{
    public class SchemaService : ISchemaFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IEnumerable<ISchemaBuilder> _schemaBuilders;
        private readonly IServiceProvider _serviceProvider;

        public SchemaService(
            IMemoryCache memoryCache,
            IEnumerable<ISchemaBuilder> schemaBuilders,
            IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _schemaBuilders = schemaBuilders;
            _serviceProvider = serviceProvider;
        }

        public async Task<ISchema> GetSchema()
        {
            return await _memoryCache.GetOrCreateAsync("GraphQLSchema", async f =>
            {
                f.SetSlidingExpiration(TimeSpan.FromHours(1));

                var schema = new Schema();

                schema.Query = new ObjectGraphType { Name = "Query" };
                schema.Mutation = new ObjectGraphType { Name = "Mutation" };
                schema.Subscription = new ObjectGraphType { Name = "Subscription" };

                schema.DependencyResolver = _serviceProvider.GetService<IDependencyResolver>();

                foreach (var builder in _schemaBuilders)
                {
                    var token = await builder.BuildAsync(schema);

                    if (token != null)
                    {
                        f.AddExpirationToken(token);
                    }
                }

                // TODO: Remove MutationFieldType and convert types to ISchemaBuilder
                var mutationFields = _serviceProvider.GetServices<MutationFieldType>();

                foreach (var field in mutationFields)
                {
                    schema.Mutation.AddField(field);
                }

                foreach (var type in _serviceProvider.GetServices<IInputObjectGraphType>())
                {
                    schema.RegisterType(type);
                }

                foreach (var type in _serviceProvider.GetServices<IObjectGraphType>())
                {
                    schema.RegisterType(type);
                }
                
                // Clean Query, Mutation and Subscription if they have no fields
                // to prevent GraphQL configuration errors.

                if (!schema.Query.Fields.Any())
                {
                    schema.Query = null;
                }

                if (!schema.Mutation.Fields.Any())
                {
                    schema.Mutation = null;
                }

                if (!schema.Subscription.Fields.Any())
                {
                    schema.Subscription = null;
                }

                return schema;
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.Apis.GraphQL.Services
{
    public class SchemaService : ISchemaFactory
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IEnumerable<ISchemaBuilder> _schemaBuilders;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISiteService _siteService;
        private readonly IServiceProvider _serviceProvider;

        public SchemaService(
            IMemoryCache memoryCache,
            IEnumerable<ISchemaBuilder> schemaBuilders,
            IHttpContextAccessor httpContextAccessor,
            ISiteService siteService,
            IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _schemaBuilders = schemaBuilders;
            _httpContextAccessor = httpContextAccessor;
            _siteService = siteService;
            _serviceProvider = serviceProvider;
        }

        public async Task<ISchema> GetSchema()
        {
            var cultureFeature = _httpContextAccessor
                       .HttpContext
                       .Features
                       .Get<IRequestCultureFeature>();

            var culture = cultureFeature?.RequestCulture.Culture.ToString();

            if (culture == null)
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();

                culture = siteSettings.Culture;
            }

            return await _memoryCache.GetOrCreateAsync($"GraphQLSchema.{culture}", async f =>
            {
                f.SetSlidingExpiration(TimeSpan.FromHours(1));

                ISchema schema = new Schema
                {
                    Query = new ObjectGraphType { Name = "Query" },
                    Mutation = new ObjectGraphType { Name = "Mutation" },
                    Subscription = new ObjectGraphType { Name = "Subscription" },
                    FieldNameConverter = new OrchardFieldNameConverter(),

                    DependencyResolver = _serviceProvider.GetService<IDependencyResolver>()
                };

                foreach (var builder in _schemaBuilders)
                {
                    var token = await builder.BuildAsync(schema);

                    if (token != null)
                    {
                        f.AddExpirationToken(token);
                    }
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

                schema.Initialize();

                return schema;
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.Localization.GraphQL
{
    public class SiteCulturesQuery : ISchemaBuilder
    {
        public SiteCulturesQuery(IStringLocalizer<SiteCulturesQuery> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var field = new FieldType
            {
                Name = "SiteCultures",
                Description = T["The active cultures configured for the site."],
                Type = typeof(ListGraphType<CultureQueryObjectType>),
                Resolver = new AsyncFieldResolver<IEnumerable<SiteCulture>>(ResolveAsync)
            };

            schema.Query.AddField(field);

            return Task.FromResult<IChangeToken>(null);
        }

        private async Task<IEnumerable<SiteCulture>> ResolveAsync(ResolveFieldContext resolveContext)
        {
            var context = (GraphQLContext)resolveContext.UserContext;
            var localizationService = context.ServiceProvider.GetService<ILocalizationService>();

            var defaultCulture = await localizationService.GetDefaultCultureAsync();
            var supportedCultures = await localizationService.GetSupportedCulturesAsync();

             var cultures = supportedCultures.Select(culture =>
                new SiteCulture {
                    Culture = culture,
                    IsDefault = string.Equals(defaultCulture, culture, StringComparison.OrdinalIgnoreCase)
                }
            );

            return cultures;
        }
    }
}

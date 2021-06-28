using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using System.Linq;
using OrchardCore.Taxonomies.Fields;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Taxonomies.Services;

namespace OrchardCore.Taxonomies.Handlers
{
    public class TaxonomyfieldContentHandler : ContentHandlerBase
    {
        private IContentDefinitionManager _contentDefinitionManager;
        private readonly IServiceProvider _serviceProvider;
        private ITaxonomyService _taxonomyService;

        public TaxonomyfieldContentHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task PublishedAsync(PublishContentContext context)
        {
            _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();
            _taxonomyService = _taxonomyService ?? _serviceProvider.GetRequiredService<ITaxonomyService>();

            var fieldDefinitions = _contentDefinitionManager
                    .GetTypeDefinition(context.ContentItem.ContentType)
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)))
                    .ToArray();

            foreach (var fieldDefinition in fieldDefinitions)
            {
                var jPart = (JObject)context.ContentItem.Content[fieldDefinition.PartDefinition.Name];
                if (jPart == null)
                {
                    continue;
                }

                var jField = (JObject)jPart[fieldDefinition.Name];
                if (jField == null)
                {
                    continue;
                }

                var field = jField.ToObject<TaxonomyField>();

                await _taxonomyService.EnsureUniqueOrderValues(field);
            }
        }
    }
}

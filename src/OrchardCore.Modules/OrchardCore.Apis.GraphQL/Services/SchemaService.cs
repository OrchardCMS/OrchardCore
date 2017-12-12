using System;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.Apis.GraphQL.Services
{
    public class SchemaService : ISchemaService
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IServiceProvider _serviceProvider;
        private int _lastHash;
        private ISchema _schema;

        public SchemaService(
            IContentDefinitionManager contentDefinitionManager,
            IServiceProvider serviceProvider)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _serviceProvider = serviceProvider;
        }

        public async Task<ISchema> GetSchema()
        {
            var hash = await _contentDefinitionManager.GetTypesHashAsync();

            if (_lastHash == hash)
            {
                _schema = _serviceProvider.GetService<ISchema>();
                _lastHash = hash;
            }

            return _schema;
        }
    }
}

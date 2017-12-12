using System;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Apis.GraphQL.Services
{
    public class SchemaService : ISchemaService
    {
        private readonly IGraphQLSchemaHashService _hashService;
        private readonly IServiceProvider _serviceProvider;
        private int? _lastHash;
        private ISchema _schema;

        public SchemaService(
            IGraphQLSchemaHashService hashService,
            IServiceProvider serviceProvider)
        {
            _hashService = hashService;
            _serviceProvider = serviceProvider;
        }

        public async Task<ISchema> GetSchema()
        {
            var hash = await _hashService.GetHash();

            if (_lastHash != hash)
            {
                _schema = _serviceProvider.GetService<ISchema>();
                _lastHash = hash;
            }

            return _schema;
        }
    }
}

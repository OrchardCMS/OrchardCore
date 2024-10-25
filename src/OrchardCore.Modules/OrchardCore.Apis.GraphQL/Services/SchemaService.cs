using System.Collections.Concurrent;
using GraphQL.MicrosoftDI;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Apis.GraphQL.Services;

public class SchemaService : ISchemaFactory
{
    private readonly IEnumerable<ISchemaBuilder> _schemaBuilders;
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _schemaGenerationSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<ISchemaBuilder, string> _identifiers = new();

    private ISchema _schema;

    public SchemaService(IEnumerable<ISchemaBuilder> schemaBuilders, IServiceProvider serviceProvider)
    {
        _schemaBuilders = schemaBuilders;
        _serviceProvider = serviceProvider;
    }

    public async Task<ISchema> GetSchemaAsync()
    {
        var hasChanged = false;

        foreach (var builder in _schemaBuilders)
        {
            if (_identifiers.TryGetValue(builder, out var identifier) && await builder.GetIdentifierAsync() != identifier)
            {
                hasChanged = true;
                break;
            }
        }

        if (_schema is object && !hasChanged)
        {
            return _schema;
        }

        await _schemaGenerationSemaphore.WaitAsync();

        try
        {
            foreach (var builder in _schemaBuilders)
            {
                if (_identifiers.TryGetValue(builder, out var identifier) && await builder.GetIdentifierAsync() != identifier)
                {
                    hasChanged = true;
                    break;
                }
            }

            if (_schema is object && !hasChanged)
            {
                return _schema;
            }

            var serviceProvider = ShellScope.Services;

            var schema = new Schema(new SelfActivatingServiceProvider(_serviceProvider))
            {
                Query = new ObjectGraphType { Name = "Query" },
                Mutation = new ObjectGraphType { Name = "Mutation" },
                Subscription = new ObjectGraphType { Name = "Subscription" },
                NameConverter = new OrchardFieldNameConverter(),
            };

            foreach (var type in serviceProvider.GetServices<IInputObjectGraphType>())
            {
                schema.RegisterType(type);
            }

            foreach (var type in serviceProvider.GetServices<IObjectGraphType>())
            {
                schema.RegisterType(type);
            }

            foreach (var builder in _schemaBuilders)
            {
                var identifier = await builder.GetIdentifierAsync();

                // Null being a valid value not yet updated.
                if (identifier != string.Empty)
                {
                    _identifiers[builder] = identifier;
                }

                await builder.BuildAsync(schema);
            }


            // Clean Query, Mutation and Subscription if they have no fields
            // to prevent GraphQL configuration errors.

            if (schema.Query.Fields.Count == 0)
            {
                schema.Query = null;
            }

            if (schema.Mutation.Fields.Count == 0)
            {
                schema.Mutation = null;
            }

            if (schema.Subscription.Fields.Count == 0)
            {
                schema.Subscription = null;
            }

            schema.Initialize();
            return _schema = schema;
        }
        finally
        {
            _schemaGenerationSemaphore.Release();
        }
    }
}

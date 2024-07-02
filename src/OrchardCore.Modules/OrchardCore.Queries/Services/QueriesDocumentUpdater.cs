using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Queries.Services;

public sealed class QueriesDocumentUpdater : FeatureEventHandler
{
    private readonly IDocumentManager<QueriesDocument> _documentManager;
    private readonly IEnumerable<IQuerySource> _querySources;
    private readonly ITypeFeatureProvider _typeFeatureProvider;

    public QueriesDocumentUpdater(
        IDocumentManager<QueriesDocument> documentManager,
        IEnumerable<IQuerySource> querySources,
        ITypeFeatureProvider typeFeatureProvider)
    {
        _documentManager = documentManager;
        _querySources = querySources;
        _typeFeatureProvider = typeFeatureProvider;
    }

    public override async Task EnabledAsync(IFeatureInfo feature)
    {
        // If the newly activated feature contains a IQuerySource, update the QueriesDocument in case that source
        // type has been used before.
        if (_querySources.Any(source => _typeFeatureProvider.GetFeaturesForDependency(source.GetType()).Any(f => f.Id == feature.Id)))
        {
            await FixupDocumentAsync();
        }
    }

    private async Task FixupDocumentAsync()
    {
        var document = await _documentManager.GetOrCreateMutableAsync();

        if (!ValidateDocument(document) && FixupDocument(document))
        {
            await _documentManager.UpdateAsync(document);
        }
    }

    private bool FixupDocument(QueriesDocument document)
    {
        var hasChanged = false;

        // Check for any query that has no specific type information and try to fix it up with a derived type.
        // The type information is lost if a user edits the document while it contains a query for a feature that
        // has been disabled.
        foreach (var kv in document.Queries)
        {
            var query = kv.Value;
            if (query.GetType() == typeof(Query))
            {
                var sample = _querySources.FirstOrDefault(x => x.Name == query.Source)?.Create();

                if (sample != null)
                {
                    var json = JObject.FromObject(query);

                    document.Queries[kv.Key] = (Query)json.ToObject(sample.GetType());
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }

    private static bool ValidateDocument(QueriesDocument document) 
        => document.Queries.All(kv => kv.Value.GetType() != typeof(Query));
}

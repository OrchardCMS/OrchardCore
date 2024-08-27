using System.Text.Json.Serialization;
using OrchardCore.Data.Documents;
using OrchardCore.Json.Serialization;

namespace OrchardCore.Queries.Core.Models;

public sealed class QueriesDocument : Document
{
    [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<Query>))]
    public Dictionary<string, Query> Queries { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

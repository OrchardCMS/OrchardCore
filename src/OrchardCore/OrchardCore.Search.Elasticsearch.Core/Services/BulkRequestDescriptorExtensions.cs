using Elastic.Clients.Elasticsearch;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Services;

internal static class BulkRequestDescriptorExtensions
{
    public static BulkRequestDescriptor CreateElasticDocument(this BulkRequestDescriptor descriptor, IEnumerable<ContentItemDocumentIndex> documentIndex)
    {
        foreach (var document in documentIndex)
        {
            descriptor.Index(CreateElasticDocument(document), i => i.Id(document.ContentItemId));
        }

        return descriptor;
    }

    private static Dictionary<string, object> CreateElasticDocument(ContentItemDocumentIndex documentIndex)
    {
        var entries = new Dictionary<string, object>
        {
            { ContentIndexingConstants.ContentItemIdKey, documentIndex.ContentItemId },
            { ContentIndexingConstants.ContentItemVersionIdKey, documentIndex.ContentItemVersionId },
        };

        foreach (var entry in documentIndex.Entries)
        {
            switch (entry.Type)
            {
                case DocumentIndex.Types.Boolean:
                    if (entry.Value is bool boolValue)
                    {
                        AddValue(entries, entry.Name, boolValue);
                    }
                    break;

                case DocumentIndex.Types.DateTime:

                    if (entry.Value is DateTimeOffset offsetValue)
                    {
                        AddValue(entries, entry.Name, offsetValue);
                    }
                    else if (entry.Value is DateTime dateTimeValue)
                    {
                        AddValue(entries, entry.Name, dateTimeValue.ToUniversalTime());
                    }

                    break;

                case DocumentIndex.Types.Integer:
                    if (entry.Value != null && long.TryParse(entry.Value.ToString(), out var value))
                    {
                        AddValue(entries, entry.Name, value);
                    }

                    break;

                case DocumentIndex.Types.Number:
                    if (entry.Value != null)
                    {
                        AddValue(entries, entry.Name, Convert.ToDouble(entry.Value));
                    }
                    break;

                case DocumentIndex.Types.Text:
                    if (entry.Value != null)
                    {
                        var stringValue = Convert.ToString(entry.Value);

                        if (!string.IsNullOrEmpty(stringValue))
                        {
                            AddValue(entries, entry.Name, stringValue);
                        }
                    }
                    break;
                case DocumentIndex.Types.GeoPoint:
                    if (entry.Value is DocumentIndex.GeoPoint point)
                    {
                        AddValue(entries, entry.Name, GeoLocation.LatitudeLongitude(new LatLonGeoLocation
                        {
                            Lat = (double)point.Latitude,
                            Lon = (double)point.Longitude,
                        }));
                    }

                    break;
            }
        }

        return entries;
    }

    private static void AddValue(Dictionary<string, object> entries, string key, object value)
    {
        if (entries.TryAdd(key, value))
        {
            return;
        }

        // At this point, we know that a value already exists.
        if (entries[key] is List<object> list)
        {
            list.Add(value);

            entries[key] = list;

            return;
        }

        // Convert the existing value to a list of values.
        var values = new List<object>
        {
            entries[key],
            value,
        };

        entries[key] = values;
    }
}

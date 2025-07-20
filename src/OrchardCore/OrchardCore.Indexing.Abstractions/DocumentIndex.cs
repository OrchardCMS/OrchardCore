using Microsoft.AspNetCore.Html;

namespace OrchardCore.Indexing;

/// <summary>
/// Represents a document index that can be used to store various types of indexed data.
/// </summary>
public class DocumentIndex
{
    public string Id { get; private set; }

    public DocumentIndex(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        Id = id;
    }

    public List<DocumentIndexEntry> Entries { get; } = [];

    public void Set(string name, string value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Text, options));
    }

    public void Set(string name, IHtmlContent value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Text, options));
    }

    public void Set(string name, DateTimeOffset? value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.DateTime, options));
    }

    public void Set(string name, int? value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Integer, options));
    }

    public void Set(string name, bool? value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Boolean, options));
    }

    public void Set(string name, object value, DocumentIndexOptions options, Dictionary<string, object> metadata = null)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Complex, options)
        {
            Metadata = metadata,
        });
    }

    public void Set(string name, float[] value, int dimensions, DocumentIndexOptions options, Dictionary<string, object> metadata = null)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Vector, options)
        {
            Metadata = metadata,
            Dimensions = dimensions,
        });
    }

    public void Set(string name, double? value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Number, options));
    }

    public void Set(string name, decimal? value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.Number, options));
    }

    public void Set(string name, GeoPoint value, DocumentIndexOptions options)
    {
        Entries.Add(new DocumentIndexEntry(name, value, Types.GeoPoint, options));
    }

    public enum Types
    {
        Integer,
        Text,
        DateTime,
        Boolean,
        Number,
        GeoPoint,
        Complex,
        Vector,
    }

    public class GeoPoint
    {
        public decimal Longitude;
        public decimal Latitude;
    }

    public class DocumentIndexEntry(string name, object value, Types type, DocumentIndexOptions options)
    {
        public string Name { get; } = name;

        public object Value { get; } = value;

        public Types Type { get; } = type;

        public DocumentIndexOptions Options { get; } = options;

        public int Dimensions { get; set; }

        public Dictionary<string, object> Metadata { get; set; }
    }
}

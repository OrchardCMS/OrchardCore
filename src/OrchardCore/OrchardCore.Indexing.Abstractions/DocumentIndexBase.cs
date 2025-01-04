using Microsoft.AspNetCore.Html;

namespace OrchardCore.Indexing;

public class DocumentIndexBase
{
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
    }
}

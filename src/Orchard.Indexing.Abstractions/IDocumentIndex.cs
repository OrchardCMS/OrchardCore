using System;
using System.Collections.Generic;

namespace Orchard.Indexing
{
    public class DocumentIndex
    {
        public DocumentIndex(string contentItemId)
        {
            ContentItemId = contentItemId;
        }

        public Dictionary<string, DocumentIndexEntry> Entries { get; } = new Dictionary<string, DocumentIndexEntry>();

        public void Set(string name, string value, DocumentIndexOptions options)
        {
            Entries[name] = new DocumentIndexEntry(value, Types.Text, options);
        }

        public void Set(string name, DateTimeOffset value, DocumentIndexOptions options)
        {
            Entries[name] = new DocumentIndexEntry(value, Types.DateTime, options);
        }

        public void Set(string name, int value, DocumentIndexOptions options)
        {
            Entries[name] = new DocumentIndexEntry(value, Types.Integer, options);
        }

        public void Set(string name, bool value, DocumentIndexOptions options)
        {
            Entries[name] = new DocumentIndexEntry(value, Types.Boolean, options);
        }

        public void Set(string name, double value, DocumentIndexOptions options)
        {
            Entries[name] = new DocumentIndexEntry(value, Types.Number, options);
        }

        public string ContentItemId { get; }

        public enum Types
        {
            Integer,
            Text,
            DateTime,
            Boolean,
            Number
        }

        public class DocumentIndexEntry
        {
            public DocumentIndexEntry(object value, Types type, DocumentIndexOptions options)
            {
                Value = value;
                Type = type;
                Options = options;
            }

            public object Value { get; set; }
            public Types Type { get; set; }
            public DocumentIndexOptions Options { get; set; }
        }
    }
}

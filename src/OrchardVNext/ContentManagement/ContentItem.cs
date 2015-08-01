using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement.MetaData.Models;
using OrchardVNext.ContentManagement.Records;
#if DNXCORE50
using System.Reflection;
#endif

namespace OrchardVNext.ContentManagement {
    public class ContentItem : IContent {
        public ContentItem() {
            _parts = new List<ContentPart>();
        }

        private readonly IList<ContentPart> _parts;

        ContentItem IContent.ContentItem => this;

        public int Id { get { return Record?.Id ?? 0; } }
        public int Version { get { return VersionRecord?.Number ?? 0; } }

        public string ContentType { get; set; }
        public ContentTypeDefinition TypeDefinition { get; set; }
        public ContentItemRecord Record { get { return VersionRecord?.ContentItemRecord; } }
        public ContentItemVersionRecord VersionRecord { get; set; }
        
        public IEnumerable<ContentPart> Parts => _parts;

        public IContentManager ContentManager { get; set; }

        public bool Has(Type partType) {
            return partType == typeof(ContentItem) || _parts.Any(partType.IsInstanceOfType);
        }

        public IContent Get(Type partType) {
            if (partType == typeof(ContentItem))
                return this;
            return _parts.FirstOrDefault(partType.IsInstanceOfType);
        }

        public void Weld(ContentPart part) {
            part.ContentItem = this;
            _parts.Add(part);
        }
    }
}
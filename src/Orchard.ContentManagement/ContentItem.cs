using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.Records;
#if DNXCORE50
using System.Reflection;
#endif

namespace Orchard.ContentManagement {
    public class ContentItem : IContent {
        public ContentItem()
        {
        }

        ContentItem IContent.ContentItem => this;

        public int Id { get { return Record.Id; } }
        public int Version { get { return VersionRecord?.Number ?? 0; } }
        public string ContentType { get; set; }

        public ContentItemRecord Record { get; set; }
        public ContentItemVersionRecord VersionRecord { get; set; }
        
        public IEnumerable<ContentPart> Parts
        {
            get
            {
                if (Record != null)
                {
                    foreach (var record in Record.Parts)
                    {
                        yield return record;
                    }
                }

                if (VersionRecord != null)
                {
                    foreach (var record in VersionRecord.Parts)
                    {
                        yield return record;
                    }
                }
            }
        }

        public bool Has(Type partType) {
            return Parts.Any(partType.IsInstanceOfType);
        }

        public IContent Get(Type partType) {
            return Parts.FirstOrDefault(partType.IsInstanceOfType);
        }

        public void Weld(ContentPart part) {
            var contentVersionPart = part as ContentVersionPart;
            if (contentVersionPart != null)
            {
                if(VersionRecord == null)
                {
                    VersionRecord = new ContentItemVersionRecord();
                }

                VersionRecord.Parts.Add(contentVersionPart);
            }
            else
            {
                if (Record == null)
                {
                    Record = new ContentItemRecord();
                }

                Record.Parts.Add(part);
            }

            part.ContentItem = this;

        }
    }
}
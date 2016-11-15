using System;

namespace Orchard.ContentManagement.Handlers
{
    public class ContentItemAspectContext
    {
        public ContentItem ContentItem { get; set; }
        public object Aspect { get; set; }
        public Type Type { get; set; }
    }
}

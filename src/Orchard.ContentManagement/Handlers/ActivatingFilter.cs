using System;
using System.Linq;

namespace Orchard.ContentManagement.Handlers {
    /// <summary>
    /// Filter reponsible for adding a part to a content item when content items are activated
    /// </summary>
    public class ActivatingFilter<TPart> : IContentActivatingFilter where TPart : ContentPart, new() {
        private readonly Func<string, bool> _predicate;

        public ActivatingFilter(Func<string, bool> predicate) {
            _predicate = predicate;
        }

        public ActivatingFilter(params string[] contentTypes)
            : this(contentType => contentTypes.Contains(contentType)) {
        }

        public void Activating(ActivatingContentContext context) {
            if (_predicate(context.ContentType))
                context.Builder.Weld<TPart>();
        }
    }
}

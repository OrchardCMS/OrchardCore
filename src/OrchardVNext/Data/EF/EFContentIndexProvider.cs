using System;
using System.Collections.Generic;
using System.Linq;
using OrchardVNext.ContentManagement;

namespace OrchardVNext.Data.EF {
    public class EFContentIndexProvider : IContentIndexProvider {
        private readonly DataContext _dataContext;

        public EFContentIndexProvider(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public void Index(IContent content) {
            var contentType = content.ContentItem.ContentType;

            //_dataContext
            //    .Set<ContentItemVersionRecord>()
            //    .Where(n => n.ContentItemRecord.ContentType == "foo");


            //var p = GetByFilter(x => x.ContentItem.ContentType == "ddd");

            //Func<IContent, bool> filter = (contentItem) => contentItem.ContentItem.ContentType == "foo";
            //filter.
        }

        public IEnumerable<int> GetByFilter(Func<IContent, bool> filter) {
            return Enumerable.Empty<int>();
        }
    }
}
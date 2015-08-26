using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Data {
    public class ContentIndexResult<TDocument> {
        public IReadOnlyList<TDocument> Records { get; set; }

        public IReadOnlyList<TDocument> Reduce(Func<TDocument, bool> reduce) {
            return Records.Where(reduce).ToList();
        }
    }
}
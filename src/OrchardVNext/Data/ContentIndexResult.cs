using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.Data {
    public class ContentIndexResult<TDocument> {
        public IEnumerable<TDocument> Records { get; set; }

        public IEnumerable<TDocument> Reduce(Func<TDocument, bool> reduce) {
            return Records.Where(reduce);
        }
    }
}
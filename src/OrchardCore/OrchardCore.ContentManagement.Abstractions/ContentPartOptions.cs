using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    public class ContentPartOptions
    {
        public IEnumerable<ContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<ContentPartOption>();
    }
}
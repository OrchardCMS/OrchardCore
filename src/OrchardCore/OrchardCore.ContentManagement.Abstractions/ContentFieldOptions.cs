using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    public class ContentFieldOptions
    {
        public IEnumerable<ContentFieldOption> FieldOptions { get; set; }
            = Enumerable.Empty<ContentFieldOption>();

    }
}
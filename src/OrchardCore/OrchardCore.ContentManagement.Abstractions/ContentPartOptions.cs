using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.ContentManagement
{
    public class ContentPartOptions
    {
        public IEnumerable<ContentPartOption> PartOptions { get; set; }
            = Enumerable.Empty<ContentPartOption>();

        public ContentPartOptions AddPart<TContentPart>()
            where TContentPart : ContentPart
        {
            var option = new ContentPartOption<TContentPart>();

            PartOptions = PartOptions.Union(new[] { option });

            return this;
        }

        public ContentPartOptions AddPart<TContentPart>(Action<ContentPartOption> action)
            where TContentPart : ContentPart
        {
            var option = new ContentPartOption<TContentPart>();

            action(option);

            PartOptions = PartOptions.Union(new[] { option });

            return this;
        }
    }
}
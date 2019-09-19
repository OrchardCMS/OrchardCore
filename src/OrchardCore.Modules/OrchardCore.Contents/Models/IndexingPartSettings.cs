using System.ComponentModel;

namespace OrchardCore.Contents.Models
{
    public class IndexingPartSettings
    {
        //TODO don't index full content item #3809
        public bool IsNotIndexingFullTextOrAll { get; set; }

        [DefaultValue(true)]
        public bool IndexBodyAspect { get; set; } = true;

        [DefaultValue(true)]
        public bool IndexDisplayText { get; set; } = true;
    }
}
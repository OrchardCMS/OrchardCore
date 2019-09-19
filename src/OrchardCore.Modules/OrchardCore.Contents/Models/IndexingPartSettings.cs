using System.ComponentModel;

namespace OrchardCore.Contents.Models
{
    public class IndexingPartSettings
    {
        //TODO don't index full content item #3809
        public bool IsNotIndexingFullTextOrAll { get; set; }
    }
}
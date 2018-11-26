using YesSql.Indexes;

namespace OrchardCore.ContentManagement.Routable
{
    public class RoutableIndex : MapIndex
    {
        public static int MaxPathLength = 1024;

        public string ContentItemId { get; set; }
        public string RootContentItemId { get; set; }
        public string Path { get; set; }
        public string JsonPath { get; set; }
        public bool Published { get; set; }
    }
}

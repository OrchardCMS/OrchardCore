namespace OrchardCore.Indexing.Core;

public static class IndexingConstants
{
    public const string ContentsIndexSource = "Content";

    public static class Feature
    {
        public const string Area = "OrchardCore.Indexing";

        public const string Worker = "OrchardCore.Indexing.Worker";
    }

    // Breadcrumb trail names.
    public const string List = "Indexes";
    public const string Create = "IndexesCreate";
    public const string Edit = "IndexesEdit";
    public const string DisplayNameKey = "DisplayName";
}

namespace OrchardCore.Contents.JsonApi
{
    internal static class RouteHelpers
    {
        public const string AreaName = "OrchardCore.Contents";

        public class ContentItems
        {
            public const string ApiRouteByIdName = "Api.GetContents.ById";
            public const string ApiRouteByVersionName = "Api.GetContents.ByVersion";
        }

        public class ContentTypes {
            public const string ApiRouteByNameName = "Api.GetContentType.ByName";
        }
        
    }

    internal static class LinkKeyworks
    {
        public const string LatestVersion = "latest-version";
        public const string PublishedVersion = "published-version";
    }
}

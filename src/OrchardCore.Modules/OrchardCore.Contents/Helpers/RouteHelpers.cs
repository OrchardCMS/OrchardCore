namespace OrchardCore.Contents.Helpers
{
    internal static class RouteHelpers
    {
        public const string AreaName = "OrchardCore.Contents";

        public class ContentItems
        {
            public const string ApiRouteByIdName = "Api.GetContents.ById";
            public const string ApiRouteByVersionName = "Api.GetContents.ByVersion";
            public const string ApiRouteByTypeName = "Api.GetContents.ByType";
        }

        public class ContentTypes {
            public const string ApiRouteByNameName = "Api.GetContentType.ByName";
        }
    }
}

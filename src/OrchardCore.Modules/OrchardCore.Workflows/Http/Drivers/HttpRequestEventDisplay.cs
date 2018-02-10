using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRequestEventDisplay : ActivityDisplayDriver<HttpRequestEvent, HttpRequestEventViewModel>
    {
        protected override void Map(HttpRequestEvent source, HttpRequestEventViewModel target)
        {
            target.HttpMethod = source.HttpMethod;
            target.Url = source.Url;
        }

        protected override void Map(HttpRequestEventViewModel source, HttpRequestEvent target)
        {
            target.HttpMethod = source.HttpMethod?.Trim();
            target.Url = source.Url?.Trim();
        }
    }
}

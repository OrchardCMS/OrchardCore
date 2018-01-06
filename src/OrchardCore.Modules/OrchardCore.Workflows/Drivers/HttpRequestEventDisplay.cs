using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class HttpRequestEventDisplay : ActivityDisplayDriver<HttpRequestEvent, HttpRequestEventViewModel>
    {
        protected override void Map(HttpRequestEvent source, HttpRequestEventViewModel target)
        {
            target.HttpMethod = source.HttpMethod;
            target.RequestPath = source.RequestPath;
        }

        protected override void Map(HttpRequestEventViewModel source, HttpRequestEvent target)
        {
            target.HttpMethod = source.HttpMethod?.Trim();
            target.RequestPath = source.RequestPath?.Trim();
        }
    }
}

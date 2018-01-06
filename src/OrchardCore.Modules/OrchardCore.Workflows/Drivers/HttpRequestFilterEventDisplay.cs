using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class HttpRequestFilterEventDisplay : ActivityDisplayDriver<HttpRequestFilterEvent, HttpRequestFilterEventViewModel>
    {
        protected override void Map(HttpRequestFilterEvent source, HttpRequestFilterEventViewModel target)
        {
            target.HttpMethod = source.HttpMethod;
            target.ControllerName = source.ControllerName;
            target.ActionName = source.ActionName;
            target.AreaName = source.AreaName;
        }

        protected override void Map(HttpRequestFilterEventViewModel source, HttpRequestFilterEvent target)
        {
            target.HttpMethod = source.HttpMethod?.Trim();
            target.ControllerName = NullIfEmpty(source.ControllerName);
            target.ActionName = NullIfEmpty(source.ActionName);
            target.AreaName = NullIfEmpty(source.AreaName);
        }

        private string NullIfEmpty(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }
    }
}

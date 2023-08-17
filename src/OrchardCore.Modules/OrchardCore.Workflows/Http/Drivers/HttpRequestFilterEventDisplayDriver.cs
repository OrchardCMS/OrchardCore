using System;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;

namespace OrchardCore.Workflows.Http.Drivers
{
    public class HttpRequestFilterEventDisplayDriver : ActivityDisplayDriver<HttpRequestFilterEvent, HttpRequestFilterEventViewModel>
    {
        protected override void EditActivity(HttpRequestFilterEvent activity, HttpRequestFilterEventViewModel model)
        {
            model.HttpMethod = activity.HttpMethod;
            model.ControllerName = activity.ControllerName;
            model.ActionName = activity.ActionName;
            model.AreaName = activity.AreaName;
        }

        protected override void UpdateActivity(HttpRequestFilterEventViewModel model, HttpRequestFilterEvent activity)
        {
            activity.HttpMethod = model.HttpMethod?.Trim();
            activity.ControllerName = NullIfEmpty(model.ControllerName);
            activity.ActionName = NullIfEmpty(model.ActionName);
            activity.AreaName = NullIfEmpty(model.AreaName);
        }

        private static string NullIfEmpty(string s)
        {
            return String.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }
    }
}

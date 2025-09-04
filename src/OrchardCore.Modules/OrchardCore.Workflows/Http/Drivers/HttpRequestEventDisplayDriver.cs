using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.ViewModels;

namespace OrchardCore.Workflows.Http.Drivers;

public sealed class HttpRequestEventDisplayDriver : ActivityDisplayDriver<HttpRequestEvent, HttpRequestEventViewModel>
{
    protected override void EditActivity(HttpRequestEvent activity, HttpRequestEventViewModel model)
    {
        model.HttpMethod = activity.HttpMethod;
        model.Url = activity.Url;
        model.ValidateAntiforgeryToken = activity.ValidateAntiforgeryToken;
        model.TokenLifeSpan = activity.TokenLifeSpan;
        model.FormLocationKey = activity.FormLocationKey;
    }

    protected override void UpdateActivity(HttpRequestEventViewModel model, HttpRequestEvent activity)
    {
        activity.HttpMethod = model.HttpMethod?.Trim();
        activity.Url = model.Url?.Trim();
        activity.ValidateAntiforgeryToken = model.ValidateAntiforgeryToken;
        activity.TokenLifeSpan = model.TokenLifeSpan;
        activity.FormLocationKey = model.FormLocationKey?.Trim();
    }
}

using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services;

/// <summary>
/// The functions workflow scripts call to read the HTTP request and write the response.
/// </summary>
public sealed class HttpWorkflowGlobalValueProvider : IWorkflowGlobalValueProvider
{
    internal readonly IStringLocalizer S;

    public HttpWorkflowGlobalValueProvider(IStringLocalizer<HttpWorkflowGlobalValueProvider> localizer)
    {
        S = localizer;
    }

    public IEnumerable<WorkflowGlobalValue> GetGlobalValues()
        =>
        [
            WorkflowGlobalValue.Function("queryString(\"name\")", "string", S["Returns a value of the query string; queryString() returns all of it."]),
            WorkflowGlobalValue.Function("requestForm(\"name\")", "string", S["Returns a value of the posted form."]),
            WorkflowGlobalValue.Function("readBody()", "string", S["Returns the body of the request."]),
            WorkflowGlobalValue.Function("deserializeRequestData()", "object", S["Returns the query string, the posted form or the JSON body as an object."]),
            WorkflowGlobalValue.Function("absoluteUrl(\"/path\")", "string", S["Returns the absolute URL of a path of the site."]),
            WorkflowGlobalValue.Function("httpContext()", "object", S["Returns the HttpContext of the request."]),
            WorkflowGlobalValue.Function("signalUrl(\"signal\")", "string", S["Returns a URL that triggers the Signal event of this instance."]),
            WorkflowGlobalValue.Function("responseWrite(text)", "any", S["Writes text to the response."]),
        ];
}

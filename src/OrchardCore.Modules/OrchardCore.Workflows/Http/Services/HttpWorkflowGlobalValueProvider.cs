using OrchardCore.Localization;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Http.Services;

/// <summary>
/// The functions workflow scripts call to read the HTTP request and write the response.
/// </summary>
public sealed class HttpWorkflowGlobalValueProvider : IWorkflowGlobalValueProvider
{
    // The values don't depend on the request or the culture, so they're created once.
    private static readonly WorkflowGlobalValue[] s_globalValues =
    [
        WorkflowGlobalValue.Function("queryString(\"name\")", "string", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns a value of the query string; queryString() returns all of it.")),
        WorkflowGlobalValue.Function("requestForm(\"name\")", "string", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns a value of the posted form.")),
        WorkflowGlobalValue.Function("readBody()", "string", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns the body of the request.")),
        WorkflowGlobalValue.Function("deserializeRequestData()", "object", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns the query string, the posted form or the JSON body as an object.")),
        WorkflowGlobalValue.Function("absoluteUrl(\"/path\")", "string", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns the absolute URL of a path of the site.")),
        WorkflowGlobalValue.Function("httpContext()", "object", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns the HttpContext of the request.")),
        WorkflowGlobalValue.Function("signalUrl(\"signal\")", "string", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Returns a URL that triggers the Signal event of this instance.")),
        WorkflowGlobalValue.Function("responseWrite(text)", "any", LocalizationSource.Create<HttpWorkflowGlobalValueProvider>("Writes text to the response.")),
    ];

    public IEnumerable<WorkflowGlobalValue> GetGlobalValues()
        => s_globalValues;
}

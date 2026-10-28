using OrchardCore.Localization;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// The global Liquid values (the site, the user, the request, the culture and the environment) and the functions
/// every workflow script can call.
/// </summary>
public sealed class DefaultWorkflowGlobalValueProvider : IWorkflowGlobalValueProvider
{
    // The values don't depend on the request or the culture, so they're created once.
    private static readonly WorkflowGlobalValue[] s_globalValues =
    [
        WorkflowGlobalValue.Liquid("Site", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The settings of the site."),
        [
            Member("SiteName", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The name of the site.")),
            Member("BaseUrl", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The base URL of the site.")),
            Member("TimeZoneId", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The time zone of the site.")),
            Member("Calendar", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The calendar of the site.")),
            Member("PageTitleFormat", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The format of the page titles.")),
            Member("PageSize", "number", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The default page size.")),
            Member("MaxPageSize", "number", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The largest page size.")),
            Member("SuperUser", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The id of the super user.")),
            Member("UseCdn", "boolean", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Whether resources come from a CDN.")),
            Member("CdnBaseUrl", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The base URL of the CDN.")),
            Member("Properties", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The custom settings of the site that Liquid may read.")),
        ]),
        WorkflowGlobalValue.Liquid("User", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The user of the current request; nothing when the workflow doesn't run in a request."),
        [
            Member("Identity.Name", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The name of the user.")),
            Member("Identity.IsAuthenticated", "boolean", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Whether the user is signed in.")),
            Member("Identity.Claims", "array", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The claims of the user.")),
        ]),
        WorkflowGlobalValue.Liquid("Request", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The current HTTP request; nothing when the workflow doesn't run in a request."),
        [
            Member("Path", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The path of the request.")),
            Member("PathBase", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The base path of the request.")),
            Member("QueryString", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The query string, with its ?.")),
            Member("Host", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The host of the request.")),
            Member("Scheme", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The scheme, http or https.")),
            Member("Method", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The HTTP method, for example GET.")),
            Member("IsHttps", "boolean", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Whether the request uses HTTPS.")),
            Member("ContentType", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The content type of the request.")),
            Member("Query", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The values of the query string, by name: Request.Query[\"name\"].")),
            Member("Form", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The posted form values, by name: Request.Form[\"name\"].")),
            Member("Headers", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The headers, by name.")),
            Member("Cookies", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The cookies, by name.")),
            Member("RouteValues", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The route values, by name.")),
        ]),
        WorkflowGlobalValue.Liquid("Culture", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The culture of the current request."),
        [
            Member("Name", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The name of the culture, for example en-US.")),
            Member("DisplayName", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The display name of the culture.")),
            Member("NativeName", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The name of the culture in its own language.")),
            Member("TwoLetterISOLanguageName", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The two-letter code of the language.")),
            Member("Dir", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The direction of the text, ltr or rtl.")),
        ]),
        WorkflowGlobalValue.Liquid("Environment", "object", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The hosting environment."),
        [
            Member("Name", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("The name of the environment, for example Production.")),
            Member("IsDevelopment", "boolean", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Whether it is the Development environment.")),
            Member("IsStaging", "boolean", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Whether it is the Staging environment.")),
            Member("IsProduction", "boolean", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Whether it is the Production environment.")),
        ]),
        WorkflowGlobalValue.Function("workflowId()", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Returns the id of the workflow instance.")),
        WorkflowGlobalValue.Function("setProperty(\"name\", value)", "any", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Sets a property of the workflow.")),
        WorkflowGlobalValue.Function("setVariable(\"name\", value)", "any", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Sets a variable, converting the value to its type.")),
        WorkflowGlobalValue.Function("output(\"name\", value)", "any", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Sets an output of the workflow.")),
        WorkflowGlobalValue.Function("setCorrelationId(id)", "any", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Sets the correlation id of the instance.")),
        WorkflowGlobalValue.Function("uuid()", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Returns a new unique id.")),
        WorkflowGlobalValue.Function("log(\"Information\", \"text\", value)", "any", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Writes a message to the log, at a level such as Information or Warning.")),
        WorkflowGlobalValue.Function("base64(text)", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Decodes Base64 text.")),
        WorkflowGlobalValue.Function("html(text)", "string", LocalizationSource.Create<DefaultWorkflowGlobalValueProvider>("Decodes HTML-encoded text.")),
    ];

    public IEnumerable<WorkflowGlobalValue> GetGlobalValues()
        => s_globalValues;

    private static ActivityProvidedValueMember Member(string name, string typeName, LocalizationSource description)
        => new() { Name = name, TypeName = typeName, Description = description };
}

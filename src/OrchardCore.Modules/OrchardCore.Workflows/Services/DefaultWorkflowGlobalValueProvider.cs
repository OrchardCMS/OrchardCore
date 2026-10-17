using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Activities;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// The global Liquid values (the site, the user, the request, the culture and the environment) and the functions
/// every workflow script can call.
/// </summary>
public sealed class DefaultWorkflowGlobalValueProvider : IWorkflowGlobalValueProvider
{
    internal readonly IStringLocalizer S;

    public DefaultWorkflowGlobalValueProvider(IStringLocalizer<DefaultWorkflowGlobalValueProvider> localizer)
    {
        S = localizer;
    }

    public IEnumerable<WorkflowGlobalValue> GetGlobalValues()
        =>
        [
            WorkflowGlobalValue.Liquid("Site", "object", S["The settings of the site."],
            [
                Member("SiteName", "string", S["The name of the site."]),
                Member("BaseUrl", "string", S["The base URL of the site."]),
                Member("TimeZoneId", "string", S["The time zone of the site."]),
                Member("Calendar", "string", S["The calendar of the site."]),
                Member("PageTitleFormat", "string", S["The format of the page titles."]),
                Member("PageSize", "number", S["The default page size."]),
                Member("MaxPageSize", "number", S["The largest page size."]),
                Member("SuperUser", "string", S["The id of the super user."]),
                Member("UseCdn", "boolean", S["Whether resources come from a CDN."]),
                Member("CdnBaseUrl", "string", S["The base URL of the CDN."]),
                Member("Properties", "object", S["The custom settings of the site that Liquid may read."]),
            ]),
            WorkflowGlobalValue.Liquid("User", "object", S["The user of the current request; nothing when the workflow doesn't run in a request."],
            [
                Member("Identity.Name", "string", S["The name of the user."]),
                Member("Identity.IsAuthenticated", "boolean", S["Whether the user is signed in."]),
                Member("Identity.Claims", "array", S["The claims of the user."]),
            ]),
            WorkflowGlobalValue.Liquid("Request", "object", S["The current HTTP request; nothing when the workflow doesn't run in a request."],
            [
                Member("Path", "string", S["The path of the request."]),
                Member("PathBase", "string", S["The base path of the request."]),
                Member("QueryString", "string", S["The query string, with its ?."]),
                Member("Host", "string", S["The host of the request."]),
                Member("Scheme", "string", S["The scheme, http or https."]),
                Member("Method", "string", S["The HTTP method, for example GET."]),
                Member("IsHttps", "boolean", S["Whether the request uses HTTPS."]),
                Member("ContentType", "string", S["The content type of the request."]),
                Member("Query", "object", S["The values of the query string, by name: Request.Query[\"name\"]."]),
                Member("Form", "object", S["The posted form values, by name: Request.Form[\"name\"]."]),
                Member("Headers", "object", S["The headers, by name."]),
                Member("Cookies", "object", S["The cookies, by name."]),
                Member("RouteValues", "object", S["The route values, by name."]),
            ]),
            WorkflowGlobalValue.Liquid("Culture", "object", S["The culture of the current request."],
            [
                Member("Name", "string", S["The name of the culture, for example en-US."]),
                Member("DisplayName", "string", S["The display name of the culture."]),
                Member("NativeName", "string", S["The name of the culture in its own language."]),
                Member("TwoLetterISOLanguageName", "string", S["The two-letter code of the language."]),
                Member("Dir", "string", S["The direction of the text, ltr or rtl."]),
            ]),
            WorkflowGlobalValue.Liquid("Environment", "object", S["The hosting environment."],
            [
                Member("Name", "string", S["The name of the environment, for example Production."]),
                Member("IsDevelopment", "boolean", S["Whether it is the Development environment."]),
                Member("IsStaging", "boolean", S["Whether it is the Staging environment."]),
                Member("IsProduction", "boolean", S["Whether it is the Production environment."]),
            ]),
            WorkflowGlobalValue.Function("workflowId()", "string", S["Returns the id of the workflow instance."]),
            WorkflowGlobalValue.Function("setProperty(\"name\", value)", "any", S["Sets a property of the workflow."]),
            WorkflowGlobalValue.Function("setVariable(\"name\", value)", "any", S["Sets a variable, converting the value to its type."]),
            WorkflowGlobalValue.Function("output(\"name\", value)", "any", S["Sets an output of the workflow."]),
            WorkflowGlobalValue.Function("setCorrelationId(id)", "any", S["Sets the correlation id of the instance."]),
            WorkflowGlobalValue.Function("uuid()", "string", S["Returns a new unique id."]),
            WorkflowGlobalValue.Function("log(\"Information\", \"text\", value)", "any", S["Writes a message to the log, at a level such as Information or Warning."]),
            WorkflowGlobalValue.Function("base64(text)", "string", S["Decodes Base64 text."]),
            WorkflowGlobalValue.Function("html(text)", "string", S["Decodes HTML-encoded text."]),
        ];

    private static ActivityProvidedValueMember Member(string name, string typeName, LocalizedString description)
        => new() { Name = name, TypeName = typeName, Description = description };
}

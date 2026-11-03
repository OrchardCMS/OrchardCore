using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

/// <summary>
/// The <c>x-oc-cli</c> metadata that the tenant endpoints publish, read the way the CLI reads it from a tenant.
/// </summary>
internal static class TenantOperations
{
    private const string EnableStep = """{"description":"enable remote management for this tenant.","whenProperty":"state","whenValue":"Running","arguments":["tenants","enable-remote-management","{name}"]}""";

    private const string Summary = """ "resourceName":"Tenant","summaryProperties":["name","tenantId","state","setupUrl","primaryUrl","url","requestUrlHost","requestUrlPrefix","recipeName","databaseProvider"] """;

    private static readonly Dictionary<string, string> _extensions = new()
    {
        ["create"] = $$"""
            {{{Summary}},"nextSteps":[
              {"description":"set up this tenant (replace the placeholders and adjust the site details).","whenProperty":"state","whenValue":"Uninitialized",
               "arguments":["tenants","setup","{name}","--site-name","{name}","--user-name","admin","--email","<admin-email>"],
               "optionalArguments":[{"whenMissing":"recipeName","arguments":["--recipe-name","<recipe-name>"]},{"whenMissing":"databaseProvider","arguments":["--database-provider","Sqlite"]}],
               "note":"The administrator password will be prompted securely. Choose a recipe such as SaaS or Blog if one is not already configured."},
              {{EnableStep}},
              {"description":"start this tenant.","whenProperty":"state","whenValue":"Disabled","arguments":["tenants","start","{name}"]}]}
            """,
        ["install"] = $$"""{{{Summary}},"provisioningProperty":"enableRemoteManagement","nextSteps":[{{EnableStep}}]}""",
        ["setup"] = $$"""{{{Summary}},"nextSteps":[{{EnableStep}}]}""",
        ["start"] = $$"""{{{Summary}},"nextSteps":[{{EnableStep}}]}""",
        ["delete"] = $$"""{{{Summary}}}""",
        ["enable-remote-management"] = $$"""{{{Summary}},"completedAction":"configured for remote management","provisioningProperty":"provisionClient","nextSteps":[{"whenProperty":"state","whenValue":"Running","savesContext":true}]}""",
        ["disable-remote-management"] = $$"""{{{Summary}},"completedAction":"disabled for remote management","nextSteps":[{"description":"the contexts saved for this tenant stop working until you enable remote management again.","arguments":["tenants","enable-remote-management","{name}"]}]}""",
        ["reload"] = $$"""{{{Summary}},"completedAction":"reloaded"}""",
    };

    public static CliOperationMetadata Get(string verb)
    {
        var extension = _extensions[verb].TrimStart()[1..];
        var document = """{"paths":{"/api/tenants":{"post":{"x-oc-cli":{"commandGroup":["tenants"],"verb":""" + $"\"{verb}\"," + extension + "}}}}";

        return Assert.Single(OpenApiCliParser.Parse(document)).CliMetadata;
    }
}

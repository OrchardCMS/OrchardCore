# OpenApi (`OrchardCore.OpenApi`)

## Implementation

If you want to provide documentation for your Api, implement the IOpenApiDefinition interface from the OrchardCore.OpenApi.Abstractions module.

Use the Document property to describe general metadata about the Api.
Use the predicate to filter for objects that belong to your Api definition.

Example:

<code>
public class OrchardApiDefinition : IOpenApiDefinition
{
    public string Name => "OrchardCoreAPI";

    public OpenApiDocument Document => new OpenApiDocument()
    {   
        Info = new OpenApiInfo()
        {
            Version = "v2",
            Title = "OrchardCore API",
            Description = "An API to manage the OrchardCore installation",
            Contact = new OpenApiContact
            {
                Name = "Orchard Team",
                Email = "info@orchardproject.net",
                Url = new Uri("https://www.orchardproject.net")
            }
        }
    };

    public Func<string, ApiDescription, bool> ApiDescriptionFilterPredicate => (name, description) => description.RelativePath.StartsWith("api/tenants");
}
</code>

# Swagger (`OrchardCore.Swagger`)

Enables a Swagger endpoint (~/swagger) for documenting API's.

## Implementation

If you want to provide documentation for your API, implement the ISwaggerApiDefinition interface from the OrchardCore.Abstractions module.

Use the Document property to describe general metadata about the API.
Use the predicate to filter for objects that belong to your API definition.

Example;

<code>
public class OrchardApiDefinition : ISwaggerApiDefinition
{
    public string Name => "OrchardCoreAPI";

    public SwaggerDocument Document => new SwaggerDocument()
    {
        Info = new Info()
        {
            Version = "v2",
            Title = "OrchardCore API",
            Description = "An API to manage the OrchardCore installation",
            Contact = new Contact
            {
                Name = "Orchard Team",
                Email = "info@orchardproject.net",
                Url = "https://www.orchardproject.net"
            }
        }
    };

    public Func<string, ApiDescription, bool> ApiDescriptionFilterPredicate => (name, description) => description.RelativePath.StartsWith("api/tenants");
}
</code>
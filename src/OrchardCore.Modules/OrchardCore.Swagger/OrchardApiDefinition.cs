using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace OrchardCore.Swagger
{
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

        public Func<string, ApiDescription, bool> ApiDescriptionFilterPredicate => (name, description) => description.RelativePath.Contains("api/tenants");
    }
}

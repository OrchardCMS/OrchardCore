using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using OrchardCore.OpenApi;

namespace OrchardCore.Swagger
{
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

        public Func<string, ApiDescription, bool> ApiDescriptionFilterPredicate => (name, description) =>
                                                                                        description.RelativePath.StartsWith("api/tenants") ||
                                                                                        description.RelativePath.StartsWith("api/content") ||
                                                                                        description.RelativePath.StartsWith("api/queries");
    }
}

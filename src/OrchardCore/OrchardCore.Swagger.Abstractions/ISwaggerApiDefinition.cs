using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;

namespace OrchardCore.Swagger
{
    public interface ISwaggerApiDefinition
    {
        string Name { get; }

        SwaggerDocument Document { get; }

        Func<string, ApiDescription, bool> ApiDescriptionFilterPredicate { get; }
    }
}

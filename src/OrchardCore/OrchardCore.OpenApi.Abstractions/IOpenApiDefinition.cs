using System;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;

namespace OrchardCore.OpenApi
{
    public interface IOpenApiDefinition
    {
        string Name { get; }

        OpenApiDocument Document { get; }

        Func<string, ApiDescription, bool> ApiDescriptionFilterPredicate { get; }
    }
}

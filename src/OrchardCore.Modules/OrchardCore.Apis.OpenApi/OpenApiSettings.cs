using Microsoft.AspNetCore.Http;

namespace OrchardCore.Apis.OpenApi
{
    public class OpenApiSettings
    {
        public PathString Path { get; set; } = "/api.json";
    }
}
using Microsoft.Net.Http.Headers;

namespace OrchardCore.Apis.OpenApi
{
    internal class MediaTypeHeaderValues
    {
        public static readonly MediaTypeHeaderValue ApplicationJson
            = MediaTypeHeaderValue.Parse("application/json").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextJson
            = MediaTypeHeaderValue.Parse("text/json").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue ApplicationYaml
            = MediaTypeHeaderValue.Parse("application/yaml").CopyAsReadOnly();

        public static readonly MediaTypeHeaderValue TextYaml
            = MediaTypeHeaderValue.Parse("text/yaml").CopyAsReadOnly();
    }
}

using Microsoft.Net.Http.Headers;

namespace OrchardCore.Apis.JsonApi
{
    internal static class MediaTypeHeaderValues
    {
        public static readonly MediaTypeHeaderValue ApplicationJsonApi
            = MediaTypeHeaderValue.Parse("application/vnd.api+json").CopyAsReadOnly();
    }
}

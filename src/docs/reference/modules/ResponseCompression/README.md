# Response Compression (`OrchardCore.ResponseCompression`)

The `OrchardCore.ResponseCompression` module enables ASP.NET Core [response compression](https://learn.microsoft.com/aspnet/core/performance/response-compression) for the tenant, reducing the size of HTTP responses sent to clients.

## Usage

Enable the `Response Compression` feature. No further configuration is required: the module registers the response compression middleware with the default providers (gzip), with compression also enabled for HTTPS responses.

## When to use it

Prefer enabling compression at the reverse proxy or web server (IIS, Nginx, Kestrel front end, CDN) when one is in front of the application, as it is usually more efficient there. Use this module when no such layer handles compression, for example when the application is exposed directly.

!!! warning
    Compressing responses over HTTPS can expose the application to attacks such as [BREACH](https://learn.microsoft.com/aspnet/core/performance/response-compression#compression-with-secure-protocol). Enable this feature only if you understand and accept that risk for your dynamically generated, secured responses.

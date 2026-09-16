using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Endpoints;

internal static class SitemapSourceEndpoints
{
    public static void AddSitemapSourceEndpoints(this IEndpointRouteBuilder routes)
    {
        SitemapEndpoints.Configure(routes.MapGet("api/sitemap-source-types", TypesAsync), "ApiListSitemapSourceTypes", "types", "Lists source types with typed management contracts.", sources: true).Produces<string[]>();
        SitemapEndpoints.Configure(routes.MapGet("api/sitemap-source-types/{type}", SchemaAsync), "ApiGetSitemapSourceSchema", "schema", "Shows the complete writable source configuration schema.", "type", sources: true).Produces<JsonObject>();
        SitemapEndpoints.Configure(routes.MapGet("api/sitemaps/{id}/sources", ListAsync), "ApiListSitemapSources", "list", "Lists sources; unknown extensions expose identity only.", "id", sources: true).Produces<SitemapSourceResponse[]>();
        SitemapEndpoints.Configure(routes.MapPost("api/sitemaps/{id}/sources", CreateAsync), "ApiCreateSitemapSource", "create", "Adds a source. Read back its server-generated ID before retrying.", "id", true, true).Produces<SitemapSourceResponse>();
        SitemapEndpoints.Configure(routes.MapPut("api/sitemaps/{id}/sources/{sourceId}", UpdateAsync), "ApiUpdateSitemapSource", "update", "Replaces the configuration of a built-in source.", "id", true, true, "sourceId").Produces<SitemapSourceResponse>();
        SitemapEndpoints.Configure(routes.MapDelete("api/sitemaps/{id}/sources/{sourceId}", DeleteAsync), "ApiDeleteSitemapSource", "delete", "Deletes a source and invalidates its sitemap cache.", "id", sources: true, secondArgument: "sourceId").Produces(204);
    }
    internal static IResult TypesAsync([FromServices] SitemapSourceManagementService service) => TypedResults.Ok(service.Types());
    internal static IResult SchemaAsync(HttpContext context, [FromServices] SitemapSourceManagementService service, string type)
    {
        if (!service.Types().Contains(type)) { return context.ApiNotFoundProblem(); }
        var schema = SitemapSourceManagementService.JsonOptions.GetJsonSchemaAsNode(type == nameof(CustomPathSitemapSource)
            ? typeof(CustomPathSitemapSource) : typeof(ContentTypesSitemapSource)).AsObject();
        schema["properties"]?.AsObject().Remove("id");
        schema["properties"]?.AsObject().Remove("lastUpdate");
        return TypedResults.Ok(schema);
    }
    internal static async Task<IResult> ListAsync(HttpContext context, [FromServices] ISitemapManager manager, string id)
    {
        var sitemap = await manager.GetSitemapAsync(id);
        return sitemap is null ? context.ApiNotFoundProblem() : TypedResults.Ok(sitemap.SitemapSources.Select(SitemapSourceManagementService.Describe).ToArray());
    }
    internal static Task<IResult> CreateAsync(HttpContext context, [FromServices] SitemapSourceManagementService service, string id, [FromBody] SitemapSourceDefinition input)
        => SaveAsync(context, service, id, input, null);
    internal static Task<IResult> UpdateAsync(HttpContext context, [FromServices] SitemapSourceManagementService service, string id, string sourceId, [FromBody] SitemapSourceDefinition input)
        => SaveAsync(context, service, id, input, sourceId);
    private static async Task<IResult> SaveAsync(HttpContext context, SitemapSourceManagementService service, string id, SitemapSourceDefinition input, string sourceId)
    {
        var result = await service.SaveAsync(id, input, sourceId);
        if (result.NotFound) { return context.ApiNotFoundProblem(); }
        if (result.Errors.Count > 0) { return TypedResults.ValidationProblem(result.Errors); }
        return TypedResults.Ok(SitemapSourceManagementService.Describe(result.Source));
    }
    internal static async Task<IResult> DeleteAsync(HttpContext context, [FromServices] ISitemapManager manager, string id, string sourceId)
    {
        var sitemap = await manager.LoadSitemapAsync(id);
        if (sitemap is null) { return context.ApiNotFoundProblem(); }
        if (sitemap is not Sitemap) { return TypedResults.Problem("Update an index's contained sitemap IDs instead.", statusCode: 400); }
        if (sitemap.SitemapSources.RemoveAll(source => source.Id == sourceId) > 0) { await manager.UpdateSitemapAsync(sitemap); }
        return TypedResults.NoContent();
    }
}

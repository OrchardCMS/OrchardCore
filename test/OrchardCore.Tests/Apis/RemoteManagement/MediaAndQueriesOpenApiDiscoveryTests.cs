using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Media.Endpoints.Api;
using OrchardCore.Queries.Endpoints.Api;
using OrchardCore.RemoteManagement;
using OrchardCore.Settings;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class MediaAndQueriesOpenApiDiscoveryTests
{
    [Fact]
    public void Endpoints_ExposeCanonicalMediaAndQueryManagementMetadata()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddAuthorization();
        builder.Services.AddControllersWithViews();
        builder.Services.AddMemoryCache();
        builder.Services.AddLocalization();
        builder.Services.AddSingleton<ISiteService, TestSiteService>();
        new global::OrchardCore.Media.Startup().ConfigureServices(builder.Services);
        new global::OrchardCore.Queries.Startup().ConfigureServices(builder.Services);
        new global::OrchardCore.Queries.CoreStartup().ConfigureServices(builder.Services);
        new global::OrchardCore.Queries.Sql.Startup().ConfigureServices(builder.Services);

        var app = builder.Build();

        app.AddGetLocalizationsEndpoint()
            .AddGetPermittedStorageEndpoint()
            .AddGetDirectoryTreeEndpoint()
            .AddGetFoldersEndpoint()
            .AddGetMediaItemsEndpoint()
            .AddGetDirectoryContentEndpoint()
            .AddGetMediaItemEndpoint()
            .AddGetMediaFieldItemsEndpoint()
            .AddGetAllMediaItemsEndpoint()
            .AddGetTusFileInfoEndpoint()
            .AddCopyMediaEndpoint()
            .AddDeleteFolderEndpoint()
            .AddDeleteMediaEndpoint()
            .AddMoveMediaEndpoint()
            .AddDeleteMediaListEndpoint()
            .AddMoveMediaListEndpoint()
            .AddCreateFolderEndpoint()
            .AddUploadMediaEndpoint();

        app.AddQueryManagementEndpoints();

        var endpoints = ((IEndpointRouteBuilder)app).DataSources.SelectMany(dataSource => dataSource.Endpoints).OfType<RouteEndpoint>().ToArray();

        AssertOperation(endpoints, "api/media/constraints", "GET", "ApiGetPermittedStorage", ["media", "constraints"], "show");
        AssertOperation(endpoints, "api/media/folders", "GET", "ApiGetFolders", ["media", "folders"], "list");
        AssertOperation(endpoints, "api/media/folders", "POST", "ApiCreateFolder", ["media", "folders"], "create");
        AssertOperation(endpoints, "api/media/file", "GET", "ApiGetMediaItem", ["media", "files"], "show");
        AssertOperation(endpoints, "api/media/file", "DELETE", "ApiDeleteMedia", ["media", "files"], "delete");
        AssertOperation(endpoints, "api/media/files", "GET", "ApiGetMediaItems", ["media", "files"], "list");
        AssertOperation(endpoints, "api/media/items", "GET", "ApiGetAllMediaItems", ["media", "items"], "list");
        AssertOperation(endpoints, "api/media/files:copy", "POST", "ApiCopyMedia", ["media", "files"], "copy");
        AssertOperation(endpoints, "api/media/files:move", "POST", "ApiMoveMedia", ["media", "files"], "move");
        AssertOperation(endpoints, "api/media/files:delete", "POST", "ApiDeleteMediaList", ["media", "files"], "delete-batch", expectedInputMode: CliInputMode.Json, expectedRequestContentType: "application/json");
        AssertOperation(endpoints, "api/media/files:move-batch", "POST", "ApiMoveMediaList", ["media", "files"], "move-batch", expectedInputMode: CliInputMode.Json, expectedRequestContentType: "application/json");
        AssertOperation(endpoints, "api/media/files/content", "PUT", "ApiUploadMedia", ["media", "files"], "upload", expectedInputMode: CliInputMode.Stream, expectedRequestContentType: "application/octet-stream");
        AssertOperation(endpoints, "api/media/uploads/{uploadId}", "GET", "ApiGetTusFileInfo", ["media", "uploads"], "show");
        AssertOperation(endpoints, "api/media/files/metadata", "GET", "ApiGetMediaFieldItems", ["media", "metadata"], "show");
        AssertOperation(endpoints, "api/media/localizations", "GET", "ApiGetMediaLocalizations", ["media", "localizations"], "show");

        foreach (var name in new[] { "ApiGetAllMediaItems", "ApiGetMediaItems", "ApiGetMediaItem", "ApiUploadMedia", "ApiCopyMedia", "ApiMoveMedia", "ApiMoveMediaList", "ApiGetTusFileInfo" })
        {
            var cli = endpoints.Single(endpoint => endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName == name)
                .Metadata.GetMetadata<CliOperationMetadata>();
            Assert.Contains(cli.TableColumns, column => column.Heading == "Path");
            Assert.Contains(cli.TableColumns, column => column.Heading == "URL");
        }

        AssertOperation(endpoints, "api/queries", "GET", "ApiListQueries", ["queries"], "list");
        AssertOperation(endpoints, "api/queries", "POST", "ApiCreateQuery", ["queries"], "create", expectedInputMode: CliInputMode.Json, expectedRequestContentType: "application/json");
        AssertOperation(endpoints, "api/queries/sources", "GET", "ApiListQuerySources", ["queries", "sources"], "list");
        AssertOperation(endpoints, "api/queries/validate", "POST", "ApiValidateQuery", ["queries"], "validate", expectedInputMode: CliInputMode.Json, expectedRequestContentType: "application/json");
        AssertOperation(endpoints, "api/queries/named/{name}", "GET", "ApiGetQuery", ["queries"], "show");
        AssertOperation(endpoints, "api/queries/named/{name}", "PUT", "ApiUpdateQuery", ["queries"], "update", expectedInputMode: CliInputMode.Json, expectedRequestContentType: "application/json");
        AssertOperation(endpoints, "api/queries/named/{name}", "DELETE", "ApiDeleteQuery", ["queries"], "delete");
        AssertOperation(endpoints, "api/queries/named/{name}/execute", "POST", "ApiExecuteQuery", ["queries"], "execute", expectedInputMode: CliInputMode.Json, expectedRequestContentType: "application/json", expectedRequestBodyOptional: true, expectedDefaultJsonBody: "{}");
    }

    private static void AssertOperation(
        IEnumerable<RouteEndpoint> endpoints,
        string route,
        string method,
        string endpointName,
        string[] commandGroup,
        string verb,
        CliInputMode expectedInputMode = CliInputMode.Options,
        string expectedRequestContentType = null,
        bool expectedRequestBodyOptional = false,
        string expectedDefaultJsonBody = null)
    {
        var endpoint = endpoints.FirstOrDefault(endpoint =>
            string.Equals(endpoint.RoutePattern.RawText, route, StringComparison.Ordinal)
            && endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase) == true);

        Assert.NotNull(endpoint);
        Assert.Equal(endpointName, endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName);
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointSummaryMetadata>()?.Summary));
        Assert.False(string.IsNullOrWhiteSpace(endpoint.Metadata.GetMetadata<IEndpointDescriptionMetadata>()?.Description));

        var cli = endpoint.Metadata.GetMetadata<CliOperationMetadata>();
        Assert.NotNull(cli);
        Assert.Equal(commandGroup, cli.CommandGroup);
        Assert.Equal(verb, cli.Verb);
        Assert.Equal(expectedInputMode, cli.InputMode);
        Assert.Equal(expectedDefaultJsonBody, cli.DefaultJsonBody);

        if (expectedRequestContentType != null)
        {
            var accepts = endpoint.Metadata.GetMetadata<IAcceptsMetadata>();
            Assert.NotNull(accepts);
            Assert.Contains(expectedRequestContentType, accepts.ContentTypes);
            Assert.Equal(expectedRequestBodyOptional, accepts.IsOptional);
        }
    }

    private sealed class TestSiteService : ISiteService
    {
        private readonly TestSite _site = new();

        public Task<ISite> LoadSiteSettingsAsync() => Task.FromResult<ISite>(_site);

        public Task<ISite> GetSiteSettingsAsync() => Task.FromResult<ISite>(_site);

        public Task UpdateSiteSettingsAsync(ISite site) => Task.CompletedTask;
    }

    private sealed class TestSite : Entity, ISite
    {
        public string SiteName { get; set; } = string.Empty;
        public string PageTitleFormat { get; set; } = string.Empty;
        public string SiteSalt { get; set; } = string.Empty;
        public string SuperUser { get; set; } = string.Empty;
        public string Calendar { get; set; } = string.Empty;
        public string TimeZoneId { get; set; } = string.Empty;
        public ResourceDebugMode ResourceDebugMode { get; set; }
        public bool UseCdn { get; set; }
        public string CdnBaseUrl { get; set; } = string.Empty;
        public int PageSize { get; set; }
        public int MaxPageSize { get; set; }
        public int MaxPagedCount { get; set; }
        public bool AllowPageSizeSelection { get; set; }
        public int[] PageSizeOptions { get; set; } = [];
        public string BaseUrl { get; set; } = string.Empty;
        public RouteValueDictionary HomeRoute { get; set; } = [];
        public bool AppendVersion { get; set; }
        public CacheMode CacheMode { get; set; }

        public T As<T>() where T : new() => EntityExtensions.GetOrCreate<T>(this);

        public T GetOrCreate<T>() where T : new() => EntityExtensions.GetOrCreate<T>(this);

        public bool TryGet<T>(out T settings) => EntityExtensions.TryGet(this, out settings);
    }
}

using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Lucene;
using OrchardCore.Testing.Apis;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : SiteContextBase<SiteStartup>
    {
        private static readonly TablePrefixGenerator TablePrefixGenerator = new TablePrefixGenerator();
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static IShellHost ShellHost { get; private set; }
        public static IShellSettingsManager ShellSettingsManager { get; private set; }
        public static IHttpContextAccessor HttpContextAccessor { get; }
        public static HttpClient DefaultTenantClient { get; }

        public string RecipeName { get; set; } = "Blog";
        public string DatabaseProvider { get; set; } = "Sqlite";
        public string ConnectionString { get; set; }
        public PermissionsContext PermissionsContext { get; set; }

        public HttpClient Client { get; private set; }
        public string TenantName { get; private set; }
        public OrchardGraphQLClient GraphQLClient { get; private set; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
            ShellSettingsManager = Site.Services.GetRequiredService<IShellSettingsManager>();
            HttpContextAccessor = Site.Services.GetRequiredService<IHttpContextAccessor>();
            DefaultTenantClient = Site.CreateDefaultClient();
        }

        public override async Task InitializeAsync()
        {
            var tenantName = Guid.NewGuid().ToString("n");
            var tablePrefix = await TablePrefixGenerator.GeneratePrefixAsync();

            var createModel = new Tenants.ViewModels.CreateApiViewModel
            {
                DatabaseProvider = DatabaseProvider,
                TablePrefix = tablePrefix,
                ConnectionString = ConnectionString,
                RecipeName = RecipeName,
                Name = tenantName,
                RequestUrlPrefix = tenantName,
                Schema = null,
            };

            var createResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/create", createModel);
            createResult.EnsureSuccessStatusCode();

            var content = await createResult.Content.ReadAsStringAsync();

            var url = new Uri(content.Trim('"'));
            url = new Uri(url.Scheme + "://" + url.Authority + url.LocalPath + "/");

            var setupModel = new Tenants.ViewModels.SetupApiViewModel
            {
                SiteName = "Test Site",
                DatabaseProvider = DatabaseProvider,
                TablePrefix = tablePrefix,
                ConnectionString = ConnectionString,
                RecipeName = RecipeName,
                UserName = "admin",
                Password = "Password01_",
                Name = tenantName,
                Email = "Nick@Orchard",
            };

            var setupResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/setup", setupModel);
            setupResult.EnsureSuccessStatusCode();

            lock (Site)
            {
                Client = Site.CreateDefaultClient(url);
                TenantName = tenantName;
            }

            if (PermissionsContext != null)
            {
                var permissionContextKey = Guid.NewGuid().ToString();
                SiteStartup.PermissionsContexts.TryAdd(permissionContextKey, PermissionsContext);
                Client.DefaultRequestHeaders.Add("PermissionsContext", permissionContextKey);
            }

            GraphQLClient = new OrchardGraphQLClient(Client);
        }

        public async Task ResetLuceneIndiciesAsync(string indexName)
        {
            var shellScope = await ShellHost.GetScopeAsync(TenantName);

            await shellScope.UsingServiceScopeAsync(async scope =>
            {
                var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();

                var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync(indexName);

                luceneIndexingService.ResetIndexAsync(indexName);

                await luceneIndexingService.ProcessContentItemsAsync(indexName);
            });
        }

        public async Task<string> CreateContentItem(string contentType, Action<ContentItem> func, bool draft = false)
        {
            // Never generate a fake ContentItemId here as it should be created by the ContentManager.NewAsync() method.
            // Controllers should use the proper sequence so that they call their event handlers.
            // In that case it would skip calling ActivatingAsync, ActivatedAsync, InitializingAsync, InitializedAsync events
            var contentItem = new ContentItem
            {
                ContentType = contentType
            };

            func(contentItem);

            var content = await Client.PostAsJsonAsync("api/content" + (draft ? "?draft=true" : ""), contentItem);

            var response = await content.Content.ReadAsAsync<ContentItem>();

            return response.ContentItemId;
        }

        public async Task DeleteContentItem(string contentItemId) => await Client.DeleteAsync("api/content/" + contentItemId);
    }
}

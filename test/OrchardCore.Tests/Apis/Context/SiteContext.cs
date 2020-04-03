using System;
using System.Net.Http;
using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : IDisposable
    {
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static HttpClient DefaultTenantClient { get; }

        public HttpClient Client { get; private set; }
        public string TenantName { get; private set; }
        public OrchardGraphQLClient GraphQLClient { get; private set; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            DefaultTenantClient = Site.CreateDefaultClient();
        }

        public virtual async Task InitializeAsync(PermissionsContext permissionsContext = null)
        {
            var tenantName = Guid.NewGuid().ToString().Replace("-", "");

            var createModel = new Tenants.ViewModels.CreateApiViewModel
            {
                DatabaseProvider = "Sqlite",
                RecipeName = "Blog",
                Name = tenantName,
                RequestUrlPrefix = tenantName
            };

            var createResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/create", createModel);
            createResult.EnsureSuccessStatusCode();

            var content = await createResult.Content.ReadAsStringAsync();

            var url = new Uri(content.Trim('"'));
            url = new Uri(url.Scheme + "://" + url.Authority + url.LocalPath + "/");

            var setupModel = new Tenants.ViewModels.SetupApiViewModel
            {
                SiteName = "Test Site",
                DatabaseProvider = "Sqlite",
                RecipeName = "Blog",
                UserName = "admin",
                Password = "Password01_",
                Name = tenantName,
                Email = "Nick@Orchard"
            };

            var setupResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/setup", setupModel);
            setupResult.EnsureSuccessStatusCode();

            lock (Site)
            {
                Client = Site.CreateDefaultClient(url);
                TenantName = tenantName;
            }

            if (permissionsContext != null)
            {
                var permissionContextKey = Guid.NewGuid().ToString();
                SiteStartup.PermissionsContexts.TryAdd(permissionContextKey, permissionsContext);
                Client.DefaultRequestHeaders.Add("PermissionsContext", permissionContextKey);
            }

            GraphQLClient = new OrchardGraphQLClient(Client);
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

        public Task DeleteContentItem(string contentItemId)
        {
            return Client.DeleteAsync("api/content/" + contentItemId);
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}

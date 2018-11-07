using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : IDisposable
    {
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static HttpClient DefaultTenantClient; // { get; private set; }

        public HttpClient Client { get; private set; }
        public OrchardGraphQLClient GraphQLClient { get; private set; }

        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            //DefaultTenantClient = Site.CreateClient();
        }

        public virtual async Task InitializeAsync()
        {
            var tenantName = Guid.NewGuid().ToString().Replace("-", "");

            await semaphoreSlim.WaitAsync();
            try
            {
                if (DefaultTenantClient == null)
                {
                    DefaultTenantClient = Site.CreateClient();
                }

                var createModel = new Tenants.ViewModels.CreateApiViewModel
                {
                    DatabaseProvider = "Sqlite",
                    RecipeName = "Blog",
                    Name = tenantName,
                    RequestUrlPrefix = tenantName
                };
                var createResult = await DefaultTenantClient.PostAsJsonAsync("api/tenants/create", createModel);

                createResult.EnsureSuccessStatusCode();

                var x = await createResult.Content.ReadAsStringAsync();

                var url = new Uri(x.Trim('"'));
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

                Client = Site.CreateDefaultClient(url);
                GraphQLClient = new OrchardGraphQLClient(Client);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
        }

        public async Task<string> CreateContentItem(string contentType, Action<ContentItem> func, bool draft = false)
        {
            var contentItem = new ContentItem();
            contentItem.ContentItemId = Guid.NewGuid().ToString();
            contentItem.ContentType = contentType;

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

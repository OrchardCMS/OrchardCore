using System;
using System.Net.Http;
using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext
    {
        private static Task _initialize;
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static OrchardGraphQLClient GraphQLClient { get; }
        public static Task InitializeSiteAsync() => _initialize;
        public static HttpClient Client;

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            Client = Site.CreateClient();
            GraphQLClient = new OrchardGraphQLClient(Client);
            _initialize = InitializeAsync();
        }

        private static async Task InitializeAsync()
        {
            var createModel = new Tenants.ViewModels.CreateApiViewModel
            {
                DatabaseProvider = "Sqlite",
                RecipeName = "Blog",
                Name = "Default"
            };
            var createResult = await Client.PostAsJsonAsync("api/tenants/create", createModel);

            createResult.EnsureSuccessStatusCode();

            var setupModel = new Tenants.ViewModels.SetupApiViewModel
            {
                SiteName = "Test Site",
                DatabaseProvider = "Sqlite",
                RecipeName = "Blog",
                UserName = "admin",
                Password = "Password01_",
                Name = "Default",
                Email = "Nick@Orchard"
            };
            var setupResult = await Client.PostAsJsonAsync("api/tenants/setup", setupModel);

            setupResult.EnsureSuccessStatusCode();
        }

        public static async Task<string> CreateContentItem(string contentType, Action<ContentItem> func)
        {
            var contentItem = new ContentItem();
            contentItem.ContentItemId = Guid.NewGuid().ToString();
            contentItem.ContentItemVersionId = Guid.NewGuid().ToString();
            contentItem.ContentType = contentType;

            func(contentItem);

            var content = await Client.PostAsJsonAsync("api/content", contentItem);
            var response = await content.Content.ReadAsAsync<ContentItem>();

            return response.ContentItemId;
        }

        public static Task DeleteContentItem(string contentItemId) {
            return Client.DeleteAsync("api/content/" + contentItemId);
        }
    }
}

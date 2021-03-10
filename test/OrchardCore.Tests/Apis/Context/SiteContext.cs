using System;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.ContentManagement;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Tests.Apis.Context
{
    public class SiteContext : IDisposable
    {
        private static readonly TablePrefixGenerator TablePrefixGenerator = new TablePrefixGenerator();
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static HttpClient DefaultTenantClient { get; }

        public string RecipeName { get; set; } = "Blog";
        public string DatabaseProvider { get; set; } = "Sqlite";
        public string ConnectionString { get; set; }
        public PermissionsContext PermissionsContext { get; set; }

        public HttpClient Client { get; private set; }
        public string TenantName { get; private set; }
        public OrchardGraphQLClient GraphQLClient { get; private set; }
        public static IShellHost ShellHost { get; private set; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            DefaultTenantClient = Site.CreateDefaultClient();
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        public virtual async Task InitializeAsync()
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
                DatabaseProvider = DatabaseProvider,
                TablePrefix = tablePrefix,
                ConnectionString = ConnectionString,
                RecipeName = RecipeName,
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

            if (PermissionsContext != null)
            {
                var permissionContextKey = Guid.NewGuid().ToString();
                SiteStartup.PermissionsContexts.TryAdd(permissionContextKey, PermissionsContext);
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

        internal static DefaultHttpContext MockHttpContext(string[] userRoles, string username = "Mock User")
        {
            DefaultHttpContext httpContext = new DefaultHttpContext();

            GenericIdentity identity = new GenericIdentity(username.Replace('@', '+'), "");

            Mock<ClaimsPrincipal> mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            if (userRoles?.Length > 0)
            {
                foreach (string role in userRoles)
                {
                    mockPrincipal.Setup(p => p.IsInRole(role)).Returns(true);
                }
            }

            httpContext.User = mockPrincipal.Object;

            return httpContext;
        }

        /// <summary>
        /// Executes the test.
        /// </summary>
        /// <param name="task">The task.</param>
        internal static async Task ExecuteTest<T>(Func<ShellScope, Task> task) where T : SiteContext, new()
        {
            using (T context = new T())
            {
                await context.InitializeAsync();

                // Test
                if (ShellHost.TryGetSettings(context.TenantName, out ShellSettings shellSettings))
                {
                    using (ShellScope shellScope = await ShellHost.GetScopeAsync(shellSettings))
                    {
                        await shellScope.UsingAsync(task);
                    }
                }
            }
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }

    public static class SiteContextExtensions
    {
        public static T WithDatabaseProvider<T>(this T siteContext, string databaseProvider) where T : SiteContext
        {
            siteContext.DatabaseProvider = databaseProvider;
            return siteContext;
        }

        public static T WithConnectionString<T>(this T siteContext, string connectionString) where T : SiteContext
        {
            siteContext.ConnectionString = connectionString;
            return siteContext;
        }

        public static T WithPermissionsContext<T>(this T siteContext, PermissionsContext permissionsContext) where T : SiteContext
        {
            siteContext.PermissionsContext = permissionsContext;
            return siteContext;
        }

        public static T WithRecipe<T>(this T siteContext, string recipeName) where T : SiteContext
        {
            siteContext.RecipeName = recipeName;
            return siteContext;
        }
    }
}

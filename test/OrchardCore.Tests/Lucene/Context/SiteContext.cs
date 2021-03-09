using System;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tests.Lucene
{
    public class SiteContext : IDisposable
    {
        public static OrchardTestFixture<SiteStartup> Site { get; }
        public static HttpClient DefaultTenantClient { get; }
        public string TenantName { get; private set; }
        public static IShellHost ShellHost { get; private set; }

        static SiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            DefaultTenantClient = Site.CreateDefaultClient();
            ShellHost = Site.Services.GetRequiredService<IShellHost>();
        }

        /// <summary>
        /// Initializes the Orchard Site and sets up Diggspace.
        /// </summary>
        /// <param name="permissionsContext">The permissions context.</param>
        public virtual async Task InitializeAsync()
        {
            // Create Tenant
            string tenantName = Guid.NewGuid().ToString("n");

            CreateApiViewModel createModel = new CreateApiViewModel
            {
                DatabaseProvider = "Sqlite",
                RecipeName = "SaaS",
                Name = tenantName,
                RequestUrlPrefix = tenantName
            };

            using (HttpResponseMessage createResult = await DefaultTenantClient.PostAsync("api/tenants/create", new StringContent(JsonConvert.SerializeObject(createModel), Encoding.UTF8, "application/json")))
            {
                createResult.EnsureSuccessStatusCode();
            }

            // Initialize Tenant
            SetupApiViewModel setupModel = new SetupApiViewModel
            {
                SiteName = "Test Site - " + tenantName,
                DatabaseProvider = "Sqlite",
                RecipeName = "SaaS",
                UserName = "admin",
                Password = "Password01_",
                Name = tenantName,
                Email = "admin@orchard.com"
            };

            using (HttpResponseMessage setupResult = await DefaultTenantClient.PostAsync("api/tenants/setup", new StringContent(JsonConvert.SerializeObject(setupModel), Encoding.UTF8, "application/json")))
            {
                setupResult.EnsureSuccessStatusCode();
            }

            TenantName = tenantName;
        }

        protected static DefaultHttpContext MockHttpContext(string[] userRoles, string username = "Mock User")
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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}

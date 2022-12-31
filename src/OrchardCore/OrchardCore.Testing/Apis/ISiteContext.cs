using System;
using System.Net.Http;
using System.Threading.Tasks;
using OrchardCore.Apis.GraphQL.Client;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Testing.Apis
{
    public interface ISiteContext : IDisposable
    {
        static IShellHost ShellHost { get; }

        static HttpClient DefaultTenantClient { get; }

        SiteContextOptions Options { init; get; }

        HttpClient Client { get; }

        string TenantName { get; }

        OrchardGraphQLClient GraphQLClient { get; }

        Task InitializeAsync();

        Task RunRecipeAsync(string recipeName, string recipePath);
    }
}

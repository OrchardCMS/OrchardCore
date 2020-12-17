using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Apis.Context.Attributes;
using Xunit;

namespace OrchardCore.Tests.Apis.Recipes
{
    public class BlogRecipeTests
    {
        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldCreateBlog(string databaseProvider, string connectionString)
        {
            using var context = new SiteContext()
                .WithDatabaseProvider(databaseProvider)
                .WithConnectionString(connectionString);

            // Act
            await context.InitializeAsync();

            var result = await context.Client.GetAsync("/");
            Assert.True(result.IsSuccessStatusCode);
        }
    }
}

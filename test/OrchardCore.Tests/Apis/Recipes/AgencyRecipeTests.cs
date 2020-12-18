using System.Threading.Tasks;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Apis.Context.Attributes;
using Xunit;

namespace OrchardCore.Tests.Apis.Recipes
{
    public class AgencyRecipeTests
    {
        [Theory]
        [SqliteData]
        [SqlServerData]
        [MySqlData]
        [PostgreSqlData]
        public async Task ShouldCreateAgency(string databaseProvider, string connectionString)
        {
            using var context = new AgencyContext()
                .WithDatabaseProvider(databaseProvider)
                .WithConnectionString(connectionString);

            // Act
            await context.InitializeAsync();

            var result = await context.Client.GetAsync("/");
            Assert.True(result.IsSuccessStatusCode);
        }
    }
}

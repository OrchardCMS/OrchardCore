using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Tests.Apis
{
    public class GraphQLSiteSetupTests :
        IClassFixture<InMemorySiteContext>
    {
        private readonly InMemorySiteContext _siteContext;

        public GraphQLSiteSetupTests(
            InMemorySiteContext siteContext)
        {
            _siteContext = siteContext;
        }

        [Fact]
        public async Task ShouldSetSiteupUsingSqlLite()
        {
            var json = @"mutation {
  createSite(site: {
  siteName: 'test',
	AdminUsername: '',
	AdminEmail: '',
	AdminPassword: '',
	DatabaseProvider: 'sqllite',
	RecipeName: 'saas.recipe.json'
  })
}"; 

                var response = await _siteContext.Site.Client.PostAsJsonAsync("graphql", json);

                response.EnsureSuccessStatusCode();

                var z = await response.Content.ReadAsStringAsync();

                Assert.NotNull(z);
        }
    }
}

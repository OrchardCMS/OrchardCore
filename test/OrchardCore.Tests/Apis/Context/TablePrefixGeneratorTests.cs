namespace OrchardCore.Tests.Apis.Context
{
    public class TablePrefixGeneratorTests
    {
        [Fact]
        public async Task TenantPrefixShouldBeUnique()
        {
            var tablePrefixGenerator = new TablePrefixGenerator();
            var prefixes = new HashSet<string>();

            for (var i = 0; i < 200; i++)
            {
                var prefix = await tablePrefixGenerator.GeneratePrefixAsync();
                Assert.DoesNotContain(prefix, prefixes);
                prefixes.Add(prefix);
            }
        }
    }
}

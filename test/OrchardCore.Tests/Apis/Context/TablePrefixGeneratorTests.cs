using System;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Tests.Apis.Context
{
    public class TablePrefixGeneratorTests
	{
        [Fact]
        public async Task TenantPrefixShouldBeUnique()
        {
            var tablePrefixGenerator = new TablePrefixGenerator();
            string previousPrefix = null;
            for(var i = 0; i < 200; i++)
            {
                var prefix = await tablePrefixGenerator.GeneratePrefixAsync();
                Assert.NotEqual(previousPrefix, prefix);
                previousPrefix = prefix;
            }
        }
    }
}

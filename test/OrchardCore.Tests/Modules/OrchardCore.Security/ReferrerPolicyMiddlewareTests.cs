using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class ReferrerPolicyMiddlewareTests
    {
        public static IEnumerable<object[]> Policies =>
            new List<object[]>
            {
                new object[] { ReferrerPolicy.NoReferrer },
                new object[] { ReferrerPolicy.NoReferrerWhenDowngrade },
                new object[] { ReferrerPolicy.Origin },
                new object[] { ReferrerPolicy.OriginWhenCrossOrigin },
                new object[] { ReferrerPolicy.SameOrigin },
                new object[] { ReferrerPolicy.StrictOrigin },
                new object[] { ReferrerPolicy.StrictOriginWhenCrossOrigin },
                new object[] { ReferrerPolicy.UnsafeUrl }
            };

        [Theory]
        [MemberData(nameof(Policies))]
        public void UseReferrerPolicy_ShouldAddReferrerPolicyHeaderWithProperValue(ReferrerPolicy policy)
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseReferrerPolicy(policy);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeader.ReferrerPolicy));
            Assert.Equal(policy, context.Response.Headers[SecurityHeader.ReferrerPolicy]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

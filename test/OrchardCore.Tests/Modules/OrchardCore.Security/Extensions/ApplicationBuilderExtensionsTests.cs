using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class ApplicationBuilderExtensionsTests
    {
        public static IEnumerable<object[]> Policies =>
            new List<object[]>
            {
                new object[] { ReferrerPolicyOptions.NoReferrer },
                new object[] { ReferrerPolicyOptions.NoReferrerWhenDowngrade },
                new object[] { ReferrerPolicyOptions.Origin },
                new object[] { ReferrerPolicyOptions.OriginWhenCrossOrigin },
                new object[] { ReferrerPolicyOptions.SameOrigin },
                new object[] { ReferrerPolicyOptions.StrictOrigin },
                new object[] { ReferrerPolicyOptions.StrictOriginWhenCrossOrigin },
                new object[] { ReferrerPolicyOptions.UnsafeUrl }
            };

        [Theory]
        [MemberData(nameof(Policies))]
        public void UseReferrerPolicy_ShouldAddReferrerPolicyHeaderWithProperValue(string policy)
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

        [Fact]
        public void UseSecurityHeadersWithDefaultHeaders()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders();

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeader.ReferrerPolicy]);
        }

        [Fact]
        public void UseSecurityHeadersWithOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new SecurityHeadersOptions
            {
                ReferrerPolicy = ReferrerPolicyOptions.SameOrigin
            };
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ReferrerPolicyOptions.SameOrigin, context.Response.Headers[SecurityHeader.ReferrerPolicy]);
        }

        [Fact]
        public void UseSecurityHeadersWithConfigureOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(options => options.ReferrerPolicy = ReferrerPolicyOptions.SameOrigin);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ReferrerPolicyOptions.SameOrigin, context.Response.Headers[SecurityHeader.ReferrerPolicy]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class ApplicationBuilderExtensionsTests
    {
        public static IEnumerable<object[]> ReferrerPolicies =>
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

        public static IEnumerable<object[]> FrameOptions =>
            new List<object[]>
            {
                new object[] { Security.FrameOptions.Deny },
                new object[] { Security.FrameOptions.SameOrigin }
            };

        [Theory]
        [MemberData(nameof(ReferrerPolicies))]
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
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.ReferrerPolicy));
            Assert.Equal(policy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        [Theory]
        [MemberData(nameof(FrameOptions))]
        public void UseFrameOptions_ShouldAddXFrameOptionsHeaderWithProperValue(string option)
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseFrameOptions(option);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
            Assert.Equal(option, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
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
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        [Fact]
        public void UseSecurityHeadersWithConfigureBuilder()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(config => config
                .AddReferrerPolicy(ReferrerPolicyOptions.Origin)
                .Build()
            );

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ReferrerPolicyOptions.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

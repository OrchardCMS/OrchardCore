using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Options;
using OrchardCore.Security.Services;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class FrameOptionsHeaderPolicyProviderTests
    {
        [Fact]
        public void AddHeader()
        {
            // Arrange
            var provider = new FrameOptionsHeaderPolicyProvider { Options = new SecurityHeadersOptions()};
            var context = new DefaultHttpContext();

            // Act
            provider.ApplyPolicy(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
        }

        [Fact]
        public void OverrideHeader_FrameAncestorsDirectiveExists()
        {
            // Arrange
            var provider = new FrameOptionsHeaderPolicyProvider();
            var context = new DefaultHttpContext();
            context.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy] = $"{ContentSecurityPolicyValue.FrameAncestors} {ContentSecurityPolicyOriginValue.Self}";

            // Act
            provider.ApplyPolicy(context);

            // Assert
            Assert.False(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
        }
    }
}

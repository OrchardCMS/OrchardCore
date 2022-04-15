using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class SecurityHeadersBuilderExtensionsTests
    {
        [Fact]
        public void AddReferrerPolicyWithFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddReferrerPolicy()
                .WithSameOrigin();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(ReferrerPolicyOptions.SameOrigin, settings.ReferrerPolicy);
        }

        [Fact]
        public void AddFrameOptionsWithFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddFrameOptions()
                .WithSameOrigin();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(FrameOptions.SameOrigin, settings.FrameOptions);
        }

        [Fact]
        public void AddSecurityHeadersWithFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddReferrerPolicy()
                    .WithSameOrigin()
                .AddFrameOptions()
                    .WithSameOrigin();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(ReferrerPolicyOptions.SameOrigin, settings.ReferrerPolicy);
            Assert.Equal(FrameOptions.SameOrigin, settings.FrameOptions);
        }
    }
}

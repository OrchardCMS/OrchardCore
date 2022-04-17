using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class SecurityHeadersBuilderExtensionsTests
    {
        [Fact]
        public void AddContentTypeOptionsWithFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddContentTypeOptions()
                .WithNoSniff();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, settings.ContentTypeOptions);
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
            Assert.Equal(FrameOptionsValue.SameOrigin, settings.FrameOptions);
        }

        [Fact]
        public void AddPermissionsPolicyWithFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddPermissionsPolicy()
                .WithCamera()
                .WithMicrophone();

            // Assert
            Assert.NotNull(settings);
            Assert.Contains(PermissionsPolicyValue.Camera, settings.PermissionsPolicy);
            Assert.Contains(PermissionsPolicyValue.Microphone, settings.PermissionsPolicy);
        }

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
            Assert.Equal(ReferrerPolicyValue.SameOrigin, settings.ReferrerPolicy);
        }

        [Fact]
        public void AddSecurityHeadersWithFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddContentTypeOptions()
                    .WithNoSniff()
                .AddFrameOptions()
                    .WithSameOrigin()
                .AddPermissionsPolicy()
                    .WithCamera()
                    .WithMicrophone()
                    .Build()
                .AddReferrerPolicy()
                    .WithSameOrigin();

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, settings.ContentTypeOptions);
            Assert.Equal(FrameOptionsValue.SameOrigin, settings.FrameOptions);
            Assert.Contains(PermissionsPolicyValue.Camera, settings.PermissionsPolicy);
            Assert.Contains(PermissionsPolicyValue.Microphone, settings.PermissionsPolicy);
            Assert.Equal(ReferrerPolicyValue.SameOrigin, settings.ReferrerPolicy);
        }
    }
}

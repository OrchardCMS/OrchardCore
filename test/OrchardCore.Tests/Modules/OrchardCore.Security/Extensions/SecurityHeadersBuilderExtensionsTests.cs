using System;
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
            builder.AddContentTypeOptions();

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
        public void AddPermissionsPolicyWithOptionsAndFluentAPIsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddPermissionsPolicy(options =>
                {
                    options
                        .AllowCamera()
                        .AllowMicrophone()
                        .WithSelfOrigin();
                });

            // Assert
            Assert.NotNull(settings.PermissionsPolicy);
            Assert.Equal(2, settings.PermissionsPolicy.Count);
            Assert.Contains(PermissionsPolicyValue.Camera, settings.PermissionsPolicy);
            Assert.Contains(PermissionsPolicyValue.Microphone, settings.PermissionsPolicy);
            Assert.Equal(PermissionsPolicyOriginValue.Self, settings.PermissionsPolicyOrigin);
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
        public void AddStrictTransportSecurityWithOptionsConfiguration()
        {
            // Arrange
            var settings = new SecuritySettings();
            var builder = new SecurityHeadersBuilder(settings);

            // Act
            builder
                .AddStrictTransportSecurity(options =>
                {
                    options.MaxAge = TimeSpan.FromMinutes(1);
                    options.IncludeSubDomains = false;
                    options.Preload = true;
                });

            // Assert
            Assert.NotNull(settings.StrictTransportSecurity);
            Assert.Equal(60, settings.StrictTransportSecurity.MaxAge.TotalSeconds);
            Assert.False(settings.StrictTransportSecurity.IncludeSubDomains);
            Assert.True(settings.StrictTransportSecurity.Preload);
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
                .AddFrameOptions()
                    .WithSameOrigin()
                .AddPermissionsPolicy(options => options.AllowCamera().AllowMicrophone())
                .AddReferrerPolicy()
                    .WithSameOrigin()
                .AddStrictTransportSecurity(options =>
                {
                    options.MaxAge = TimeSpan.FromMinutes(1);
                    options.IncludeSubDomains = false;
                });

            // Assert
            Assert.NotNull(settings);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, settings.ContentTypeOptions);
            Assert.Equal(FrameOptionsValue.SameOrigin, settings.FrameOptions);
            Assert.Equal(PermissionsPolicyOriginValue.Self, settings.PermissionsPolicyOrigin);
            Assert.Contains(PermissionsPolicyValue.Camera, settings.PermissionsPolicy);
            Assert.Contains(PermissionsPolicyValue.Microphone, settings.PermissionsPolicy);
            Assert.Equal(ReferrerPolicyValue.SameOrigin, settings.ReferrerPolicy);
            Assert.Equal(60, settings.StrictTransportSecurity.MaxAge.TotalSeconds);
            Assert.False(settings.StrictTransportSecurity.IncludeSubDomains);
            Assert.False(settings.StrictTransportSecurity.Preload);
        }
    }
}

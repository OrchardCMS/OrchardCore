using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class PermissionsPolicyApplicationBuilderExtensionsTests
    {
        [Fact]
        public void UsePermissionsPolicyWithOptionsInjectDefaultHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UsePermissionsPolicy();

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
        }

        [Fact]
        public void UsePermissionsPolicyWithOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new PermissionsPolicyOptions();
            options.Camera.Origin = PermissionsPolicyOriginValue.Self;
            options.Speaker.Origin = PermissionsPolicyOriginValue.Any;

            var applicationBuilder = CreateApplicationBuilder();


            // Act
            applicationBuilder.UsePermissionsPolicy(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=self, encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=*, sync-xhr=(), usb=(), vibrate=(), vr=()", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
        }

        [Fact]
        public void UsePermissionsPolicyWithBuilderConfiguration()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UsePermissionsPolicy(config =>
            {
                config
                    .AllowCamera(PermissionsPolicyOriginValue.Self)
                    .AllowSpeaker(PermissionsPolicyOriginValue.Any);
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=self, encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=*, sync-xhr=(), usb=(), vibrate=(), vr=()", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

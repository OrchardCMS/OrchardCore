using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class FrameOptionsBuilderExtensionsTests
    {
        [Fact]
        public void UseFrameOptionsWithoutOptionsShouldInjectDefaultHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseFrameOptions();

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(SecurityHeaderDefaults.FrameOptions, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
        }

        [Fact]
        public void UseFrameOptionsWithOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new FrameOptionsOptions { Value = FrameOptionsValue.SameOrigin };
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseFrameOptions(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(FrameOptionsValue.SameOrigin, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

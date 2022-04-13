using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Tests.Modules
{
    public class PoweredByMiddlewareTests
    {
        private const string PoweredByHeaderName = "X-Powered-By";

        [Fact]
        public void UsePoweredByOrchardCore_ShouldInjectPoweredByHeaderWithOrchardCoreValue()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UsePoweredByOrchardCore();

            applicationBuilder
                .Build()
                .Invoke(context);

            Assert.True(context.Response.Headers.ContainsKey(PoweredByHeaderName));
            Assert.Equal("Orchard Core", context.Response.Headers[PoweredByHeaderName]);
        }

        [Fact]
        public void UsePoweredBy_ShouldInjectPoweredByHeaderWithCustomValue()
        {
            // Arrange
            var poweredByValue = "Orchard Core Contrib";
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UsePoweredBy(poweredByValue);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(PoweredByHeaderName));
            Assert.Equal(poweredByValue, context.Response.Headers[PoweredByHeaderName]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

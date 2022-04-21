using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class StrictTransportSecurityApplicationBuilderExtensionsTests
    {
        [Fact]
        public void UseStrictTransportSecurityWithoutOptionsShouldInjectDefaultHeader()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseStrictTransportSecurity();

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("max-age=31536000; includeSubDomains", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        [Fact]
        public void UseStrictTransportSecurityWithOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new StrictTransportSecurityOptions
            {
                MaxAge = TimeSpan.FromHours(1),
                IncludeSubDomains = false,
                Preload = true
            };
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseStrictTransportSecurity(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("max-age=3600; preload", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        [Fact]
        public void UseStrictTransportSecurityWithBuilderConfiguration()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseStrictTransportSecurity(config =>
            {
                config
                    .WithMaxAge(TimeSpan.FromSeconds(3600))
                    .Preload(true);
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("max-age=3600; includeSubDomains; preload", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using OrchardCore.Media;
using OrchardCore.Media.Processing;
using OrchardCore.Settings;
using SixLabors.ImageSharp.Web.Processors;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class MediaTokenTests
    {
        private readonly MediaTokenSettings _mediaTokenSettings;
        public MediaTokenTests()
        {
            var rng = RandomNumberGenerator.Create();
            _mediaTokenSettings = new MediaTokenSettings();
            _mediaTokenSettings.HashKey = new byte[64];
            rng.GetBytes(_mediaTokenSettings.HashKey);
        }

        [Fact]
        public void ShouldAddToken()
        {
            var serviceProvider = CreateServiceProvider();
            var mediaTokenService = serviceProvider.GetRequiredService<IMediaTokenService>();
            var path = "/media/blog.jpg?width=100&height=100";

            var tokenizedPath = mediaTokenService.AddTokenToPath(path);

            Assert.True(tokenizedPath.Contains("token", StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData("/media/blog.jpg?v=version")]
        [InlineData("/media/blog.jpg")]
        public void ShouldNotAddTokens(string path)
        {
            var serviceProvider = CreateServiceProvider();
            var mediaTokenService = serviceProvider.GetRequiredService<IMediaTokenService>();

            var tokenizedPath = mediaTokenService.AddTokenToPath(path);
            Assert.Equal(path, tokenizedPath);
        }

        [Fact]
        public void ShouldGenerateConsistentToken()
        {
            var sp1 = CreateServiceProvider();
            var sp2 = CreateServiceProvider();
            var mts1 = sp1.GetRequiredService<IMediaTokenService>();
            var mts2 = sp2.GetRequiredService<IMediaTokenService>();
            var path = "/media/blog.jpg?width=100&height=100";

            var tokenizedPath1 = mts1.AddTokenToPath(path);
            var tokenizedPath2 = mts2.AddTokenToPath(path);
            Assert.Equal(tokenizedPath1, tokenizedPath2);
        }

        private IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            var mockSiteService = Mock.Of<ISiteService>(ss =>
                ss.GetSiteSettingsAsync() == Task.FromResult(
                    Mock.Of<ISite>(s => s.Properties == JObject.FromObject(new { MediaTokenSettings = _mediaTokenSettings }))
                )
            );

            services.AddMemoryCache();

            services.AddSingleton<ISiteService>(mockSiteService);
            services.AddSingleton<IImageWebProcessor, TokenCommandProcessor>();
            services.AddSingleton<IImageWebProcessor, TokenCommandProcessor>();
            services.AddSingleton<IImageWebProcessor, ResizeWebProcessor>();
            services.AddScoped<IMediaTokenService, MediaTokenService>();
            services.AddTransient<IConfigureOptions<MediaTokenOptions>, MediaTokenOptionsConfiguration>();

            var serviceProvider = services.BuildServiceProvider();

            return serviceProvider;
        }
    }
}

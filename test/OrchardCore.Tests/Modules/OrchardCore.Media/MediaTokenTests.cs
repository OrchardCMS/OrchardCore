using OrchardCore.Media;
using OrchardCore.Media.Processing;
using OrchardCore.Settings;
using SixLabors.ImageSharp.Web.Processors;

namespace OrchardCore.Tests.Modules.OrchardCore.Media
{
    public class MediaTokenTests
    {
        private readonly MediaTokenSettings _mediaTokenSettings;

        // use static value for repeatable tests
        private readonly byte[] _hashKey =
        {
            88, 204, 124, 2, 54, 72, 44, 8, 207, 85, 88, 9, 245, 129, 200, 41, 13, 169, 54, 213, 202, 118, 197, 9, 26,
            20, 108, 197, 123, 168, 140, 79, 32, 184, 144, 72, 254, 201, 91, 73, 230, 56, 190, 40, 11, 21, 229, 94, 118,
            120, 84, 75, 174, 181, 168, 94, 30, 191, 169, 41, 222, 12, 239, 106
        };

        public MediaTokenTests()
        {
            _mediaTokenSettings = new MediaTokenSettings
            {
                HashKey = _hashKey,
            };
        }

        [Theory]
        [InlineData("/media/blog.jpg?width=100&height=100", "/media/blog.jpg?width=100&height=100&token=eVmmj09NoysPASEiuhCuUHJR%2BSrUtSafBo738SuL2eU%3D")]
        [InlineData("/media/blog.jpg?width=100&height=100&foo=bar", "/media/blog.jpg?width=100&height=100&token=eVmmj09NoysPASEiuhCuUHJR%2BSrUtSafBo738SuL2eU%3D&foo=bar")]
        public void ShouldAddToken(string path, string expected)
        {
            var serviceProvider = CreateServiceProvider();
            var mediaTokenService = serviceProvider.GetRequiredService<IMediaTokenService>();

            // Make sure we also hit cache.
            for (var i = 0; i < 2; ++i)
            {
                var tokenizedPath = mediaTokenService.AddTokenToPath(path);
                Assert.Equal(expected, tokenizedPath);
            }
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

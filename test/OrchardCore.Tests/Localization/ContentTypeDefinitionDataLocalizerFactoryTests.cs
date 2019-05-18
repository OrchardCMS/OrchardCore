using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Records;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class ContentTypeDefinitionDataLocalizerFactoryTests
    {
        private static readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private static readonly Mock<IOptions<RequestLocalizationOptions>> _requestLocalizationOptions;

        static ContentTypeDefinitionDataLocalizerFactoryTests()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _httpContextAccessor.Setup(c => c.HttpContext).Returns(GetHttpContext);

            _requestLocalizationOptions = new Mock<IOptions<RequestLocalizationOptions>>();
            _requestLocalizationOptions.Setup(o => o.Value).Returns(new RequestLocalizationOptions());
        }

        [Fact]
        public void CreateLocalizerFactoryIfAllParametersAreNotNull()
        {
            IDataLocalizerFactory localizerFactory = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                localizerFactory = new ContentTypeDefinitionDataLocalizerFactory(null, null, null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                localizerFactory = new ContentTypeDefinitionDataLocalizerFactory(_httpContextAccessor.Object, null, null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                localizerFactory = new ContentTypeDefinitionDataLocalizerFactory(null, _requestLocalizationOptions.Object, null);
            });

            Assert.Throws<ArgumentNullException>(() =>
            {
                localizerFactory = new ContentTypeDefinitionDataLocalizerFactory(null, null, NullLoggerFactory.Instance);
            });

            localizerFactory = CreateLocalizerFactory();
            Assert.NotNull(localizerFactory);
        }

        [Fact]
        public void CreateLocalizerGetsCachedVersionIfTheLocalizerCreatedBefore()
        {
            var culture = "ar-YE";
            var localizerFactory = CreateLocalizerFactory();

            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var localizer1 = localizerFactory.Create();
            var localizer2 = localizerFactory.Create();

            Assert.Same(localizer1, localizer2);
        }

        internal static IDataLocalizerFactory CreateLocalizerFactory(bool fallBackToParentUICulture = true)
        {
            _requestLocalizationOptions.Object.Value.FallBackToParentUICultures = fallBackToParentUICulture;

            return new ContentTypeDefinitionDataLocalizerFactory(
                _httpContextAccessor.Object,
                _requestLocalizationOptions.Object,
                NullLoggerFactory.Instance);
        }

        private static HttpContext GetHttpContext()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(IContentDefinitionStore)))
                .Returns(new InMemoryContentDefinitionStore());

            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);

            return httpContext.Object;
        }

        private class InMemoryContentDefinitionStore : IContentDefinitionStore
        {
            public Task<ContentDefinitionRecord> LoadContentDefinitionAsync()
            {
                var record = new ContentDefinitionRecord
                {
                    ContentTypeDefinitionRecords = new List<ContentTypeDefinitionRecord>
                    {
                        new ContentTypeDefinitionRecord{ Name = "Menu", DisplayName = new LocalizedObject("Menu")},
                        new ContentTypeDefinitionRecord{ Name = "Blog", DisplayName = ConvertToLocalizedObject("Blog",
                            new Dictionary<string, string>{
                                { "fr", "Blog" },
                                { "ar", "مدونة" }
                            })},
                        new ContentTypeDefinitionRecord{ Name = "Shirt", DisplayName = ConvertToLocalizedObject("Shirt",
                            new Dictionary<string, string>{
                                { "fr", "Chemise" },
                                { "ar", "قميص" },
                                { "ar-YE", "شميز" }
                            })}
                    }
                };

                return Task.FromResult(record);
            }

            public Task SaveContentDefinitionAsync(ContentDefinitionRecord contentDefinitionRecord)
            {
                throw new NotImplementedException();
            }

            private LocalizedObject ConvertToLocalizedObject(string @default, IDictionary<string, string> keyValuePairs)
            {
                var localizedObject = new LocalizedObject(@default);

                foreach (var pair in keyValuePairs)
                {
                    localizedObject.Add(pair.Key, pair.Value);
                }

                return localizedObject;
            }
        }
    }
}
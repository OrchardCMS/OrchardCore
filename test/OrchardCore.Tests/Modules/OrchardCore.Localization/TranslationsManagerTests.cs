using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using OrchardCore.DataLocalization.Models;
using OrchardCore.DataLocalization.Services;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Localization
{
    public class TranslationsManagerTests
    {
        private readonly Mock<ITranslationsManager>  _translationsManager;

        public TranslationsManagerTests()
        {
            _translationsManager = new Mock<ITranslationsManager>();
            _translationsManager.Setup(tm => tm.GetTranslationsDocumentAsync())
                .ReturnsAsync(() => GetDefaultTranslationsDocument());
        }

        [Fact]
        public async Task GetTranslationsDocument()
        {
            var translationsDocument = await _translationsManager.Object.GetTranslationsDocumentAsync();

            Assert.NotNull(translationsDocument);
            Assert.Equal(3, translationsDocument.Translations.Count);
            var spanishTranslations = translationsDocument.Translations["es"];
            var frenchTranslations = translationsDocument.Translations["fr"];
            Assert.Equal("Artículo1", spanishTranslations.First().Value);
            Assert.Equal("Article1", frenchTranslations.First().Value);
            Assert.Equal("Nombre", spanishTranslations.Last().Value);
            Assert.Equal("Nome", frenchTranslations.Last().Value);
        }

        [Fact]
        public async Task GetTranslationsDocument_GroupByContext()
        {
            var translationsDocument = await _translationsManager.Object.GetTranslationsDocumentAsync();
            var groups = translationsDocument.Translations
                .SelectMany(t => t.Value)
                .GroupBy(t => t.Context);

            Assert.Equal(2, groups.Count());
            Assert.StartsWith("Content Type", groups.First().Key);
            Assert.Equal(8, groups.First().Count());
            Assert.StartsWith("Content Field", groups.Last().Key);
            Assert.Equal(3, groups.Last().Count());
        }

        private static TranslationsDocument GetDefaultTranslationsDocument()
        {
            var document = new TranslationsDocument();
            document.Translations.Add("en", new List<Translation> {
                 new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article1",
                    Value = "Article1",
                },
                  new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article2",
                    Value = "Article2",
                },
                   new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article3",
                    Value = "",
                },
                   new Translation {
                    Context = "Content Field",
                    Key = "Content Field:Name",
                    Value = "Name",
                }
            });
            document.Translations.Add("es", new List<Translation> {
                 new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article1",
                    Value = "Artículo1",
                },
                   new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article3",
                    Value = "Artículo3",
                },
                   new Translation {
                    Context = "Content Field",
                    Key = "Content Field:Name",
                    Value = "Nombre",
                }
            });
            document.Translations.Add("fr", new List<Translation> {
                 new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article1",
                    Value = "Article1",
                },
                  new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article2",
                    Value = "Article2",
                },
                   new Translation {
                    Context = "Content Type",
                    Key = "Content Type:Article3",
                    Value = "",
                },
                   new Translation {
                    Context = "Content Field",
                    Key = "Content Field:Name",
                    Value = "Nome",
                }
            });

            return document;
        }
    }
}

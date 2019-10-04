using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public class TranslationsManager
    {
        public async Task<TranslationsDocument> GetTranslationsDocumentAsync()
        {
            var document = new TranslationsDocument();
            // TODO: Fetch the translations from the database
            document.Translations.AddRange(GetTranslations());
            await Task.CompletedTask;

            return document;
        }
        
        public async Task UpdateTranslationAsync(string name, Translation template)
        {
            // TODO: update the translations into the database
            await Task.CompletedTask;
        }

        private IEnumerable<Translation> GetTranslations()
        {
            return new List<Translation>{
                new Translation {
                    Key = "Article1",
                    Values = new Dictionary<string, string> {
                        { "en", "Article1" },
                        { "fr", "Article1" },
                        { "es", "Artículo1" }
                    }
                },
                new Translation {
                    Key = "Article2",
                    Values = new Dictionary<string, string> {
                        { "en", "Article2" },
                        { "fr", "Article2" },
                        { "es", "" }
                    }
                },
                new Translation {
                    Key = "Article3",
                    Values = new Dictionary<string, string> {
                        { "en", "" },
                        { "fr", "" },
                        { "es", "Artículo3" }
                    }
                },
            };
        }
    }
}
using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public class TranslationsManager : ITranslationsManager
    {
        public async Task<TranslationsDocument> GetTranslationsDocumentAsync()
        {
            // TODO: Fetch the translations from the database
            var document = new TranslationsDocument();
            await Task.CompletedTask;

            return document;
        }
        
        public async Task UpdateTranslationAsync(string name, Translation template)
        {
            // TODO: update the translations into the database
            await Task.CompletedTask;
        }
    }
}

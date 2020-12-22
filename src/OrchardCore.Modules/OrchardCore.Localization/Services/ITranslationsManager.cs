using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public interface ITranslationsManager
    {
        Task<TranslationsDocument> LoadTranslationsDocumentAsync();

        Task<TranslationsDocument> GetTranslationsDocumentAsync();

        Task RemoveTranslationAsync(string name);

        Task UpdateTranslationAsync(string name, Translation translation);
    }
}

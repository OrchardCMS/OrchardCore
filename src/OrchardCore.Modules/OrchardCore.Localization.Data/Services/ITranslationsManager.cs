using System.Threading.Tasks;
using OrchardCore.Localization.Data.Models;

namespace OrchardCore.Localization.Data.Services
{
    public interface ITranslationsManager
    {
        Task<TranslationsDocument> LoadTranslationsDocumentAsync();

        Task<TranslationsDocument> GetTranslationsDocumentAsync();

        Task RemoveTranslationAsync(string name);

        Task UpdateTranslationAsync(string name, Translation translation);
    }
}

using System.Threading.Tasks;
using OrchardCore.Localization.Dynamic.Models;

namespace OrchardCore.Localization.Dynamic.Services
{
    public interface ITranslationsManager
    {
        Task<TranslationsDocument> LoadTranslationsDocumentAsync();

        Task<TranslationsDocument> GetTranslationsDocumentAsync();

        Task RemoveTranslationAsync(string name);

        Task UpdateTranslationAsync(string name, Translation translation);
    }
}

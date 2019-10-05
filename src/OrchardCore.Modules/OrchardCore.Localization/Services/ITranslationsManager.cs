using System.Threading.Tasks;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.Services
{
    public interface ITranslationsManager
    {
        Task<TranslationsDocument> GetTranslationsDocumentAsync();
        
        Task UpdateTranslationAsync(string name, Translation template);
    }
}
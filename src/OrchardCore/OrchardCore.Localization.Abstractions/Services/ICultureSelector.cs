using System.Threading.Tasks;

namespace OrchardCore.Localization.Services
{
    /// <summary>
    /// Provides the culture for the current request.
    /// </summary>
    public interface ICultureSelector
    {
        Task<CultureSelectorResult> GetCultureAsync();
    }
}

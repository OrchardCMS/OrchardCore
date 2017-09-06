using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// When implemented, defines a way to provide specific themes for the current request.
    /// The results of all implementations are then ordered by priority, and the result
    /// with the highest priority is selected as the current theme.
    /// </summary>
    public interface IThemeSelector
    {
        /// <summary>
        /// Returns a <see cref="ThemeSelectorResult"/> representing a specific theme
        /// and a priority order. The highest priority value will be used.
        /// </summary>
        /// <returns></returns>
        Task<ThemeSelectorResult> GetThemeAsync();
    }
}

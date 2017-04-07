using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
namespace Orchard.DisplayManagement.Theming
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
        /// <summary>
        /// if CanSet is true,this will be used to set the theme
        /// </summary>
        /// <param name="themeName"></param>
        void SetTheme(string themeName);
        /// <summary>
        /// it is a flag to tell the stysem,the selector either can be seted or not.
        /// </summary>
        bool CanSet { get; }
        /// <summary>
        /// this will tell the system,the theme.txt need contains this Tag
        /// </summary>
        string Tag { get; }
        /// <summary>
        /// a Display name,that will be used in Orchard.Themes.AdminMenu to build a menu
        /// </summary>
        LocalizedString DisplayName { get; }
        /// <summary>
        /// this like a parameter to build a menu
        /// </summary>
        string Name { get; }
    }
}

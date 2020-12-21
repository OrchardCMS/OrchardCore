using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    /// <summary>
    /// Implementations are able to provide a default context to the shortcode service.
    /// </summary>
    public interface IShortcodeContextProvider
    {
        void Contextualize(Context context);
    }
}

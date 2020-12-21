using System;
using Shortcodes;

namespace OrchardCore.Shortcodes.Services
{
    /// <summary>
    /// Provides a default context to the shortcode processor.
    /// </summary>
    public class DefaultShortcodeContextProvider : IShortcodeContextProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultShortcodeContextProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Contextualize(Context context)
        {
            context["ServiceProvider"] = _serviceProvider;
        }
    }
}

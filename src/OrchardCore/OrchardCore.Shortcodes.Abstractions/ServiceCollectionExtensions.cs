using System;
using Microsoft.Extensions.DependencyInjection;
using Shortcodes;

namespace OrchardCore.Shortcodes
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddShortcode<T>(this IServiceCollection services, string name) where T : class, IShortcodeProvider =>
            services.AddShortcode(name, null);

        public static IServiceCollection AddShortcode<T>(this IServiceCollection services, string name, Action<ShortcodeOption> describe) where T : class, IShortcodeProvider
        {
            services.Configure<ShortcodeOptions>(options => options.AddShortcode(name, describe));
            services.AddScoped<IShortcodeProvider, T>();

            return services;
        }

        public static IServiceCollection AddShortcode(this IServiceCollection services, string name, ShortcodeDelegate shortcode) =>
            services.Configure<ShortcodeOptions>(options => options.AddShortcodeDelegate(name, shortcode, null));

        public static IServiceCollection AddShortcode(this IServiceCollection services, string name, ShortcodeDelegate shortcode, Action<ShortcodeOption> describe)
        {
            services.Configure<ShortcodeOptions>(options => options.AddShortcodeDelegate(name, shortcode, describe));

            return services;
        }
    }
}

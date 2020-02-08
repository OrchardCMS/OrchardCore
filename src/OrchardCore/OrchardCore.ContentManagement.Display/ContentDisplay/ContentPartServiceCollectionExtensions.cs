using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ContentPartServiceCollectionExtensions
    {
        /// <summary>
        /// Register a display driver for use with a content part and all display modes and editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder UseDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.UseDisplayDriver(typeof(TContentPartDisplayDriver));
        }

        /// <summary>
        /// Register a display driver for use with a content part and all display modes and editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder UseDisplayDriver(this ContentPartOptionBuilder builder, Type partDisplayDriverType)
        {
            return builder.ForDisplayMode(partDisplayDriverType)
                .ForEditor(partDisplayDriverType);
        }

        /// <summary>
        /// Removes a display driver from all display modes and editors.
        /// </summary>
        public static ContentPartOptionBuilder RemoveDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.RemoveDisplayDriver(typeof(TContentPartDisplayDriver));
        }

        /// <summary>
        /// Removes a display driver from all display modes and editors.
        /// </summary>
        public static ContentPartOptionBuilder RemoveDisplayDriver(this ContentPartOptionBuilder builder, Type partDisplayDriverType)
        {
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.RemoveContentPartDisplayDriver(builder.ContentPartType, partDisplayDriverType);
            });

            builder.Services.RemoveAll(partDisplayDriverType);

            return builder;
        }

        /// <summary>
        /// Registers a display driver for use with all display modes.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplayMode<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.ForDisplayMode(typeof(TContentPartDisplayDriver));
        }

        /// <summary>
        /// Registers a display driver for use with a specific display mode.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplayMode(this ContentPartOptionBuilder builder, Type partDisplayDriverType)
        {
            return builder.ForDisplayMode(partDisplayDriverType, d => true);
        }

        /// <summary>
        /// Registers a display driver for use with a specific display mode.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplayMode<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder, Func<string, bool> predicate)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.ForDisplayMode(typeof(TContentPartDisplayDriver), predicate);
        }

        /// <summary>
        /// Registers a display driver for use with a specific display mode.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplayMode(this ContentPartOptionBuilder builder, Type partDisplayDriverType, Func<string, bool> predicate)
        {
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentPartDisplayMode(builder.ContentPartType, partDisplayDriverType, predicate);
            });

            builder.Services.TryAddScoped(partDisplayDriverType);

            return builder;
        }

        /// <summary>
        /// Registers a display driver for use with all editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForEditor<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.ForEditor(typeof(TContentPartDisplayDriver));
        }

        /// <summary>
        /// Registers a display driver for use with a specific editor.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForEditor(this ContentPartOptionBuilder builder, Type partDisplayDriverType)
        {
            return builder.ForEditor(partDisplayDriverType, d => true);
        }

        /// <summary>
        /// Registers a display driver for use with a specific editor.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForEditor<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder, Func<string, bool> predicate)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.ForEditor(typeof(TContentPartDisplayDriver), predicate);
        }

        /// <summary>
        /// Registers a display driver for use with a specific editor.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForEditor(this ContentPartOptionBuilder builder, Type partDisplayDriverType, Func<string, bool> predicate)
        {
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentPartEditor(builder.ContentPartType, partDisplayDriverType, predicate);
            });

            builder.Services.TryAddScoped(partDisplayDriverType);

            return builder;
        }
    }
}

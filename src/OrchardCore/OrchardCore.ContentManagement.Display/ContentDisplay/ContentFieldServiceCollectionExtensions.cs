using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ContentFieldServiceCollectionExtensions
    {
        /// <summary>
        /// Register a display driver for use with a content field and all display modes and editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder UseDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.UseDisplayDriver(typeof(TContentFieldDisplayDriver));
        }

        /// <summary>
        /// Register a display driver for use with a content field and all display modes and editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder UseDisplayDriver(this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType)
        {
            return builder.ForDisplayMode(fieldDisplayDriverType)
                .ForEditor(fieldDisplayDriverType);
        }

        /// <summary>
        /// Register a display driver for use with a content field and all display modes and editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder UseDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder, Func<string, bool> predicate)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForDisplayMode(typeof(TContentFieldDisplayDriver), predicate)
                .ForEditor(typeof(TContentFieldDisplayDriver), predicate);
        }

        /// <summary>
        /// Removes a display driver from all editors and display modes.
        /// </summary>
        public static ContentFieldOptionBuilder RemoveDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.RemoveDisplayDriver(typeof(TContentFieldDisplayDriver));
        }

        /// <summary>
        /// Removes a display driver from all editors and display modes.
        /// </summary>
        public static ContentFieldOptionBuilder RemoveDisplayDriver(this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType)
        {
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.RemoveContentFieldDisplayDriver(builder.ContentFieldType, fieldDisplayDriverType);
            });

            builder.Services.RemoveAll(fieldDisplayDriverType);

            return builder;
        }

        /// <summary>
        /// Registers a display driver for use with all display modes.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForDisplayMode<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForDisplayMode(typeof(TContentFieldDisplayDriver));
        }

        public static ContentFieldOptionBuilder ForDisplayMode(this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType)
        {
            return builder.ForDisplayMode(fieldDisplayDriverType, d => true);
        }

        /// <summary>
        /// Registers a display driver for use with a specific display mode.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForDisplayMode<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder, Func<string, bool> predicate)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForDisplayMode(typeof(TContentFieldDisplayDriver), predicate);
        }

        /// <summary>
        /// Registers a display driver for use with a specific display mode.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForDisplayMode(this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType, Func<string, bool> predicate)
        {
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentFieldDisplayMode(builder.ContentFieldType, fieldDisplayDriverType, predicate);
            });

            builder.Services.TryAddScoped(fieldDisplayDriverType);

            return builder;
        }

        /// <summary>
        /// Registers a display driver for use with all editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForEditor<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForEditor(typeof(TContentFieldDisplayDriver));
        }

        /// <summary>
        /// Registers a display driver for use with a specific editor.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForEditor(this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType)
        {
            return builder.ForEditor(fieldDisplayDriverType, d => true);
        }

        /// <summary>
        /// Registers a display driver for use with a specific editor.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForEditor<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder, Func<string, bool> predicate)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForEditor(typeof(TContentFieldDisplayDriver), predicate);
        }

        /// <summary>
        /// Registers a display driver for use with a specific editor.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForEditor(this ContentFieldOptionBuilder builder, Type fieldDisplayDriverType, Func<string, bool> predicate)
        {
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentFieldEditor(builder.ContentFieldType, fieldDisplayDriverType, predicate);
            });

            builder.Services.TryAddScoped(fieldDisplayDriverType);

            return builder;
        }
    }
}

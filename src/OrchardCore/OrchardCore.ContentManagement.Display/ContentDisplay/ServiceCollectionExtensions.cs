using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.ContentManagement.Display.ContentDisplay
{
    public static class ServiceCollectionExtensions
    {
        #region Parts

        /// <summary>
        /// Register a display driver for use with a content part and all editors and display implementations.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder UseDisplayDriver<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder
                .ForDisplay(typeof(TContentPartDisplayDriver), () => true)
                .ForEditor(typeof(TContentPartDisplayDriver), e => true);
        }

        /// <summary>
        /// Register a display driver for use with a content part and all editors and display implementations.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder UseDisplayDriver(this ContentPartOptionBuilder builder, Type partDisplayDriverType)
        {
            return builder.ForDisplay(partDisplayDriverType)
                .ForEditor(partDisplayDriverType);
        }

        /// <summary>
        /// Registers a display driver for use with all display implementations.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplay<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.ForDisplay<TContentPartDisplayDriver>(() => true);
        }

        public static ContentPartOptionBuilder ForDisplay(this ContentPartOptionBuilder builder, Type partDisplayDriverType)
        {
            return builder.ForDisplay(partDisplayDriverType, () => true);
        }

        /// <summary>
        /// Registers a display driver for use with a specific display implementations.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplay<TContentPartDisplayDriver>(this ContentPartOptionBuilder builder, Func<bool> predicate)
            where TContentPartDisplayDriver : class, IContentPartDisplayDriver
        {
            return builder.ForDisplay(typeof(TContentPartDisplayDriver), predicate);
        }

        /// <summary>
        /// Registers a display driver for use with a specific display implementations.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentPartOptionBuilder ForDisplay(this ContentPartOptionBuilder builder, Type partDisplayDriverType, Func<bool> predicate)
        {

            if (!typeof(IContentPartDisplayDriver).IsAssignableFrom(partDisplayDriverType))
            {

                throw new ArgumentException("The type must implement " + nameof(IContentPartDisplayDriver));
            }

            builder.Services.TryAddScoped(partDisplayDriverType);
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentPartDisplay(builder.ContentPartType, partDisplayDriverType, predicate);
            });

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
            return builder.ForEditor<TContentPartDisplayDriver>(e => true);
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
            if (!typeof(IContentPartDisplayDriver).IsAssignableFrom(partDisplayDriverType))
            {

                throw new ArgumentException("The type must implement " + nameof(IContentPartDisplayDriver));
            }

            builder.Services.TryAddScoped(partDisplayDriverType);
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentPartEditor(builder.ContentPartType, partDisplayDriverType, predicate);
            });

            return builder;
        }

        #endregion

        #region Fields
        /// <summary>
        /// Register a display driver for use with a content field and all display modes and editors.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder UseDisplayDriver<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForDisplayMode<TContentFieldDisplayDriver>()
                .ForEditor<TContentFieldDisplayDriver>();
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
        /// Registers a display driver for use with all display modes.
        /// This method will override previous registrations for the driver type,
        /// and can be called multiple times safely, to reconfigure an existing driver.
        /// </summary>
        public static ContentFieldOptionBuilder ForDisplayMode<TContentFieldDisplayDriver>(this ContentFieldOptionBuilder builder)
            where TContentFieldDisplayDriver : class, IContentFieldDisplayDriver
        {
            return builder.ForDisplayMode<TContentFieldDisplayDriver>(d => true);
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

            if (!typeof(IContentFieldDisplayDriver).IsAssignableFrom(fieldDisplayDriverType))
            {

                throw new ArgumentException("The type must implement " + nameof(IContentFieldDisplayDriver));
            }

            builder.Services.TryAddScoped(fieldDisplayDriverType);
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentFieldDisplayMode(builder.ContentFieldType, fieldDisplayDriverType, predicate);
            });

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
            return builder.ForEditor<TContentFieldDisplayDriver>(e => true);
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
            if (!typeof(IContentFieldDisplayDriver).IsAssignableFrom(fieldDisplayDriverType)){

                throw new ArgumentException("The type must implement " + nameof(IContentFieldDisplayDriver));
            }

            builder.Services.TryAddScoped(fieldDisplayDriverType);
            builder.Services.Configure<ContentDisplayOptions>(o =>
            {
                o.ForContentFieldEditor(builder.ContentFieldType, fieldDisplayDriverType, predicate);
            });

            return builder;
        }

        #endregion
    }
}

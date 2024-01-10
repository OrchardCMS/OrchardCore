using System;

namespace OrchardCore.DisplayManagement.Manifest
{
    using Modules.Manifest;

    /// <summary>
    /// Defines a Theme which is a dedicated Module for theming purposes.
    /// If the Theme has only one default feature, it may be defined there.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ThemeAttribute : ModuleAttribute
    {
        /// <summary>
        /// &quot;&quot;
        /// </summary>
        internal const string DefaultBaseTheme = "";

        /// <summary>
        /// Default parameterless ctor.
        /// </summary>
        public ThemeAttribute() : base()
        {
        }

        /// <summary>
        /// Ctor allowing <paramref name="author"/>, as well as defaults for
        /// <paramref name="websiteUrl"/>, <paramref name="semVer"/>, and <paramref name="tags"/>.
        /// </summary>
        /// <param name="id">The identifier for the Module.</param>
        /// <param name="baseTheme">The base theme.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        public ThemeAttribute(
            string id,
            string baseTheme,
            string description,
            string author,
            string semVer,
            string websiteUrl,
            string featureDependencies,
            string tags,
            object defaultTenant,
            object alwaysEnabled,
            object enabledByDependencyOnly
        ) : this(
            id,
            default,
            baseTheme,
            default,
            default,
            description,
            author,
            semVer,
            websiteUrl,
            featureDependencies,
            tags,
            defaultTenant,
            alwaysEnabled,
            enabledByDependencyOnly
        )
        {
        }

        /// <summary>
        /// Ctor allowing <paramref name="author"/>, as well as defaults for
        /// <paramref name="websiteUrl"/>, <paramref name="semVer"/>, and <paramref name="tags"/>.
        /// </summary>
        /// <param name="id">The identifier for the Module.</param>
        /// <param name="name">The feature name, defaults to <see cref="FeatureAttribute.Id"/>
        /// when null or blank.</param>
        /// <param name="baseTheme">The base theme.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        public ThemeAttribute(
            string id,
            string name,
            string baseTheme,
            string description,
            string author,
            string semVer,
            string websiteUrl,
            string featureDependencies,
            string tags,
            object defaultTenant,
            object alwaysEnabled,
            object enabledByDependencyOnly
        ) : this(
            id,
            name,
            baseTheme,
            default,
            default,
            description,
            author,
            semVer,
            websiteUrl,
            featureDependencies,
            tags,
            defaultTenant,
            alwaysEnabled,
            enabledByDependencyOnly
        )
        {
        }

        // TODO: MWP: do we need a 'type' version? probably not...
        /// <summary>
        /// Ctor allowing <paramref name="author"/>, as well as defaults for
        /// <paramref name="websiteUrl"/>, <paramref name="semVer"/>, and <paramref name="tags"/>.
        /// </summary>
        /// <param name="id">The identifier for the Module.</param>
        /// <param name="name">The feature name, defaults to <see cref="FeatureAttribute.Id"/>
        /// when null or blank.</param>
        /// <param name="baseTheme">The base theme.</param>
        /// <param name="category">A simple feature category.</param>
        /// <param name="priority">Priority for the Module.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="string"/> and <see cref="bool"/> only.</param>
        public ThemeAttribute(
            string id,
            string name,
            string baseTheme,
            string category,
            string priority,
            string description,
            string author,
            string semVer,
            string websiteUrl,
            string featureDependencies,
            string tags,
            object defaultTenant,
            object alwaysEnabled,
            object enabledByDependencyOnly
        ) : base(
            id,
            name,
            category,
            priority,
            description,
            author,
            semVer,
            websiteUrl,
            featureDependencies,
            tags,
            defaultTenant,
            alwaysEnabled,
            enabledByDependencyOnly
        )
        {
            BaseTheme = baseTheme;
        }

        private string _baseTheme;

        /// <summary>
        /// Gets or sets the Base Theme if the theme is derived from an existing one.
        /// </summary>
        public string BaseTheme
        {
            get => _baseTheme;
            // Only case we need to be concerned about is Null, everything else we Trim up front
            set => _baseTheme = (value ?? DefaultBaseTheme).Trim();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Modules.Manifest
{
    /// <summary>
    /// Defines a Module which is composed of features. If the Module has only one default
    /// feature, it may be defined there.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class ModuleAttribute : FeatureAttribute
    {
        /// <summary>
        /// &quot;&quot;
        /// </summary>
        internal const string DefaultAuthor = "";

        /// <summary>
        /// &quot;&quot;
        /// </summary>
        internal const string DefaultWebsiteUrl = "";

        /// <summary>
        /// &quot;0.0&quot;
        /// </summary>
        internal const string DefaultVersionZero = "0.0";

        /// <summary>
        /// &quot;&quot;
        /// </summary>
        internal const string DefaultTags = "";

        /// <summary>
        /// Default parameterless ctor.
        /// </summary>
        public ModuleAttribute() : base()
        {
        }

        /// <summary>
        /// Ctor allowing <paramref name="author"/>, as well as defaults for
        /// <paramref name="websiteUrl"/>, <paramref name="semVer"/>, and <paramref name="tags"/>.
        /// </summary>
        /// <param name="id">The identifier for the Module.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public ModuleAttribute(
            string id,
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
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public ModuleAttribute(
            string id,
            string name,
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
        /// <param name="category">A simple feature category.</param>
        /// <param name="priority">Priority for the Module.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public ModuleAttribute(
            string id,
            string name,
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
            featureDependencies,
            defaultTenant,
            alwaysEnabled,
            enabledByDependencyOnly
        )
        {
            Author = author;
            Website = websiteUrl;
            Version = semVer;
            DelimitedTags = tags;
        }

        /// <summary>
        /// Ctor allowing <paramref name="author"/>, as well as defaults for
        /// <paramref name="websiteUrl"/>, <paramref name="semVer"/>, and <paramref name="tags"/>.
        /// </summary>
        /// <param name="id">The identifier for the Module.</param>
        /// <param name="name">The feature name, defaults to <see cref="FeatureAttribute.Id"/>
        /// when null or blank.</param>
        /// <param name="type">User provided type of the Module.</param>
        /// <param name="category">A simple feature category.</param>
        /// <param name="priority">Priority for the Module.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="author">The module author name.</param>
        /// <param name="semVer">Semantic Version string.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="FeatureAttribute.Name"/>
        /// properties.</param>
        /// <param name="websiteUrl">The module website URL.</param>
        /// <param name="tags">Tags associated with the Module.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <remarks>At least <paramref name="author" /> expected herein to differentiate with
        /// parameterless ctor.</remarks>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public ModuleAttribute(
            string id,
            string name,
            string type,
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
        ) : this(
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
            type = (type ?? String.Empty).Trim();
            _type = String.IsNullOrEmpty(type) ? null : type;
        }

        /// <summary>
        /// Returns the <see cref="System.Reflection.MemberInfo.Name"/> less the
        /// <see cref="Attribute"/> suffix when present.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        protected internal static string GetAttributePrefix(Type attributeType)
        {
            const string attributeSuffix = nameof(Attribute);

            // Drops the 'Attribute' suffix from the conventional abbreviation, or leaves it alone
            static string GetTypeNamePrefix(string typeName) =>
                typeName.EndsWith(attributeSuffix)
                ? typeName[..^attributeSuffix.Length]
                : typeName
                ;

            return GetTypeNamePrefix(attributeType.Name);
        }

        private string _type;

        /// <summary>
        /// Gets or sets the Type. Allows authors to identify the attribute by a logical,
        /// human-readable Type. Defaults to the abbreviated <see cref="Attribute"/> class name,
        /// sans suffix.
        /// </summary>
        public virtual string Type
        {
            get => _type ??= GetAttributePrefix(GetType());
            protected internal set => _type = value?.Trim();
        }

        private string _author = DefaultAuthor;

        /// <summary>
        /// Gets or sets the name of the developer.
        /// </summary>
        /// <see cref="DefaultAuthor" />
        public virtual string Author
        {
            get => _author;
            set => _author = (value ?? DefaultAuthor).Trim();
        }

        private string _website = DefaultWebsiteUrl;

        /// <summary>
        /// Gets or sets the URL for the website of the developer.
        /// </summary>
        /// <see cref="DefaultWebsiteUrl" />
        public virtual string Website
        {
            get => _website;
            set => _website = (value ?? DefaultWebsiteUrl).Trim();
        }

        private string _version = DefaultVersionZero;

        /// <summary>
        /// Gets or sets the Semantic Version string.
        /// </summary>
        /// <see cref="!:https://semver.org">Semantic Versioning</see>
        /// <see cref="DefaultVersionZero" />
        public virtual string Version
        {
            get => _version;
            set => _version = (value ?? DefaultVersionZero).Trim();
        }

        /// <summary>
        /// Set-only <see cref="Tags"/> property.
        /// </summary>
        private string DelimitedTags
        {
            set => Tags = (value ?? DefaultTags).Trim().Split(ListDelims, DefaultSplitOptions);
        }

        private string[] _tags = GetValues<string>().ToArray();

        /// <summary>
        /// Gets or sets an array of enumerated Tags.
        /// </summary>
        public virtual string[] Tags
        {
            get => _tags;
            set => _tags = (value ?? GetValues<string>()).Select(_ => _.Trim()).ToArray();
        }

        /// <summary>
        /// Gets a list of Features attributes associated with the Module.
        /// </summary>
        public virtual List<FeatureAttribute> Features { get; } = GetValues<FeatureAttribute>().ToList();
    }
}

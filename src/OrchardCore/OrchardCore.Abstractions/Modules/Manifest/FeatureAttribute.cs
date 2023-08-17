using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Modules.Manifest
{
    using static StringSplitOptions;

    /// <summary>
    /// Defines a Feature in a Module, can be used multiple times.
    /// If at least one Feature is defined, the Module default feature is ignored.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
        /// <summary>
        /// &quot;&quot;
        /// </summary>
        protected internal const string DefaultName = "";

        /// <summary>
        /// &quot;&quot;
        /// </summary>
        protected internal const string DefaultDescription = "";

        /// <summary>
        /// &quot;Uncategorized&quot;
        /// </summary>
        protected internal const string Uncategorized = nameof(Uncategorized);

        /// <summary>
        /// &quot;&quot;
        /// </summary>
        protected internal const string DefaultCategory = "";

        /// <summary>
        /// &quot;&quot;
        /// </summary>
        protected internal const string DefaultFeatureDependencies = "";

        /// <summary>
        /// <c>false</c>
        /// </summary>
        protected internal const bool DefaultDefaultTenantOnly = false;

        /// <summary>
        /// <c>false</c>
        /// </summary>
        protected internal const bool DefaultAlwaysEnabled = false;

        /// <summary>
        /// Default parameterless ctor.
        /// </summary>
        /// <remarks>Cannot route to <c>this(...)</c> in any form here due to restrictions on the Id property setter.</remarks>
        public FeatureAttribute()
        {
            // Defaults are defaults, caller may initialize the properties themselves.
        }

        /// <summary>
        /// Constructs an instance of the attribute with some default values.
        /// </summary>
        /// <param name="id">An identifier overriding the Name.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="Name"/> properties.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.</param>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public FeatureAttribute(
            string id,
            string description,
            string featureDependencies,
            object defaultTenant,
            object alwaysEnabled,
            object enabledByDependencyOnly
        ) : this(
            id,
            default,
            default,
            default,
            description,
            featureDependencies,
            defaultTenant,
            alwaysEnabled,
            enabledByDependencyOnly
        )
        {
        }

        /// <summary>
        /// Constructs an instance of the attribute with some default values.
        /// </summary>
        /// <param name="id">An identifier overriding the Name.</param>
        /// <param name="name">The feature name, defaults to <see cref="Id"/> when null or
        /// blank.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="Name"/> properties.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public FeatureAttribute(
            string id,
            string name,
            string description,
            string featureDependencies,
            object defaultTenant,
            object alwaysEnabled,
            object enabledByDependencyOnly
        ) : this(
            id,
            name,
            default,
            default,
            description,
            featureDependencies,
            defaultTenant,
            alwaysEnabled,
            enabledByDependencyOnly
        )
        {
        }

        /// <summary>
        /// Constructs an instance of the attribute with some default values.
        /// </summary>
        /// <param name="id">An identifier overriding the Name.</param>
        /// <param name="name">The feature name, defaults to <see cref="Id"/> when null or
        /// blank.</param>
        /// <param name="category">A simple feature category.</param>
        /// <param name="priority">The priority of the Feature.</param>
        /// <param name="description">A simple feature description.</param>
        /// <param name="featureDependencies">Zero or more delimited feature dependencies,
        /// corresponding to each of the feature <see cref="Name"/> properties.</param>
        /// <param name="defaultTenant">Whether considered default tenant only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="alwaysEnabled">Whether feature is always enabled.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        /// <param name="enabledByDependencyOnly">Whether feature is enabled by dependency only.
        /// Supported types are <see cref="String"/> and <see cref="Boolean"/> only.</param>
        public FeatureAttribute(
            string id,
            string name,
            string category,
            string priority,
            string description,
            string featureDependencies,
            object defaultTenant,
            object alwaysEnabled,
            object enabledByDependencyOnly
        )
        {
            Id = id;
            Name = name;
            Category = category ?? DefaultCategory;
            Priority = priority ?? String.Empty;
            Description = description ?? DefaultDescription;
            DelimitedDependencies = featureDependencies ?? DefaultFeatureDependencies;

            // https://docs.microsoft.com/en-us/dotnet/api/system.convert.toboolean
            static bool ToBoolean(object value) => Convert.ToBoolean(value);

            DefaultTenantOnly = ToBoolean(defaultTenant);
            IsAlwaysEnabled = ToBoolean(alwaysEnabled);
            EnabledByDependencyOnly = ToBoolean(enabledByDependencyOnly);
        }

        /// <summary>
        /// Whether the feature exists based on the <see cref="Id"/>.
        /// </summary>
        public virtual bool Exists => !String.IsNullOrEmpty(Id);

        private string _id;

        /// <summary>
        /// Gets or sets the feature identifier. Identifier is required.
        /// </summary>
        public virtual string Id
        {
            get => _id;
            set
            {
                // Guards setting Id with strictly invalid values.
                if (String.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException($"When '{nameof(Id)}' has been provided it should not be null or empty.")
                    {
                        Data = { { nameof(value), value } }
                    };
                }

                _id = value;
            }
        }

        private string _name;

        /// <summary>
        /// Returns the <see cref="String"/> <paramref name="s"/> as is, or <c>null</c> when that
        /// or <see cref="String.Empty"/>.
        /// </summary>
        /// <param name="s">The string value to consider.</param>
        /// <returns>The <paramref name="s"/> value as is, or Null when either that or Empty.</returns>
        /// <see cref="String.IsNullOrEmpty(String?)"/>
        internal static string StringOrNull(string s) => String.IsNullOrEmpty(s) ? null : s;

        /// <summary>
        /// Gets or sets the human readable or canonical feature name. <see cref="Id"/> will be
        /// returned when not provided or blank.
        /// </summary>
        public virtual string Name
        {
            get => StringOrNull(_name) ?? Id;
            set => _name = value;
        }

        /// <summary>
        /// Yields return of the <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        protected static IEnumerable<T> GetValues<T>(params T[] values)
        {
            foreach (var value in values)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Gets or sets a brief summary of what the feature does.
        /// </summary>
        public virtual string Description { get; set; } = DefaultDescription;

        /// <summary>
        /// Describes the first or default Feature starting with This instance,
        /// which defines a <see cref="Description"/>.
        /// </summary>
        /// <param name="additionalFeatures">Additional Features to consider in the aggregate.</param>
        /// <returns>The first or default Description with optional back stop features.</returns>
        internal virtual string Describe(params FeatureAttribute[] additionalFeatures)
        {
            static bool IsNotNullOrEmpty(string s) => !String.IsNullOrEmpty(s);
            var firstOrDefaultResult = GetValues(this).Concat(additionalFeatures)
                .Select(feature => feature.Description)
                .FirstOrDefault(IsNotNullOrEmpty);
            return firstOrDefaultResult ?? DefaultDescription;
        }

        /// <summary>
        /// <see cref="TrimEntries"/> | <see cref="RemoveEmptyEntries"/>, trim the entries, and
        /// remove the empty ones.
        /// </summary>
        internal protected const StringSplitOptions DefaultSplitOptions = TrimEntries | RemoveEmptyEntries;

        /// <summary>
        /// Gets the default known ListDelims supporting <see cref="Dependencies"/> splits, etc.
        /// Semi-colon (&apos;;&apos;) delimiters are most common, expected from a <em>CSPROJ</em>
        /// perspective. Also common are comma (&apos;,&apos;) and space (&apos; &apos;)
        /// delimiters.
        /// </summary>
        /// <see cref="String.Split(Char[], StringSplitOptions)"/>
        internal protected static char[] ListDelims { get; } = GetValues(';', ',', ' ').ToArray();

        /// <summary>
        /// Set-only <see cref="Dependencies"/> property.
        /// </summary>
        private string DelimitedDependencies
        {
            set => Dependencies = (value ?? DefaultFeatureDependencies).Trim().Split(ListDelims, DefaultSplitOptions);
        }

        private string[] _dependencies = GetValues<string>().ToArray();

        /// <summary>
        /// Gets or sets an array of Feature Dependencies. Used to arrange drivers, handlers
        /// invoked during startup and so forth.
        /// </summary>
        public virtual string[] Dependencies
        {
            get => _dependencies;
            set => _dependencies = (value ?? GetValues<string>()).Select(_ => _.Trim()).ToArray();
        }

        /// <summary>
        /// 0
        /// </summary>
        protected internal const int DefaultPriority = 0;

        /// <summary>
        /// Gets or sets the feature priority without breaking the <see cref="Dependencies"/>
        /// order. The higher is the priority, the later the drivers / handlers are invoked.
        /// </summary>
        /// <remarks>The default value is aligned with <see cref="DefaultPriority"/>, consistent
        /// with the baseline, however, could be nullified, which would in turn favor the parent
        /// <see cref="ModuleAttribute"/>.</remarks>
        public virtual string Priority { get; set; } = $"{DefaultPriority}";

        /// <summary>
        /// Gets the <see cref="Priority"/>, parsed and ready to go for Internal use. May yield
        /// <c>null</c> when failing to <see cref="Int32.TryParse(String, out Int32)"/>.
        /// </summary>
        internal virtual int? InternalPriority => Int32.TryParse(Priority, out var result) ? result : null;

        /// <summary>
        /// Prioritizes the Features starting with This one, concatenating
        /// <paramref name="additionalFeatures"/>, and lifting the <see cref="InternalPriority"/>
        /// from there. We prefer the first non Null Priority, default
        /// <see cref="DefaultPriority"/>.
        /// </summary>
        /// <param name="additionalFeatures"></param>
        /// <returns></returns>
        internal virtual int Prioritize(params FeatureAttribute[] additionalFeatures)
        {
            var firstPriority = GetValues(this).Concat(additionalFeatures)
                .Select(feature => feature.InternalPriority)
                .FirstOrDefault(priority => priority.HasValue);
            return firstPriority ?? DefaultPriority;
        }

        private string _category = DefaultCategory;

        /// <summary>
        /// Gets or sets the Category for use with the Module.
        /// </summary>
        public virtual string Category
        {
            get => _category;
            set => _category = (value ?? DefaultCategory).Trim();
        }

        /// <summary>
        /// Categorizes This <see cref="Category"/> using <paramref name="additionalFeatures"/> as
        /// back stops, presents the <see cref="Category"/> that is not Null nor Empty, or returns
        /// <see cref="DefaultCategory"/> by default.
        /// </summary>
        /// <param name="additionalFeatures">Additional Feature instances to use as potential back stops.</param>
        /// <returns>The Category normalized across This instance and optional Module.</returns>
        internal virtual string Categorize(params FeatureAttribute[] additionalFeatures)
        {
            static bool IsNotNullOrEmpty(string s) => !String.IsNullOrEmpty(s);
            var categories = GetValues(this).Concat(additionalFeatures).Select(feature => feature.Category);
            var category = categories.FirstOrDefault(IsNotNullOrEmpty);
            // TODO: MWP: 'Uncategorized'? or is empty acceptable here?
            return category ?? Uncategorized;
        }

        /// <summary>
        /// Set to <c>true</c> to only allow the <em>Default tenant to enable or disable</em> the feature.
        /// </summary>
        public virtual bool DefaultTenantOnly { get; set; }

        /// <summary>
        /// Once enabled, check whether the feature cannot be disabled. Defaults to <c>false</c>.
        /// </summary>
        public virtual bool IsAlwaysEnabled { get; set; } = false;

        /// <summary>
        /// Set to <c>true</c> to make the feature available by dependency only.
        /// </summary>
        public virtual bool EnabledByDependencyOnly { get; set; }
    }
}

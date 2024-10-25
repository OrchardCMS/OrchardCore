//// TODO: MWP: will circle around on this one...
//// TODO: MWP: rinse and repeat for module marker ...
//using System;
//using System.Linq;

//namespace OrchardCore.Modules.Manifest
//{
//    using Xunit;
//    using Xunit.Abstractions;

//    /// <inheritdoc/>
//    public class ModuleMarkerAttributeTests : ModuleAttributeTests<ModuleMarkerAttribute>
//    {
//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        /// <param name="outputHelper"></param>
//        public ModuleMarkerAttributeTests(ITestOutputHelper outputHelper) : base(outputHelper)
//        {
//        }

//        /// <summary>
//        /// Classifier supporting
//        /// <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, bool, bool)"/>,
//        /// arguments in order,
//        /// <c>id, baseTheme, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        /// <param name="index">The parameter index.</param>
//        /// <param name="_">The argument, unused.</param>
//        /// <returns></returns>
//        private static Type ThemeString8Bool2CtorClassifier(int index, object _) =>
//            index switch
//            {
//                8 or 9 => typeof(object),
//                _ => typeof(string),
//            };

//        /// <summary>
//        /// &quot;Module&quot;
//        /// </summary>
//        private const string ModuleMarker = nameof(ModuleMarker);

//#pragma warning disable xUnit1008 // Fulfulling base class Theory method test data
//        [InlineData(ModuleMarker)]
//        public override void Default(string type) => base.Default(type);
//#pragma warning restore xUnit1008

//        /// <summary>
//        /// Classifier supporting
//        /// <see cref="ModuleMarkerAttribute(string, string)"/>,
//        /// <see cref="ModuleMarkerAttribute(string, string, string)"/>,
//        /// arguments in order,
//        /// <c>id, name, baseTheme, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        /// <param name="_0">The parameter index.</param>
//        /// <param name="_1">The argument itself, unused.</param>
//        /// <returns>Always returning <see cref="Type"/> of <see cref="string"/> in this case.</returns>
//        private static Type ModuleMarkerStringsCtorClassifier(int _0, object _1) => typeof(string);

//        /// <summary>
//        /// Classifier supporting
//        /// <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, bool, bool)"/>,
//        /// arguments in order,
//        /// <c>id, type, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        /// <param name="index">The parameter index.</param>
//        /// <param name="_">The argument, unused.</param>
//        /// <returns></returns>
//        private static Type ModuleMarkerString8Bool2CtorClassifier(int index, object _) =>
//            index switch
//            {
//                8 or 9 => typeof(object),
//                _ => typeof(string),
//            };

//        /// <summary>
//        /// Classifier supporting
//        /// <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, string, bool, bool)"/>,
//        /// arguments in order,
//        /// <c>id, name, type, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        /// <param name="index">The parameter index.</param>
//        /// <param name="_">The argument, unused.</param>
//        /// <returns></returns>
//        private static Type ModuleMarkerString9Bool2CtorClassifier(int index, object _) =>
//            index switch
//            {
//                9 or 10 => typeof(object),
//                _ => typeof(string),
//            };

//        /// <summary>
//        /// Classifier supporting
//        /// <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, string, string, string, bool, bool)"/>,
//        /// arguments in order,
//        /// <c>id, name, type, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        /// <param name="index">The parameter index.</param>
//        /// <param name="_">The argument, unused.</param>
//        /// <returns></returns>
//        private static Type ModuleMarkerString11Bool2CtorClassifier(int index, object _) =>
//            index switch
//            {
//                11 or 12 => typeof(object),
//                _ => typeof(string),
//            };

//        /// <summary>
//        /// Verify the <see cref="ModuleMarkerAttribute(string, string)"/>
//        /// ctor, arguments
//        /// <c>id, type</c>.
//        /// </summary>
//        [Fact]
//        public virtual void Ipsum_Ctor_Id_Type()
//        {
//            var id = LoremWords(1);
//            var type = LoremWords(1);
//            var category = DefaultCategory;
//            var priority = DefaultPriority;
//            var description = DefaultDescription;
//            var author = DefaultAuthor;
//            var semVer = DefaultVersionZero;
//            var deps = GetArray<string>();
//            var website = DefaultWebsiteUrl;
//            var tags = GetArray<string>();
//            var defaultTenant = DefaultDefaultTenantOnly;
//            var alwaysEnabled = DefaultAlwaysEnabled;

//            //var depString = string.Join(';', deps);
//            //var tagString = string.Join(';', tags);
//            var priString = string.Empty;

//            ReportKeyValuePairs(
//                new RenderKeyValuePair(nameof(id), id),
//                new RenderKeyValuePair(nameof(type), type)
//            );

//            var marker = CreateFromArgs(ModuleMarkerStringsCtorClassifier, id, type);

//            Assert.Equal(id, marker.Id);
//            Assert.Equal(id, marker.Name);
//            Assert.Equal(type, marker.Type);

//            Assert.Equal(category, marker.Category);
//            Assert.Null(marker.InternalPriority);
//            Assert.Equal(priString, marker.Priority);

//            Assert.Equal(description, marker.Description);

//            Assert.NotNull(marker.Dependencies);
//            Assert.Equal(deps, marker.Dependencies);

//            Assert.NotNull(marker.Tags);
//            Assert.Equal(tags, marker.Tags);

//            Assert.Equal(author, marker.Author);
//            Assert.Equal(semVer, marker.Version);
//            Assert.Equal(website, marker.Website);

//            Assert.Equal(defaultTenant, marker.DefaultTenantOnly);
//            Assert.Equal(alwaysEnabled, marker.IsAlwaysEnabled);

//            Assert.NotNull(marker.Features);
//            Assert.Empty(marker.Features);
//        }

//        /// <summary>
//        /// Verify the <see cref="ModuleMarkerAttribute(string, string, string)"/>
//        /// ctor, arguments
//        /// <c>id, name, type</c>.
//        /// </summary>
//        [Fact]
//        public virtual void Ipsum_Ctor_Id_Name_Type()
//        {
//            var id = LoremWords(1);
//            var name = LoremWords(1);
//            var type = LoremWords(1);
//            var category = DefaultCategory;
//            var priority = DefaultPriority;
//            var description = DefaultDescription;
//            var author = DefaultAuthor;
//            var semVer = DefaultVersionZero;
//            var deps = GetArray<string>();
//            var website = DefaultWebsiteUrl;
//            var tags = GetArray<string>();
//            var defaultTenant = DefaultDefaultTenantOnly;
//            var alwaysEnabled = DefaultAlwaysEnabled;

//            //var depString = string.Join(';', deps);
//            //var tagString = string.Join(';', tags);
//            var priString = string.Empty;

//            ReportKeyValuePairs(
//                new RenderKeyValuePair(nameof(id), id),
//                new RenderKeyValuePair(nameof(name), name),
//                new RenderKeyValuePair(nameof(type), type)
//            );

//            var marker = CreateFromArgs(ModuleMarkerStringsCtorClassifier, id, name, type);

//            Assert.Equal(id, marker.Id);
//            Assert.Equal(name, marker.Name);
//            Assert.Equal(type, marker.Type);

//            Assert.Equal(category, marker.Category);
//            Assert.Null(marker.InternalPriority);
//            Assert.Equal(priString, marker.Priority);

//            Assert.Equal(description, marker.Description);

//            Assert.NotNull(marker.Dependencies);
//            Assert.Equal(deps, marker.Dependencies);

//            Assert.NotNull(marker.Tags);
//            Assert.Equal(tags, marker.Tags);

//            Assert.Equal(author, marker.Author);
//            Assert.Equal(semVer, marker.Version);
//            Assert.Equal(website, marker.Website);

//            Assert.Equal(defaultTenant, marker.DefaultTenantOnly);
//            Assert.Equal(alwaysEnabled, marker.IsAlwaysEnabled);

//            Assert.NotNull(marker.Features);
//            Assert.Empty(marker.Features);
//        }

//        /// <summary>
//        /// Verify the <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, bool, bool)"/>
//        /// ctor, arguments
//        /// <c>id, type, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        [Fact]
//        public virtual void Ipsum_Ctor_Id_Type_Author()
//        {
//            var id = LoremWords(1);
//            var type = LoremWords(1);
//            var category = DefaultCategory;
//            var description = DefaultDescription;
//            var author = DefaultAuthor;
//            var semVer = DefaultVersionZero;
//            var deps = LoremWords(5).Split(' ');
//            var website = $"lorem://{LoremWords(1)}.ipsum";
//            var tags = LoremWords(5).Split(' ');
//            var defaultTenant = DefaultDefaultTenantOnly;
//            var alwaysEnabled = DefaultAlwaysEnabled;

//            var depString = string.Join(';', deps);
//            var tagString = string.Join(';', tags);
//            var priString = string.Empty;

//            ReportKeyValuePairs(
//                new RenderKeyValuePair(nameof(id), id),
//                new RenderKeyValuePair(nameof(type), type),
//                new RenderKeyValuePair(nameof(description), description),
//                new RenderKeyValuePair(nameof(author), author),
//                new RenderKeyValuePair(nameof(semVer), semVer),
//                new RenderKeyValuePair(nameof(deps), depString),
//                new RenderKeyValuePair(nameof(tags), tagString),
//                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
//                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled)
//            );

//            var marker = CreateFromArgs(ModuleMarkerString8Bool2CtorClassifier, id, type, description, author, semVer, depString, website, tagString, defaultTenant, alwaysEnabled);

//            Assert.Equal(id, marker.Id);
//            Assert.Equal(id, marker.Name);
//            Assert.Equal(type, marker.Type);

//            Assert.Equal(category, marker.Category);
//            Assert.Null(marker.InternalPriority);
//            Assert.Equal(priString, marker.Priority);

//            Assert.Equal(description, marker.Description);

//            Assert.NotNull(marker.Dependencies);
//            Assert.Equal(deps, marker.Dependencies);

//            Assert.NotNull(marker.Tags);
//            Assert.Equal(tags, marker.Tags);

//            Assert.Equal(author, marker.Author);
//            Assert.Equal(semVer, marker.Version);
//            Assert.Equal(website, marker.Website);

//            Assert.Equal(defaultTenant, marker.DefaultTenantOnly);
//            Assert.Equal(alwaysEnabled, marker.IsAlwaysEnabled);

//            Assert.NotNull(marker.Features);
//            Assert.Empty(marker.Features);
//        }

//        /// <summary>
//        /// Verify the <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, string, bool, bool)"/>
//        /// ctor, arguments
//        /// <c>id, name, type, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        [Fact]
//        public virtual void Ipsum_Ctor_Id_Name_Type_Author()
//        {
//            var id = LoremWords(1);
//            var name = LoremWords(1);
//            var type = LoremWords(1);
//            var category = DefaultCategory;
//            var description = DefaultDescription;
//            var author = DefaultAuthor;
//            var semVer = DefaultVersionZero;
//            var deps = LoremWords(5).Split(' ');
//            var website = LoremWebsiteUrl();
//            var tags = LoremWords(5).Split(' ');
//            var defaultTenant = DefaultDefaultTenantOnly;
//            var alwaysEnabled = DefaultAlwaysEnabled;

//            var depString = string.Join(';', deps);
//            var tagString = string.Join(';', tags);
//            var priString = string.Empty;

//            ReportKeyValuePairs(
//                new RenderKeyValuePair(nameof(id), id),
//                new RenderKeyValuePair(nameof(name), name),
//                new RenderKeyValuePair(nameof(type), type),
//                new RenderKeyValuePair(nameof(description), description),
//                new RenderKeyValuePair(nameof(author), author),
//                new RenderKeyValuePair(nameof(semVer), semVer),
//                new RenderKeyValuePair(nameof(deps), depString),
//                new RenderKeyValuePair(nameof(website), website),
//                new RenderKeyValuePair(nameof(tags), tagString),
//                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
//                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled)
//            );

//            var marker = CreateFromArgs(ModuleMarkerString9Bool2CtorClassifier, id, name, type, description, author, semVer, depString, website, tagString, defaultTenant, alwaysEnabled);

//            Assert.Equal(id, marker.Id);
//            Assert.Equal(name, marker.Name);
//            Assert.Equal(type, marker.Type);

//            Assert.Equal(category, marker.Category);
//            Assert.Null(marker.InternalPriority);
//            Assert.Equal(priString, marker.Priority);

//            Assert.Equal(description, marker.Description);

//            Assert.NotNull(marker.Dependencies);
//            Assert.Equal(deps, marker.Dependencies);

//            Assert.NotNull(marker.Tags);
//            Assert.Equal(tags, marker.Tags);

//            Assert.Equal(author, marker.Author);
//            Assert.Equal(semVer, marker.Version);
//            Assert.Equal(website, marker.Website);

//            Assert.Equal(defaultTenant, marker.DefaultTenantOnly);
//            Assert.Equal(alwaysEnabled, marker.IsAlwaysEnabled);

//            Assert.NotNull(marker.Features);
//            Assert.Empty(marker.Features);
//        }

//        /// <summary>
//        /// Verify the <see cref="ModuleMarkerAttribute(string, string, string, string, string, string, string, string, string, string, string, bool, bool)"/>
//        /// ctor, arguments
//        /// <c>id, name, type, description, category, priority, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled</c>.
//        /// </summary>
//        [Fact]
//        public virtual void Ipsum_Ctor_Id_Name_Type_Cat_Pri_Author()
//        {
//            var id = LoremWords(1);
//            var name = LoremWords(1);
//            var type = LoremWords(1);
//            var category = LoremWords(1);
//            var priority = DefaultPriority + 1;
//            var description = DefaultDescription;
//            var author = DefaultAuthor;
//            var semVer = DefaultVersionZero;
//            var deps = LoremWords(5).Split(' ');
//            var website = $"lorem://{LoremWords(1)}.ipsum";
//            var tags = LoremWords(5).Split(' ');
//            var defaultTenant = DefaultDefaultTenantOnly;
//            var alwaysEnabled = DefaultAlwaysEnabled;

//            var depString = string.Join(';', deps);
//            var tagString = string.Join(';', tags);
//            var priString = $"{priority}";

//            ReportKeyValuePairs(
//                new RenderKeyValuePair(nameof(id), id),
//                new RenderKeyValuePair(nameof(name), name),
//                new RenderKeyValuePair(nameof(type), type),
//                new RenderKeyValuePair(nameof(category), category),
//                new RenderKeyValuePair(nameof(priority), priString),
//                new RenderKeyValuePair(nameof(description), description),
//                new RenderKeyValuePair(nameof(author), author),
//                new RenderKeyValuePair(nameof(semVer), semVer),
//                new RenderKeyValuePair(nameof(deps), depString),
//                new RenderKeyValuePair(nameof(website), website),
//                new RenderKeyValuePair(nameof(tags), tagString),
//                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
//                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled)
//            );

//            var marker = CreateFromArgs(ModuleMarkerString11Bool2CtorClassifier, id, name, type, category, priString, description, author, semVer, depString, website, tagString, defaultTenant, alwaysEnabled);

//            Assert.Equal(id, marker.Id);
//            Assert.Equal(name, marker.Name);
//            Assert.Equal(type, marker.Type);

//            Assert.Equal(category, marker.Category);
//            Assert.Equal(priority, marker.InternalPriority);
//            Assert.Equal(priString, marker.Priority);

//            Assert.Equal(description, marker.Description);

//            Assert.NotNull(marker.Dependencies);
//            Assert.Equal(deps, marker.Dependencies);

//            Assert.NotNull(marker.Tags);
//            Assert.Equal(tags, marker.Tags);

//            Assert.Equal(author, marker.Author);
//            Assert.Equal(semVer, marker.Version);
//            Assert.Equal(website, marker.Website);

//            Assert.Equal(defaultTenant, marker.DefaultTenantOnly);
//            Assert.Equal(alwaysEnabled, marker.IsAlwaysEnabled);

//            Assert.NotNull(marker.Features);
//            Assert.Empty(marker.Features);
//        }
//    }
//}

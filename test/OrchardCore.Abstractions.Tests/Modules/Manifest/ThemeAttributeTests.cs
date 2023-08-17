// TODO: MWP: will circle around on this one...
// TODO: MWP: rinse and repeat for module marker ...
using System;
using System.Linq;

namespace OrchardCore.DisplayManagement.Manifest
{
    using Modules.Manifest;
    using Xunit;
    using Xunit.Abstractions;

    /// <inheritdoc/>
    public class ThemeAttributeTests : ModuleAttributeTests<ThemeAttribute>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        public ThemeAttributeTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Classifier supporting
        /// <see cref="ThemeAttribute(String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, baseTheme, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index">The parameter index.</param>
        /// <param name="_">The argument, unused.</param>
        /// <returns></returns>
        private static Type ThemeString8Bool3CtorClassifier(int index, object _) =>
            index switch
            {
                8 or 9 or 10 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// &quot;Module&quot;
        /// </summary>
        private const string Theme = nameof(Theme);

#pragma warning disable xUnit1008 // Fulfulling base class Theory method test data
        [InlineData(Theme)]
        public override void Default(string type) => base.Default(type);
#pragma warning restore xUnit1008

        /// <summary>
        /// Classifier supporting
        /// <see cref="ThemeAttribute(String, String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, name, baseTheme, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index">The parameter index.</param>
        /// <param name="_">The argument, unused.</param>
        /// <returns></returns>
        private static Type ThemeString9Bool3CtorClassifier(int index, object _) =>
            index switch
            {
                9 or 10 or 11 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Classifier supporting
        /// <see cref="ThemeAttribute(String, String, String, String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, name, baseTheme, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index">The parameter index.</param>
        /// <param name="_">The argument, unused.</param>
        /// <returns></returns>
        private static Type ThemeString11Bool3CtorClassifier(int index, object _) =>
            index switch
            {
                11 or 12 or 13 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Verify the <see cref="ThemeAttribute(String, String, String, String, String, String, String ,String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, baseTheme, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id()
        {
            var id = LoremWords(1);
            var baseTheme = LoremWords(1);
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var deps = LoremWords(5).Split(' ');
            var website = LoremWebsiteUrl();
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(baseTheme), baseTheme),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(author), author),
                new RenderKeyValuePair(nameof(semVer), semVer),
                new RenderKeyValuePair(nameof(website), website),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(tags), tagString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            var theme = CreateFromArgs(ThemeString8Bool3CtorClassifier, id, baseTheme, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, theme.Id);
            Assert.Equal(id, theme.Name);
            Assert.Equal(baseTheme, theme.BaseTheme);

            Assert.Equal(String.Empty, theme.Category);
            Assert.Null(theme.InternalPriority);
            Assert.Equal(String.Empty, theme.Priority);

            Assert.Equal(description, theme.Description);

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(deps, theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(tags, theme.Tags);

            Assert.Equal(author, theme.Author);
            Assert.Equal(semVer, theme.Version);
            Assert.Equal(website, theme.Website);

            Assert.Equal(defaultTenant, theme.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        /// <summary>
        /// Verify the <see cref="ThemeAttribute(String, String, String, String, String, String, String, String ,String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, baseTheme, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var baseTheme = LoremWords(1);
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var deps = LoremWords(5).Split(' ');
            var website = LoremWebsiteUrl();
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(baseTheme), baseTheme),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(author), author),
                new RenderKeyValuePair(nameof(semVer), semVer),
                new RenderKeyValuePair(nameof(website), website),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(tags), tagString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            var theme = CreateFromArgs(ThemeString9Bool3CtorClassifier, id, name, baseTheme, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, theme.Id);
            Assert.Equal(name, theme.Name);
            Assert.Equal(baseTheme, theme.BaseTheme);

            Assert.Equal(String.Empty, theme.Category);
            Assert.Null(theme.InternalPriority);
            Assert.Equal(String.Empty, theme.Priority);

            Assert.Equal(description, theme.Description);

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(deps, theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(tags, theme.Tags);

            Assert.Equal(author, theme.Author);
            Assert.Equal(semVer, theme.Version);
            Assert.Equal(website, theme.Website);

            Assert.Equal(defaultTenant, theme.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        /// <summary>
        /// Verify the <see cref="ThemeAttribute(String, String, String, String, String, String, String, String, String, String ,String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, baseTheme, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name_Cat_Pri()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var baseTheme = LoremWords(1);
            var category = LoremWords(1);
            var priority = DefaultPriority + 1;
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var deps = LoremWords(5).Split(' ');
            var website = LoremWebsiteUrl();
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);
            var priString = $"{priority}";

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(baseTheme), baseTheme),
                new RenderKeyValuePair(nameof(category), category),
                new RenderKeyValuePair(nameof(priority), priority),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(author), author),
                new RenderKeyValuePair(nameof(semVer), semVer),
                new RenderKeyValuePair(nameof(website), website),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(tags), tagString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            var theme = CreateFromArgs(ThemeString11Bool3CtorClassifier, id, name, baseTheme, category, priString, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, theme.Id);
            Assert.Equal(name, theme.Name);
            Assert.Equal(baseTheme, theme.BaseTheme);

            Assert.Equal(category, theme.Category);
            Assert.Equal(priority, theme.InternalPriority);
            Assert.Equal(priString, theme.Priority);

            Assert.Equal(description, theme.Description);

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(deps, theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(tags, theme.Tags);

            Assert.Equal(author, theme.Author);
            Assert.Equal(semVer, theme.Version);
            Assert.Equal(website, theme.Website);

            Assert.Equal(defaultTenant, theme.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id"/> except via <em>MSBuild</em> <c>AssemblyAttribute</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_Id()
        {
            var id = "one";
            var baseTheme = "two";
            var category = String.Empty;
            var description = "three";
            var author = "four";
            var semVer = "5.6.7";
            var website = LoremAssyAttribIpsumUrl;
            var deps = GetArray("seven", "eight", "nine");
            var tags = GetArray("eight", "nine", "ten");
            const bool defaultTenant = true;
            const bool alwaysEnabled = true;
            const bool enabledByDependencyOnly = default;

            var priString = String.Empty;
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Themes.AssyAttrib.Charlie.Root);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(baseTheme), baseTheme),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(author), author),
                new RenderKeyValuePair(nameof(semVer), semVer),
                new RenderKeyValuePair(nameof(website), website),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(tags), tagString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            // We are looking for one instance of ThemeAttribute in particular in this case
            var theme = GetAssemblyAttribute<ThemeAttribute>(rootType, _ => _.GetType() == typeof(ThemeAttribute));
            // We are also not expecting any other strict ModuleAttribute match
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute) && _.GetType() != typeof(ThemeAttribute));
            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(theme);
            Assert.Null(module);

            Assert.Equal(id, theme.Id);
            Assert.Equal(id, theme.Name);
            Assert.Equal(baseTheme, theme.BaseTheme);

            Assert.Equal(category, theme.Category);
            Assert.Null(theme.InternalPriority);
            Assert.Equal(priString, theme.Priority);

            Assert.Equal(description, theme.Description);

            Assert.Equal(author, theme.Author);
            Assert.Equal(semVer, theme.Version);
            Assert.Equal(website, theme.Website);

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(deps, theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(tags, theme.Tags);

            Assert.Equal(defaultTenant, theme.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id_Name"/> except via <em>MSBuild</em> <c>AssemblyAttribute</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_Id_Name()
        {
            var id = "one";
            var name = "two";
            var baseTheme = "three";
            var category = String.Empty;
            var description = "four";
            var author = "five";
            var semVer = "6.7.8";
            var website = LoremAssyAttribIpsumUrl;
            var deps = GetArray("eight", "nine", "ten");
            var tags = GetArray("nine", "ten", "eleven");
            const bool defaultTenant = true;
            const bool alwaysEnabled = true;
            const bool enabledByDependencyOnly = default;

            var priString = String.Empty;
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Themes.AssyAttrib.Bravo.Root);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(baseTheme), baseTheme),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(author), author),
                new RenderKeyValuePair(nameof(semVer), semVer),
                new RenderKeyValuePair(nameof(website), website),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(tags), tagString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            // We are looking for one instance of ThemeAttribute in particular in this case
            var theme = GetAssemblyAttribute<ThemeAttribute>(rootType, _ => _.GetType() == typeof(ThemeAttribute));
            // We are also not expecting any other strict ModuleAttribute match
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute) && _.GetType() != typeof(ThemeAttribute));
            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(theme);
            Assert.Null(module);

            Assert.Equal(id, theme.Id);
            Assert.Equal(name, theme.Name);
            Assert.Equal(baseTheme, theme.BaseTheme);

            Assert.Equal(category, theme.Category);
            Assert.Null(theme.InternalPriority);
            Assert.Equal(priString, theme.Priority);

            Assert.Equal(description, theme.Description);

            Assert.Equal(author, theme.Author);
            Assert.Equal(semVer, theme.Version);
            Assert.Equal(website, theme.Website);

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(deps, theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(tags, theme.Tags);

            Assert.Equal(defaultTenant, theme.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id_Name_Cat_Pri"/> except via <em>MSBuild</em> <c>AssemblyAttribute</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_Id_Name_Cat_Pri()
        {
            var id = "one";
            var name = "two";
            var baseTheme = "three";
            var category = "four";
            const int priority = 5;
            var description = "six";
            var author = "seven";
            var semVer = "8.9.10";
            var website = LoremAssyAttribIpsumUrl;
            var deps = GetArray("ten", "eleven", "twelve");
            var tags = GetArray("eleven", "twelve", "thirteen");
            const bool defaultTenant = true;
            const bool alwaysEnabled = true;
            const bool enabledByDependencyOnly = default;

            var priString = $"{priority}";
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Themes.AssyAttrib.Alpha.Root);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(baseTheme), baseTheme),
                new RenderKeyValuePair(nameof(category), category),
                new RenderKeyValuePair(nameof(priority), priority),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(author), author),
                new RenderKeyValuePair(nameof(semVer), semVer),
                new RenderKeyValuePair(nameof(website), website),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(tags), tagString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            // We are looking for one instance of ThemeAttribute in particular in this case
            var theme = GetAssemblyAttribute<ThemeAttribute>(rootType, _ => _.GetType() == typeof(ThemeAttribute));
            // We are also not expecting any other strict ModuleAttribute match
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute) && _.GetType() != typeof(ThemeAttribute));
            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(theme);
            Assert.Null(module);

            Assert.Equal(id, theme.Id);
            Assert.Equal(name, theme.Name);
            Assert.Equal(baseTheme, theme.BaseTheme);

            Assert.Equal(category, theme.Category);
            Assert.Equal(priority, theme.InternalPriority);
            Assert.Equal(priString, theme.Priority);

            Assert.Equal(description, theme.Description);

            Assert.Equal(author, theme.Author);
            Assert.Equal(semVer, theme.Version);
            Assert.Equal(website, theme.Website);

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(deps, theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(tags, theme.Tags);

            Assert.Equal(defaultTenant, theme.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id_Name_Cat_Pri"/> except via <em>MSBuild</em>
        /// <c>OrchardCoreThemes</c> list items. Not going to be concerned with any
        /// <c>OrchardCoreFeatures</c> at this level, this has been handled by the similarly
        /// named <em>Modules</em> tests.
        /// </summary>
        [Fact]
        public virtual void Csproj_OrchardCoreThemes_MSBuild_ItemLists()
        {
            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.OrchardCoreThemes.Alpha.Root);
            var baseId = rootType.Assembly.GetName().Name;

            const string three = nameof(three);
            const string four = nameof(four);
            const string six = nameof(six);
            const string seven = nameof(seven);
            const string ten = nameof(ten);
            const string eleven = nameof(eleven);
            const string twelve = nameof(twelve);
            const string thirteen = nameof(thirteen);
            const bool tenant = true;
            const bool enabled = true;

            const int _5 = 5;

            const string Theme = nameof(Theme);

            // TODO: TBD: may report what is being tested also for Module, Features, etc...

            // We are looking for one instance of ModuleAttribute in particular in this case
            var theme = GetAssemblyAttribute<ThemeAttribute>(rootType, _ => _.GetType() == typeof(ThemeAttribute));

            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(theme);

            Assert.Equal(baseId, theme.Id);
            Assert.Equal(baseId, theme.Name);
            Assert.Equal(Theme, theme.Type);

            Assert.Equal(three, theme.BaseTheme);

            Assert.Equal(four, theme.Category);
            Assert.Equal(_5, theme.InternalPriority);
            Assert.Equal($"{_5}", theme.Priority);

            Assert.Equal(six, theme.Description);

            Assert.Equal(seven, theme.Author);
            Assert.Equal("oc://msbuildthemeitemlists.prop", theme.Website);

            // Mind the trailing zeros that is intentional, and the field count, also intentional
            VerifyVersion(theme, rootType.Assembly, new Version(8, 9, 10, 0), 3);
            //                                                  ^^^^^^^^^^^   ^

            Assert.NotNull(theme.Dependencies);
            Assert.Equal(GetValues(ten, eleven, twelve), theme.Dependencies);

            Assert.NotNull(theme.Tags);
            Assert.Equal(GetValues(eleven, twelve, thirteen), theme.Tags);

            Assert.Equal(tenant, theme.DefaultTenantOnly);
            Assert.Equal(enabled, theme.IsAlwaysEnabled);

            Assert.NotNull(theme.Features);
            Assert.Empty(theme.Features);
        }

        // TODO: TBD: ditto ModuleAttributeTests re: in process Build scaffold
    }
}

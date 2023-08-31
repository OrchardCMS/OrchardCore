using System;
using System.Linq;

namespace OrchardCore.Modules.Manifest
{
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// 
    /// </summary>
    public class FeatureAttributeTests : FeatureAttributeTests<FeatureAttribute>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        public FeatureAttributeTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Verifies default Static and Constant assets, as exposed by fixture properties.
        /// </summary>
        [Fact]
        public virtual void Default_Static_Consts()
        {
            Assert.Empty(DefaultName);
            Assert.Empty(DefaultCategory);
            Assert.Equal(0, DefaultPriority);
            Assert.Empty(DefaultDescription);
            Assert.Empty(DefaultFeatureDependencies);
            Assert.False(DefaultDefaultTenantOnly);
            Assert.False(DefaultAlwaysEnabled);
            Assert.Equal(GetArray(';', ',', ' '), ListDelims);
        }

        /// <summary>
        /// Verify the <see cref="FeatureAttribute(String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, description, featureDependencies, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id()
        {
            var id = LoremWords(1);
            var description = LoremWords(7);
            var deps = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var depString = String.Join(';', deps);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            var feature = CreateFromArgs(FeatureString3Object3CtorClassifier, id, description, depString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.True(feature.Exists);
            Assert.Equal(id, feature.Id);
            Assert.Equal(id, feature.Name);

            Assert.Empty(feature.Category);
            Assert.Equal(description, feature.Description);

            Assert.Null(feature.InternalPriority);
            Assert.Equal(String.Empty, feature.Priority);

            Assert.NotNull(feature.Dependencies);
            Assert.Equal(deps, feature.Dependencies);

            Assert.Equal(defaultTenant, feature.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, feature.IsAlwaysEnabled);
        }

        /// <summary>
        /// Verify the <see cref="FeatureAttribute(String, String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, description, featureDependencies, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var description = LoremWords(7);
            var deps = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var depString = String.Join(';', deps);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            var feature = CreateFromArgs(FeatureString4Object3CtorClassifier, id, name, description, depString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.True(feature.Exists);
            Assert.Equal(id, feature.Id);
            Assert.Equal(name, feature.Name);

            Assert.Empty(feature.Category);
            Assert.Equal(description, feature.Description);

            Assert.Null(feature.InternalPriority);
            Assert.Equal(String.Empty, feature.Priority);

            Assert.NotNull(feature.Dependencies);
            Assert.Equal(deps, feature.Dependencies);

            Assert.Equal(defaultTenant, feature.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, feature.IsAlwaysEnabled);
        }

        /// <summary>
        /// Verify the <see cref="FeatureAttribute(String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, category, priority, description, featureDependencies, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name_Cat_Pri()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var category = LoremWords(1);
            var priority = DefaultPriority + 1;
            var description = LoremWords(7);
            var deps = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var depString = String.Join(';', deps);
            var priString = $"{priority}";

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(category), category),
                new RenderKeyValuePair(nameof(priority), priority),
                new RenderKeyValuePair(nameof(description), description),
                new RenderKeyValuePair(nameof(deps), depString),
                new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
                new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled),
                new RenderKeyValuePair(nameof(enabledByDependencyOnly), enabledByDependencyOnly)
            );

            var feature = CreateFromArgs(FeatureString6Object3CtorClassifier, id, name, category, priString, description, depString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.True(feature.Exists);
            Assert.Equal(id, feature.Id);
            Assert.Equal(name, feature.Name);

            Assert.Equal(category, feature.Category);
            Assert.Equal(description, feature.Description);

            Assert.Equal(priority, feature.InternalPriority);
            Assert.Equal(priString, feature.Priority);

            Assert.NotNull(feature.Dependencies);
            Assert.Equal(deps, feature.Dependencies);

            Assert.Equal(defaultTenant, feature.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, feature.IsAlwaysEnabled);
        }

        // OrchardCore.Manifest.Features

        /// <summary>
        /// Which depends on <c>ItemGroup</c> items defined by <c>OrchardCoreFeatures</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_EmbedFeatures_Target()
        {
            //// TODO: MWP: devote a bit of thought to how better to test these...
            //var id = "one";
            //var name = "two";
            //var category = "three";
            //const int priority = 4;
            //var description = "five";
            //var author = "six";
            //var semVer = "7.8.9";
            //var website = LoremAssyAttribIpsumUrl;
            //var deps = GetArray("nine", "ten", "eleven");
            //var tags = GetArray("ten", "eleven", "twelve");
            //const bool defaultTenant = true;
            //const bool alwaysEnabled = true;

            //var priString = $"{priority}";
            //var depString = string.Join(';', deps);
            //var tagString = string.Join(';', tags);

            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Features.AssyAttrib.Root);

            //ReportKeyValuePairs(
            //    new RenderKeyValuePair(nameof(id), id),
            //    new RenderKeyValuePair(nameof(name), name),
            //    new RenderKeyValuePair(nameof(category), category),
            //    new RenderKeyValuePair(nameof(priority), priority),
            //    new RenderKeyValuePair(nameof(description), description),
            //    new RenderKeyValuePair(nameof(author), author),
            //    new RenderKeyValuePair(nameof(semVer), semVer),
            //    new RenderKeyValuePair(nameof(website), website),
            //    new RenderKeyValuePair(nameof(deps), depString),
            //    new RenderKeyValuePair(nameof(tags), tagString),
            //    new RenderKeyValuePair(nameof(defaultTenant), defaultTenant),
            //    new RenderKeyValuePair(nameof(alwaysEnabled), alwaysEnabled)
            //);

            var features = GetAssemblyAttributes<FeatureAttribute>(rootType, _ => _.GetType() == typeof(FeatureAttribute));

            Assert.NotNull(features);

            //// We are looking for one instance of ModuleAttribute in particular in this case
            //var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute));
            //// TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            //// TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            //Assert.NotNull(module);

            //Assert.Equal(id, module.Id);
            //Assert.Equal(name, module.Name);

            //Assert.Equal(category, module.Category);
            //Assert.Equal(priority, module.InternalPriority);
            //Assert.Equal(priString, module.Priority);

            //Assert.Equal(description, module.Description);

            //Assert.Equal(author, module.Author);
            //Assert.Equal(semVer, module.Version);
            //Assert.Equal(website, module.Website);

            //Assert.NotNull(module.Dependencies);
            //Assert.Equal(deps, module.Dependencies);

            //Assert.NotNull(module.Tags);
            //Assert.Equal(tags, module.Tags);

            //Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            //Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            //Assert.NotNull(module.Features);
            //Assert.Empty(module.Features);
        }

        /// <summary>
        /// Verifies the <see cref="FeatureAttribute.Prioritize(FeatureAttribute[])"/> method in a
        /// handful of scenarios.
        /// </summary>
        [Fact]
        public virtual void Prioritize()
        {
            const string name = null;
            const string category = null;
            const string description = null;
            const string depString = null;
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var priority = DefaultPriority;
            var expected = priority + 1;

            // TODO: TBD: also for attributes created using property initializers
            FeatureAttribute CreateForPriority(string priString = null) => CreateFromArgs(
                FeatureString6Object3CtorClassifier,
                LoremWords(1),
                name,
                category,
                priString ?? String.Empty,
                description,
                depString,
                defaultTenant,
                alwaysEnabled,
                enabledByDependencyOnly
            );

            var alpha = CreateForPriority();
            var bravo = CreateForPriority($"{expected}");

            // Verifies that we can Prioritize as the root and a couple of argument positions
            Assert.Equal(expected, bravo.Prioritize(alpha, alpha));
            Assert.Equal(expected, alpha.Prioritize(bravo, alpha));
            Assert.Equal(expected, alpha.Prioritize(alpha, bravo));
            Assert.Equal(priority, alpha.Prioritize(alpha, alpha));
        }

        /// <summary>
        /// Verifies the <see cref="FeatureAttribute.Describe(FeatureAttribute[])"/> method in a
        /// handful of scenarios.
        /// </summary>
        [Fact]
        public virtual void Describe()
        {
            const string name = null;
            const string category = null;
            const string priority = null;
            const string depString = null;
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var expected = LoremWords(7);

            // TODO: TBD: also for attributes created using property initializers
            FeatureAttribute CreateForDescription(string description = null) => CreateFromArgs(
                FeatureString6Object3CtorClassifier,
                LoremWords(1),
                name,
                category,
                priority,
                description,
                depString,
                defaultTenant,
                alwaysEnabled,
                enabledByDependencyOnly
            );

            var alpha = CreateForDescription();
            var bravo = CreateForDescription(expected);

            // Verifies that we can Describe as the root Feature, and a couple of argument positions
            Assert.Equal(expected, bravo.Describe(alpha, alpha));
            Assert.Equal(expected, alpha.Describe(bravo, alpha));
            Assert.Equal(expected, alpha.Describe(alpha, bravo));
            Assert.Equal(FeatureAttribute.DefaultDescription, alpha.Describe(alpha, alpha));
        }

        /// <summary>
        /// Verifies the <see cref="FeatureAttribute.Categorize(FeatureAttribute[])"/> method in a
        /// handful of scenarios.
        /// </summary>
        [Fact]
        public virtual void Categorize()
        {
            const string name = null;
            const string priority = null;
            const string description = null;
            const string depString = null;
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var expected = LoremWords(1);

            // TODO: TBD: also for attributes created using property initializers
            FeatureAttribute CreateForCategory(string category = null) => CreateFromArgs(
                FeatureString6Object3CtorClassifier,
                LoremWords(1),
                name,
                category,
                priority,
                description,
                depString,
                defaultTenant,
                alwaysEnabled,
                enabledByDependencyOnly
            );

            var alpha = CreateForCategory();
            var bravo = CreateForCategory(expected);

            // Verifies that we can Describe as the root Feature, and a couple of argument positions
            Assert.Equal(expected, bravo.Categorize(alpha, alpha));
            Assert.Equal(expected, alpha.Categorize(bravo, alpha));
            Assert.Equal(expected, alpha.Categorize(alpha, bravo));
            Assert.Equal(FeatureAttribute.Uncategorized, alpha.Categorize(alpha, alpha));
        }

        /// <summary>
        /// Verifies that <see cref="FeatureAttribute.Dependencies"/> functions correctly in
        /// a handful of delimiter scenarios.
        /// </summary>
        /// <param name="delim"></param>
        [
            Theory,
            InlineData(';'),
            InlineData(','),
            InlineData(' '),
            InlineData(':')
        ]
        public virtual void Dependencies(char delim)
        {
            var deps = LoremWords(5).Split(' ');
            var depString = String.Join(delim, deps);

            var listDelims = FeatureAttribute.ListDelims;

            FeatureAttribute CreateForDeps(params string[] deps) => CreateFromArgs(
                FeatureString6Object3CtorClassifier,
                LoremWords(1),
                null,
                null,
                null,
                null,
                depString,
                default(bool),
                default(bool),
                default(bool)
            );

            var feature = CreateForDeps(deps);

            if (listDelims.Contains(delim))
            {
                // Validity reflected by the Deps via the Delim
                Assert.Equal(deps, feature.Dependencies);
            }
            else
            {
                // Which in this case 'correctly' yields, although semantically not what we expected, given an invalid delim
                Assert.Equal(GetValues(depString), feature.Dependencies);
            }
        }
    }
}

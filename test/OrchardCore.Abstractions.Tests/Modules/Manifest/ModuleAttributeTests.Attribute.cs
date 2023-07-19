using System;
using System.Reflection;

namespace OrchardCore.Modules.Manifest
{
    using Xunit;
    using Xunit.Abstractions;

    /// <inheritdoc/>
    public abstract class ModuleAttributeTests<TAttribute> : FeatureAttributeTests<TAttribute>
        where TAttribute : ModuleAttribute
    {
        /// <summary>
        /// Gets the <see cref="ModuleAttribute.DefaultAuthor"/>.
        /// </summary>
        protected static string DefaultAuthor => ModuleAttribute.DefaultAuthor;

        /// <summary>
        /// Gets the <see cref="ModuleAttribute.DefaultVersionZero"/>.
        /// </summary>
        protected static string DefaultVersionZero => ModuleAttribute.DefaultVersionZero;

        /// <summary>
        /// Gets the <see cref="ModuleAttribute.DefaultWebsiteUrl"/>.
        /// </summary>
        protected static string DefaultWebsiteUrl => ModuleAttribute.DefaultWebsiteUrl;

        /// <summary>
        /// &quot;lorem://assyattrib.ipsum&quot;
        /// </summary>
        protected const string LoremAssyAttribIpsumUrl = "lorem://assyattrib.ipsum";

        /// <summary>
        /// Returns a Lorem Ipsum based Url. Completely fictional, folks, for test purposes only.
        /// </summary>
        /// <returns></returns>
        protected virtual string LoremWebsiteUrl() => $"lorem://{LoremWords(1)}.ipsum";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        public ModuleAttributeTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// Returns the <typeparamref name="TDetails"/> based on the given
        /// <paramref name="assembly"/>.
        /// </summary>
        /// <typeparam name="TDetails"></typeparam>
        /// <param name="assembly"></param>
        /// <param name="extract"></param>
        /// <returns></returns>
        protected static TDetails GetAssemblyDetails<TDetails>(Assembly assembly, Func<Assembly, AssemblyName, TDetails> extract) =>
            extract.Invoke(assembly, assembly.GetName());

        protected override void VerifyDefault(TAttribute module)
        {
            base.VerifyDefault(module);

            Assert.Equal(DefaultAuthor, module.Author);
            Assert.Equal(DefaultVersionZero, module.Version);
            Assert.Equal(DefaultWebsiteUrl, module.Website);

            Assert.NotNull(module.Tags);
            Assert.Empty(module.Tags);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /// <summary>
        /// Verifies the Default <paramref name="module"/> aspects, including
        /// <see cref="ModuleAttribute.Type"/>.
        /// </summary>
        /// <param name="module">The module to consider.</param>
        /// <param name="type">Type to consider.</param>
        protected virtual void VerifyDefault(TAttribute module, string type)
        {
            VerifyDefault(module);

            Assert.Equal(type, module.Type);
        }

#pragma warning disable xUnit1013 // Public method on test class should be marked as a Theory
#pragma warning disable xUnit1003 // Test data fulfilled by derived tests
        /// <summary>
        /// Verifies that the Default instance values, etc, are appropriate.
        /// </summary>
        /// <remarks>Re: pragma warnings, should be, etc, which it is. However, we separated
        /// Theory+Data between classes, appropriately so.</remarks>
        [Theory]
        public virtual void Default(string type) => VerifyDefault(CreateFromArgs(classifier: null), type);
#pragma warning restore xUnit1003
#pragma warning restore xUnit1013

        /// <summary>
        /// Verifies the <paramref name="expected"/> <see cref="Version"/>, including
        /// <paramref name="fieldCount"/> aligned with the <em>OrchardCore</em> usage. Note
        /// that <paramref name="expected"/> may require trailing zeros on account of how
        /// <em>MSBuild</em> treats versions during target attribute code generation.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="assembly"></param>
        /// <param name="expected"></param>
        /// <param name="fieldCount"></param>
        protected virtual void VerifyVersion(TAttribute module, Assembly assembly, Version expected, int fieldCount)
        {
            Assert.NotNull(module);
            Assert.NotNull(assembly);
            Assert.NotNull(expected);
            Assert.True(fieldCount >= 0 && fieldCount <= 3);

            // Overall, not too concerned care trailing zero octets, but for test purposes we need to be aware.
            var assyName = assembly.GetName();
            var assyVersion = assyName.Version;
            // Because a zero Revision octet is assumed by MSBuild et al.
            Assert.Equal(expected, assyVersion);
            // And along similar lines, only render the field count octets, aligned with the Module Version for test purposes.
            Assert.Equal(assyVersion.ToString(fieldCount), module.Version);
        }

        /// <summary>
        /// Classifier supporting
        /// <see cref="ModuleAttribute(String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type ModuleString7Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                7 or 8 or 9 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Classifier supporting
        /// <see cref="ModuleAttribute(String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, name, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type ModuleString8Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                8 or 9 or 10 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Classifier supporting
        /// <see cref="ModuleAttribute(String, String, String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, name, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type ModuleString10Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                10 or 11 or 12 => typeof(object),
                _ => typeof(string),
            };

        /// <summary>
        /// Classifier supporting
        /// <see cref="ModuleAttribute(String, String, String, String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>,
        /// arguments in order,
        /// <c>id, name, type, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        protected virtual Type ModuleString11Object3CtorClassifier(int index, object arg) =>
            index switch
            {
                11 or 12 or 13 => typeof(object),
                _ => typeof(string),
            };
    }
}

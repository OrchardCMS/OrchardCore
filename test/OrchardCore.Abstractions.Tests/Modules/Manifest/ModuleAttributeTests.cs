using System;
using System.Linq;

namespace OrchardCore.Modules.Manifest
{
    using DisplayManagement.Manifest;
    using Xunit;
    using Xunit.Abstractions;

    // TODO: TBD: add tests that help us expose non-named ctor args...
    // TODO: TBD: i.e. avoid ambiguous args...
    /// <inheritdoc/>
    public class ModuleAttributeTests : ModuleAttributeTests<ModuleAttribute>
    {
        // TODO: TBD: also include tests which verify known project/assembly references
        // TODO: TBD: with the idea that we introduce them in the csproj like we might otherwise expect

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHelper"></param>
        public ModuleAttributeTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        /// <summary>
        /// &quot;Module&quot;
        /// </summary>
        private const string Module = nameof(Module);

#pragma warning disable xUnit1008 // Fulfulling base class Theory method test data
        [InlineData(Module)]
        public override void Default(string type) => base.Default(type);
#pragma warning restore xUnit1008

#pragma warning disable CA1018 // Specify AttributeUsage...
        // No need since the attribute is only here in order to verify Prefix extraction
        public class TestAttributePrefix : Attribute { }
#pragma warning restore CA1018

        // TODO: MWP: could probably separate this between classes, Theory at one level, InlineData at another...
        // TODO: MWP: plus separate out DisplayManagement tests to its own assembly, potentialy...
        /// <summary>
        /// Verify a couple of scenarios parsing an Attribute Prefix.
        /// </summary>
        /// <param name="attributeType"></param>
        /// <param name="expected"></param>
        [
            Theory,
            InlineData(typeof(ModuleAttribute), "Module"),
            InlineData(typeof(ThemeAttribute), "Theme"),
            InlineData(typeof(TestAttributePrefix), nameof(TestAttributePrefix))
        ]
        public virtual void AttributePrefix(Type attributeType, string expected)
        {
            Assert.Equal(expected, ModuleAttribute.GetAttributePrefix(attributeType));
        }

        /// <summary>
        /// Verify the <see cref="ModuleAttribute(String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id()
        {
            var id = LoremWords(1);
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var website = LoremWebsiteUrl();
            var deps = LoremWords(5).Split(' ');
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var priString = String.Empty;
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
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

            var module = CreateFromArgs(ModuleString7Object3CtorClassifier, id, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, module.Id);
            Assert.Equal(id, module.Name);

            Assert.NotNull(module.Category);
            Assert.Empty(module.Category);

            Assert.Null(module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        // TODO: TBD: add the ArgumentException path for the bool (object) variations...
        /// <summary>
        /// Verify the <see cref="ModuleAttribute(String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var website = LoremWebsiteUrl();
            var deps = LoremWords(5).Split(' ');
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var priString = String.Empty;
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
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

            var module = CreateFromArgs(ModuleString8Object3CtorClassifier, id, name, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, module.Id);
            Assert.Equal(name, module.Name);

            Assert.NotNull(module.Category);
            Assert.Empty(module.Category);

            Assert.Null(module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /// <summary>
        /// Verify the <see cref="ModuleAttribute(String, String, String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name_Cat_Pri()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var category = LoremWords(1);
            var priority = DefaultPriority + 1;
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var website = LoremWebsiteUrl();
            var deps = LoremWords(5).Split(' ');
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var priString = $"{priority}";
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
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

            var module = CreateFromArgs(ModuleString10Object3CtorClassifier, id, name, category, priString, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, module.Id);
            Assert.Equal(name, module.Name);

            Assert.Equal(category, module.Category);
            Assert.Equal(priority, module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /// <summary>
        /// Verify the <see cref="ModuleAttribute(String, String, String, String, String, String, String, String, String, String, String, Boolean, Boolean, Boolean)"/>
        /// ctor, arguments
        /// <c>id, name, type, category, priority, description, author, semVer, featureDependencies, websiteUrl, tags, defaultTenant, alwaysEnabled, enabledByDependencyOnly</c>.
        /// </summary>
        [Fact]
        public virtual void Ipsum_Ctor_Id_Name_Type_Cat_Pri()
        {
            var id = LoremWords(1);
            var name = LoremWords(1);
            var type = LoremWords(1);
            var category = LoremWords(1);
            var priority = DefaultPriority + 1;
            var description = LoremWords(7);
            var author = LoremWords(2);
            var semVer = String.Join('.', GetValues(1, 2, 3, 4).Select(_ => $"{_}"));
            var website = LoremWebsiteUrl();
            var deps = LoremWords(5).Split(' ');
            var tags = LoremWords(5).Split(' ');
            const bool defaultTenant = default;
            const bool alwaysEnabled = default;
            const bool enabledByDependencyOnly = default;

            var priString = $"{priority}";
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
                new RenderKeyValuePair(nameof(type), type),
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

            var module = CreateFromArgs(ModuleString11Object3CtorClassifier, id, name, type, category, priString, description, author, semVer, website, depString, tagString, defaultTenant, alwaysEnabled, enabledByDependencyOnly);

            Assert.Equal(id, module.Id);
            Assert.Equal(name, module.Name);
            Assert.Equal(type, module.Type);

            Assert.Equal(category, module.Category);
            Assert.Equal(priority, module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /////// TODO: MWP: so far so good... review, rinse, and repeat with ThemeAttribute in view...
        /////// TODO: MWP: then need to (re-)consider whether ModuleMarkerAttribute is a desirable thing after all?
        /////// TODO: MWP: what purpose d(oes/id) it serve, would a simple ctor on ModuleAttribute itself be sufficient?
        ///// TODO: MWP: possible xUnit issue discovering this as a Theory... / https://github.com/xunit/xunit/discussions/2508
        ///// <param name = "rootType" ></ param >
        //[
        //    Theory,
        //    InlineData(typeof(Abstractions.AssyAttrib.Alpha.Root))
        //]
        //public virtual void Csproj_AssyAttrib_Id_Name_Cat_Pri(Type rootType)

        // TODO: MWP: verify as well, if there are services, contexts, infos, etc, that are more actively involved...
        // TODO: MWP: involved how, discovering, dissecting, disseminating, distributing, bits of Module details
        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id"/> except via <em>MSBuild</em> <c>AssemblyAttribute</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_Id()
        {
            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Modules.AssyAttrib.Charlie.Root);

            var id = "one";
            var description = "two";
            var category = DefaultCategory;
            var author = "three";
            var semVer = "4.5.6";
            var website = LoremAssyAttribIpsumUrl;
            var deps = GetArray("six", "seven", "eight");
            var tags = GetArray("seven", "eight", "nine");
            const bool defaultTenant = true;
            const bool alwaysEnabled = true;
            const bool enabledByDependencyOnly = default;

            var priString = String.Empty;
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
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

            // We are looking for one instance of ModuleAttribute in particular in this case
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute));
            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(module);

            Assert.Equal(id, module.Id);
            Assert.Equal(id, module.Name);

            Assert.Equal(category, module.Category);
            Assert.Null(module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id_Name"/> except via <em>MSBuild</em> <c>AssemblyAttribute</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_Id_Name()
        {
            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Modules.AssyAttrib.Bravo.Root);

            var id = "one";
            var name = "two";
            var description = "three";
            var category = DefaultCategory;
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

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
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

            // We are looking for one instance of ModuleAttribute in particular in this case
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute));
            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(module);

            Assert.Equal(id, module.Id);
            Assert.Equal(name, module.Name);

            Assert.Equal(category, module.Category);
            Assert.Null(module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id_Name_Cat_Pri"/> except via <em>MSBuild</em> <c>AssemblyAttribute</c>.
        /// </summary>
        [Fact]
        public virtual void Csproj_AssyAttrib_Id_Name_Cat_Pri()
        {
            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.Modules.AssyAttrib.Alpha.Root);

            var id = "one";
            var name = "two";
            var category = "three";
            const int priority = 4;
            var description = "five";
            var author = "six";
            var semVer = "7.8.9";
            var website = LoremAssyAttribIpsumUrl;
            var deps = GetArray("nine", "ten", "eleven");
            var tags = GetArray("ten", "eleven", "twelve");
            const bool defaultTenant = true;
            const bool alwaysEnabled = true;
            const bool enabledByDependencyOnly = default;

            var priString = $"{priority}";
            var depString = String.Join(';', deps);
            var tagString = String.Join(';', tags);

            ReportKeyValuePairs(
                new RenderKeyValuePair(nameof(id), id),
                new RenderKeyValuePair(nameof(name), name),
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

            // We are looking for one instance of ModuleAttribute in particular in this case
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute));
            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(module);

            Assert.Equal(id, module.Id);
            Assert.Equal(name, module.Name);

            Assert.Equal(category, module.Category);
            Assert.Equal(priority, module.InternalPriority);
            Assert.Equal(priString, module.Priority);

            Assert.Equal(description, module.Description);

            Assert.Equal(author, module.Author);
            Assert.Equal(semVer, module.Version);
            Assert.Equal(website, module.Website);

            Assert.NotNull(module.Dependencies);
            Assert.Equal(deps, module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(tags, module.Tags);

            Assert.Equal(defaultTenant, module.DefaultTenantOnly);
            Assert.Equal(alwaysEnabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);
        }

        /// <summary>
        /// Same as <see cref="Ipsum_Ctor_Id_Name_Cat_Pri"/> except via <em>MSBuild</em>
        /// <c>OrchardCoreModules</c> list items. We also pick up the <c>OrchardCoreFeatures</c>
        /// list items in this case as well.
        /// </summary>
        [Fact]
        public virtual void Csproj_OrchardCoreModules_MSBuild_ItemLists()
        {
            // Would inject via Theory but for issues xUnit failing to discover the cases
            var rootType = typeof(Examples.OrchardCoreModules.Alpha.Root);
            var baseId = rootType.Assembly.GetName().Name;

            const string One = nameof(One);
            const string Two = nameof(Two);
            const string three = nameof(three);
            const string four = nameof(four);
            const string five = nameof(five);
            const string six = nameof(six);
            const string seven = nameof(seven);
            const string eight = nameof(eight);
            const string nine = nameof(nine);
            const string ten = nameof(ten);
            const string eleven = nameof(eleven);
            const string twelve = nameof(twelve);
            const string thirteen = nameof(thirteen);
            const bool tenant = true;
            const bool enabled = true;

            const int _3 = 3;
            const int _4 = 4;

            const string Module = nameof(Module);

            // TODO: TBD: may report what is being tested also for Module, Features, etc...

            // We are looking for one instance of ModuleAttribute in particular in this case
            var module = GetAssemblyAttribute<ModuleAttribute>(rootType, _ => _.GetType() == typeof(ModuleAttribute));
            var features = GetAssemblyAttributes<FeatureAttribute>(rootType, _ => _.GetType() == typeof(FeatureAttribute))
                // We shall assume the test case includes a valid sortable Priority
                .OrderBy(_ => _.InternalPriority).ToArray();

            // TODO: MWP: note that targets are mining for project properties and are also injecting a ModuleMarkerAttribute...
            // TODO: MWP: ...which is also a 'Module', which is a possible source of confusion, spoofing, counterfeit, fraud, etc
            Assert.NotNull(module);

            Assert.Equal(baseId, module.Id);
            Assert.Equal(baseId, module.Name);
            Assert.Equal(Module, module.Type);

            Assert.Equal(three, module.Category);
            Assert.Equal(_4, module.InternalPriority);
            Assert.Equal($"{_4}", module.Priority);

            Assert.Equal(five, module.Description);

            Assert.Equal(six, module.Author);
            Assert.Equal("oc://moduleandfeaturesmsbuilditemlists.prop", module.Website);

            // Mind the trailing zeros that is intentional, and the field count, also intentional
            VerifyVersion(module, rootType.Assembly, new Version(7, 8, 9, 0), 3);
            //                                                   ^^^^^^^^^^   ^

            Assert.NotNull(module.Dependencies);
            Assert.Equal(GetValues(nine, ten, eleven), module.Dependencies);

            Assert.NotNull(module.Tags);
            Assert.Equal(GetValues(ten, eleven, twelve), module.Tags);

            Assert.Equal(tenant, module.DefaultTenantOnly);
            Assert.Equal(enabled, module.IsAlwaysEnabled);

            Assert.NotNull(module.Features);
            Assert.Empty(module.Features);

            /* Features should contain the following composites as well, which, by definition,
             * should contain 'two' in the following shapes:
             */
            Assert.Contains(features, _ =>
                _.Id == String.Join(".", baseId, One) &&
                _.Name == _.Id &&
                _.Category == Two.ToLower() &&
                _.InternalPriority == _3 &&
                _.Description == four &&
                _.Dependencies.SequenceEqual(GetValues(five, six, seven)) &&
                _.DefaultTenantOnly == tenant &&
                _.IsAlwaysEnabled == enabled
            );

            Assert.Contains(features, _ =>
                _.Id == String.Join(".", baseId, Two) &&
                _.Name == _.Id &&
                _.Category == three &&
                _.InternalPriority == _4 &&
                _.Description == five &&
                _.Dependencies.SequenceEqual(GetValues(six, seven, eight)) &&
                _.DefaultTenantOnly == tenant &&
                _.IsAlwaysEnabled == enabled
            );
        }

        /* TODO: TBD: might be interesting to add tests that allow projects to be built in
         * process, and to gauge the status codes in response accordingly... The supporting
         * projects are 'there' already, and all we would need to do is load them in an
         * appropriate Build context, try to build, and monitor.
         */
    }
}

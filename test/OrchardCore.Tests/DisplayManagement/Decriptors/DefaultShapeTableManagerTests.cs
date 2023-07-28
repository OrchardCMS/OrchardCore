using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Manifest;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules.Manifest;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement.Decriptors
{
    public class DefaultShapeTableManagerTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private class TestModuleExtensionInfo : IExtensionInfo
        {
            public TestModuleExtensionInfo(string name)
            {
                var dic1 = new Dictionary<string, string>()
                {
                    {"name", name},
                    {"desciption", name},
                    {"type", "module"},
                };

                var memConfigSrc1 = new MemoryConfigurationSource { InitialData = dic1 };
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.Add(memConfigSrc1);

                Manifest = new ManifestInfo(new ModuleAttribute());

                var features =
                    new List<IFeatureInfo>()
                    {
                        { new FeatureInfo(name, name, 0, String.Empty, String.Empty, this, Array.Empty<string>(), false, false, false) }
                    };

                Features = features;
            }

            public IFileInfo ExtensionFileInfo { get; set; }
            public IEnumerable<IFeatureInfo> Features { get; set; }
            public string Id { get; set; }
            public IManifestInfo Manifest { get; set; }
            public string SubPath { get; set; }
            public bool Exists => true;
        }

        private class TestThemeExtensionInfo : IThemeExtensionInfo
        {
            public TestThemeExtensionInfo(string name)
            {
                var dic1 = new Dictionary<string, string>()
                {
                    {"name", name},
                    {"desciption", name},
                    {"type", "theme"},
                };

                var memConfigSrc1 = new MemoryConfigurationSource { InitialData = dic1 };
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.Add(memConfigSrc1);

                Manifest = new ManifestInfo(new ThemeAttribute());

                var features =
                    new List<IFeatureInfo>()
                    {
                        { new FeatureInfo(name, name, 0, String.Empty, String.Empty, this, Array.Empty<string>(), false, false, false) }
                    };

                Features = features;

                Id = name;
            }

            public TestThemeExtensionInfo(string name, IFeatureInfo baseTheme)
            {
                var dic1 = new Dictionary<string, string>()
                {
                    {"name", name},
                    {"desciption", name},
                    {"type", "theme"},
                    {"basetheme", baseTheme.Id }
                };

                var memConfigSrc1 = new MemoryConfigurationSource { InitialData = dic1 };
                var configurationBuilder = new ConfigurationBuilder();
                configurationBuilder.Add(memConfigSrc1);

                Manifest = new ManifestInfo(new ThemeAttribute());

                Features =
                    new List<IFeatureInfo>()
                    {
                        { new FeatureInfo(name, name, 0, String.Empty, String.Empty, this, new string[] { baseTheme.Id }, false, false, false) }
                    };

                Id = name;
            }

            public IFileInfo ExtensionFileInfo { get; set; }
            public IEnumerable<IFeatureInfo> Features { get; set; }
            public string Id { get; set; }
            public IManifestInfo Manifest { get; set; }
            public string SubPath { get; set; }
            public bool Exists => true;
        }

        public DefaultShapeTableManagerTests()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            serviceCollection.AddScoped<IShellFeaturesManager, TestShellFeaturesManager>();
            serviceCollection.AddScoped<IShapeTableManager, DefaultShapeTableManager>();
            serviceCollection.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();
            serviceCollection.AddSingleton<IHostEnvironment>(new StubHostingEnvironment());

            var testFeatureExtensionInfo = new TestModuleExtensionInfo("Testing");
            var theme1FeatureExtensionInfo = new TestThemeExtensionInfo("Theme1");
            var baseThemeFeatureExtensionInfo = new TestThemeExtensionInfo("BaseTheme");
            var derivedThemeFeatureExtensionInfo = new TestThemeExtensionInfo("DerivedTheme", baseThemeFeatureExtensionInfo.Features.First());

            var features = new[] {
                testFeatureExtensionInfo.Features.First(),
                theme1FeatureExtensionInfo.Features.First(),
                baseThemeFeatureExtensionInfo.Features.First(),
                derivedThemeFeatureExtensionInfo.Features.First()
            };

            serviceCollection.AddSingleton<IExtensionManager>(new TestExtensionManager(features));

            TestShapeProvider.InitFeatureShapes(new Dictionary<IFeatureInfo, IEnumerable<string>>
            {
                { TestFeature(), new [] {"Hello"} },
                { features[1], new [] {"Theme1Shape"} },
                { features[2], new [] {"DerivedShape", "OverriddenShape"} },
                { features[3], new [] {"BaseShape", "OverriddenShape"} },
            });

            serviceCollection.AddScoped<IShapeTableProvider, TestShapeProvider>();
            serviceCollection.AddScoped(sp => (TestShapeProvider)sp.GetService<IShapeTableProvider>());

            _serviceProvider = serviceCollection.BuildServiceProvider();

            var typeFeatureProvider = _serviceProvider.GetService<ITypeFeatureProvider>();
            typeFeatureProvider.TryAdd(typeof(TestShapeProvider), TestFeature());
        }

        private static IFeatureInfo TestFeature()
        {
            return new FeatureInfo("Testing", new TestModuleExtensionInfo("Testing"));
        }

        public class TestShellFeaturesManager : IShellFeaturesManager
        {
            private readonly IExtensionManager _extensionManager;

            public TestShellFeaturesManager(IExtensionManager extensionManager)
            {
                _extensionManager = extensionManager;
            }

            public Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync()
            {
                return Task.FromResult(_extensionManager.GetFeatures());
            }

            public Task<IEnumerable<IFeatureInfo>> GetAlwaysEnabledFeaturesAsync()
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync()
            {
                throw new NotImplementedException();
            }

            public Task<(IEnumerable<IFeatureInfo>, IEnumerable<IFeatureInfo>)> UpdateFeaturesAsync(IEnumerable<IFeatureInfo> featuresToDisable, IEnumerable<IFeatureInfo> featuresToEnable, bool force)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync()
            {
                return Task.FromResult(_extensionManager.GetExtensions());
            }

            public Task<IEnumerable<IFeatureInfo>> GetAvailableFeaturesAsync()
            {
                return Task.FromResult(_extensionManager.GetFeatures());
            }

            public Task<IEnumerable<IExtensionInfo>> GetAvailableExtensionsAsync()
            {
                return Task.FromResult(_extensionManager.GetExtensions());
            }
        }

        public class TestExtensionManager : IExtensionManager
        {
            private readonly IEnumerable<IFeatureInfo> _features;
            public TestExtensionManager(IEnumerable<IFeatureInfo> features)
            {
                _features = features;
            }

            public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
            {
                var feature = _features.First(x => x.Id == featureId);

                return _features.Where(x => feature.Dependencies.Contains(x.Id));
            }

            public IExtensionInfo GetExtension(string extensionId)
            {
                return _features.Select(x => x.Extension).First(x => x.Id == extensionId);
            }

            public IEnumerable<IExtensionInfo> GetExtensions()
            {
                return _features.Select(x => x.Extension).Distinct();
            }

            public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<ExtensionEntry>> LoadExtensionsAsync(IEnumerable<IExtensionInfo> extensionInfos)
            {
                throw new NotImplementedException();
            }

#pragma warning disable CA1822 // Mark members as static
            public Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
            {
                return Task.FromResult(new FeatureEntry(feature));
            }

            public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(IEnumerable<IFeatureInfo> features)
            {
                return Task.FromResult(features.Select(x => new FeatureEntry(x)));
            }
#pragma warning restore CA1822 // Mark members as static

            public IEnumerable<IFeatureInfo> GetFeatures()
            {
                return _features;
            }

            public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
            {
                return _features.Where(x => featureIdsToLoad.Contains(x.Id));
            }

            public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync()
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(string[] featureIdsToLoad)
            {
                throw new NotImplementedException();
            }
        }

        public class TestShapeProvider : IShapeTableProvider
        {
            private static IDictionary<IFeatureInfo, IEnumerable<string>> _featureShapes;

            public static IDictionary<IFeatureInfo, IEnumerable<string>> FeatureShapes => _featureShapes;

            public Action<ShapeTableBuilder> Discover = x => { };

            public static void InitFeatureShapes(IDictionary<IFeatureInfo, IEnumerable<string>> featureShapes)
                => _featureShapes = featureShapes;

            void IShapeTableProvider.Discover(ShapeTableBuilder builder)
            {
                foreach (var pair in FeatureShapes)
                {
                    foreach (var shape in pair.Value)
                    {
                        builder.Describe(shape).From(pair.Key).BoundAs(pair.Key.Id, null);
                    }
                }

                Discover(builder);
            }
        }

        [Fact]
        public void ManagerCanBeResolved()
        {
            var manager = _serviceProvider.GetService<IShapeTableManager>();
            Assert.NotNull(manager);
        }

        [Fact]
        public void DefaultShapeTableIsReturnedForNullOrEmpty()
        {
            var manager = _serviceProvider.GetService<IShapeTableManager>();
            var shapeTable1 = manager.GetShapeTable(null);
            var shapeTable2 = manager.GetShapeTable(String.Empty);
            Assert.NotNull(shapeTable1.Descriptors["Hello"]);
            Assert.NotNull(shapeTable2.Descriptors["Hello"]);
        }

        [Fact]
        public void CallbackAlterationsContributeToDescriptor()
        {
            Func<ShapeCreatingContext, Task> cb1 = x => { return Task.CompletedTask; };
            Func<ShapeCreatedContext, Task> cb2 = x => { return Task.CompletedTask; };
            Func<ShapeDisplayContext, Task> cb3 = x => { return Task.CompletedTask; };
            Func<ShapeDisplayContext, Task> cb4 = x => { return Task.CompletedTask; };

            _serviceProvider.GetService<TestShapeProvider>().Discover =
                builder => builder.Describe("Foo").From(TestFeature())
                               .OnCreating(cb1)
                               .OnCreated(cb2)
                               .OnDisplaying(cb3)
                               .OnDisplayed(cb4);

            var manager = _serviceProvider.GetService<IShapeTableManager>();

            var foo = manager.GetShapeTable(null).Descriptors["Foo"];

            Assert.Same(cb1, foo.CreatingAsync.Single());
            Assert.Same(cb2, foo.CreatedAsync.Single());
            Assert.Same(cb3, foo.DisplayingAsync.Single());
            Assert.Same(cb4, foo.DisplayedAsync.Single());
        }
        [Fact]
        public void DefaultPlacementIsReturnedByDefault()
        {
            var manager = _serviceProvider.GetService<IShapeTableManager>();

            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
            hello.DefaultPlacement = "Header:5";
            var result = hello.Placement(null);
            Assert.Equal("Header:5", result.Location);
        }

        [Fact]
        public void DescribedPlacementIsReturnedIfNotNull()
        {
            var shapeDetail = new ShapePlacementContext("Foo", "Detail", String.Empty, null);
            var shapeSummary = new ShapePlacementContext("Foo", "Summary", String.Empty, null);
            var shapeTitle = new ShapePlacementContext("Foo", "Title", String.Empty, null);

            _serviceProvider.GetService<TestShapeProvider>().Discover =
                builder => builder.Describe("Hello1").From(TestFeature())
                    .Placement(ctx => ctx.DisplayType == "Detail" ? new PlacementInfo { Location = "Main" } : null)
                    .Placement(ctx => ctx.DisplayType == "Summary" ? new PlacementInfo { Location = String.Empty } : null);

            var manager = _serviceProvider.GetService<IShapeTableManager>();
            var hello = manager.GetShapeTable(null).Descriptors["Hello1"];
            var result1 = hello.Placement(shapeDetail);
            var result2 = hello.Placement(shapeSummary);
            var result3 = hello.Placement(shapeTitle);
            hello.DefaultPlacement = "Header:5";
            var result4 = hello.Placement(shapeDetail);
            var result5 = hello.Placement(shapeSummary);
            var result6 = hello.Placement(shapeTitle);

            Assert.Equal("Main", result1.Location);
            Assert.Empty(result2.Location);
            Assert.Null(result3);
            Assert.Equal("Main", result4.Location);
            Assert.Empty(result5.Location);
            Assert.Equal("Header:5", result6.Location);
        }

        [Fact]
        public void TwoArgumentVariationDoesSameThing()
        {
            var shapeDetail = new ShapePlacementContext("Foo", "Detail", String.Empty, null);
            var shapeSummary = new ShapePlacementContext("Foo", "Summary", String.Empty, null);
            var shapeTitle = new ShapePlacementContext("Foo", "Title", String.Empty, null);

            _serviceProvider.GetService<TestShapeProvider>().Discover =
                builder => builder.Describe("Hello2").From(TestFeature())
                    .Placement(ctx => ctx.DisplayType == "Detail", new PlacementInfo { Location = "Main" })
                    .Placement(ctx => ctx.DisplayType == "Summary", new PlacementInfo { Location = String.Empty });

            var manager = _serviceProvider.GetService<IShapeTableManager>();
            var hello = manager.GetShapeTable(null).Descriptors["Hello2"];
            var result1 = hello.Placement(shapeDetail);
            var result2 = hello.Placement(shapeSummary);
            var result3 = hello.Placement(shapeTitle);
            hello.DefaultPlacement = "Header:5";
            var result4 = hello.Placement(shapeDetail);
            var result5 = hello.Placement(shapeSummary);
            var result6 = hello.Placement(shapeTitle);

            Assert.Equal("Main", result1.Location);
            Assert.Empty(result2.Location);
            Assert.Null(result3);
            Assert.Equal("Main", result4.Location);
            Assert.Empty(result5.Location);
            Assert.Equal("Header:5", result6.Location);
        }

        // "Path not supported yet"
        [Fact]
        internal void PathConstraintShouldMatch()
        {
            // all path have a trailing / as per the current implementation
            // todo: (sebros) find a way to 'use' the current implementation in DefaultContentDisplay.BindPlacement instead of emulating it

            var rules = new[] {
                Tuple.Create("~/my-blog", "~/my-blog/", true),
                Tuple.Create("~/my-blog/", "~/my-blog/", true),

                // star match
                Tuple.Create("~/my-blog*", "~/my-blog/", true),
                Tuple.Create("~/my-blog*", "~/my-blog/my-post/", true),
                Tuple.Create("~/my-blog/*", "~/my-blog/", true),
                Tuple.Create("~/my-blog/*", "~/my-blog123/", false),
                Tuple.Create("~/my-blog*", "~/my-blog123/", true)
            };

            foreach (var rule in rules)
            {
                var path = rule.Item1;
                var context = rule.Item2;
                var match = rule.Item3;

                _serviceProvider.GetService<TestShapeProvider>().Discover =
                    builder => builder.Describe("Hello").From(TestFeature())
                                   .Placement(ctx => true, new PlacementInfo { Location = "Match" });

                var manager = _serviceProvider.GetService<IShapeTableManager>();
                var hello = manager.GetShapeTable(null).Descriptors["Hello"];
                //var result = hello.Placement(new ShapePlacementContext { Path = context });

                //if (match)
                //{
                //    Assert.Equal("Match", result.Location);
                //}
                //else
                //{
                //    Assert.Null(result.Location);
                //}
            }
        }

        [Fact]
        public void OnlyShapesFromTheGivenThemeAreProvided()
        {
            _serviceProvider.GetService<TestShapeProvider>();
            var manager = _serviceProvider.GetService<IShapeTableManager>();
            var table = manager.GetShapeTable("Theme1");
            Assert.True(table.Descriptors.ContainsKey("Theme1Shape"));
            Assert.False(table.Descriptors.ContainsKey("DerivedShape"));
            Assert.False(table.Descriptors.ContainsKey("BaseShape"));
        }

        [Fact]
        public void ShapesFromTheBaseThemeAreProvided()
        {
            _serviceProvider.GetService<TestShapeProvider>();
            var manager = _serviceProvider.GetService<IShapeTableManager>();
            var table = manager.GetShapeTable("DerivedTheme");
            Assert.False(table.Descriptors.ContainsKey("Theme1Shape"));
            Assert.True(table.Descriptors.ContainsKey("DerivedShape"));
            Assert.True(table.Descriptors.ContainsKey("BaseShape"));
        }

        [Fact]
        public void DerivedThemesCanOverrideBaseThemeShapeBindings()
        {
            _serviceProvider.GetService<TestShapeProvider>();
            var manager = _serviceProvider.GetService<IShapeTableManager>();
            var table = manager.GetShapeTable("DerivedTheme");
            Assert.True(table.Bindings.TryGetValue("OverriddenShape", out _));
            Assert.Equal("DerivedTheme", table.Descriptors["OverriddenShape"].BindingSource);
        }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
        {
            (_serviceProvider as IDisposable)?.Dispose();
        }
    }
}

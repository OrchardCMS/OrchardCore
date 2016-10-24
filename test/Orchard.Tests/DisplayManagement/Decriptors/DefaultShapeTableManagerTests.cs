//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Orchard.DisplayManagement.Descriptors;
//using Orchard.DisplayManagement.Descriptors.ShapePlacementStrategy;
//using Orchard.DisplayManagement.Implementation;
//using Orchard.DisplayManagement.Shapes;
//using Orchard.Environment.Extensions;
//using Orchard.Environment.Extensions.Features;
//using Orchard.Environment.Extensions.Models;
//using Orchard.Events;
//using Orchard.Tests.Stubs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Threading.Tasks;
//using Xunit;

//namespace Orchard.Tests.DisplayManagement.Decriptors
//{
//    public class DefaultShapeTableManagerTests
//    {
//        IServiceProvider _serviceProvider;

//        public class StubEventBus : IEventBus
//        {
//            public Task NotifyAsync(string message, IDictionary<string, object> arguments)
//            {
//                return null;
//            }

//            public Task NotifyAsync<TEventHandler>(Expression<Func<TEventHandler, Task>> eventNotifier) where TEventHandler : IEventHandler
//            {
//                return null;
//            }

//            public void Subscribe(string message, Func<IServiceProvider, IDictionary<string, object>, Task> action)
//            {
//            }
//        }

//        public DefaultShapeTableManagerTests()
//        {
//            IServiceCollection serviceCollection = new ServiceCollection();

//            serviceCollection.AddLogging();
//            serviceCollection.AddMemoryCache();
//            serviceCollection.AddScoped<IFeatureManager, StubFeatureManager>();
//            serviceCollection.AddScoped<IShapeTableManager, DefaultShapeTableManager>();
//            serviceCollection.AddScoped<IEventBus, StubEventBus>();
//            serviceCollection.AddSingleton<ITypeFeatureProvider, TypeFeatureProvider>();

//            var features = new[] {
//                new FeatureDescriptor {
//                    Id = "Theme1",
//                    Extension = new ExtensionDescriptor {
//                        Id = "Theme1",
//                        ExtensionType = DefaultExtensionTypes.Theme
//                    }
//                },
//                new FeatureDescriptor {
//                    Id = "DerivedTheme",
//                    Extension = new ExtensionDescriptor {
//                        Id = "DerivedTheme",
//                        ExtensionType = DefaultExtensionTypes.Theme,
//                        BaseTheme = "BaseTheme"
//                    }
//                },
//                new FeatureDescriptor {
//                    Id = "BaseTheme",
//                    Extension = new ExtensionDescriptor {
//                        Id = "BaseTheme",
//                        ExtensionType = DefaultExtensionTypes.Theme
//                    }
//                }
//            };
//            serviceCollection.AddSingleton<IExtensionManager>(new TestExtensionManager(features));

//            TestShapeProvider.FeatureShapes = new Dictionary<Feature, IEnumerable<string>> {
//                { TestFeature(), new [] {"Hello"} },
//                { Feature(features[0]), new [] {"Theme1Shape"} },
//                { Feature(features[1]), new [] {"DerivedShape", "OverriddenShape"} },
//                { Feature(features[2]), new [] {"BaseShape", "OverriddenShape"} }
//            };

//            serviceCollection.AddScoped<IShapeTableProvider, TestShapeProvider>();
//            serviceCollection.AddScoped<TestShapeProvider>((x => (TestShapeProvider)x.GetService<IShapeTableProvider>()));

//            _serviceProvider = serviceCollection.BuildServiceProvider();
//        }

//        static Feature Feature(FeatureDescriptor descriptor)
//        {
//            return new Feature
//            {
//                Descriptor = descriptor
//            };
//        }

//        static Feature TestFeature()
//        {
//            return new Feature
//            {
//                Descriptor = new FeatureDescriptor
//                {
//                    Id = "Testing",
//                    Dependencies = Enumerable.Empty<string>(),
//                    Extension = new ExtensionDescriptor
//                    {
//                        Id = "Testing",
//                        ExtensionType = DefaultExtensionTypes.Module,
//                    }
//                }
//            };
//        }

//        public class TestExtensionManager : IExtensionManager
//        {
//            private readonly IEnumerable<FeatureDescriptor> _availableFeautures;

//            public TestExtensionManager(IEnumerable<FeatureDescriptor> availableFeautures)
//            {
//                _availableFeautures = availableFeautures;
//            }

//            public ExtensionDescriptor GetExtension(string name)
//            {
//                throw new NotImplementedException();
//            }

//            public IEnumerable<ExtensionDescriptor> AvailableExtensions()
//            {
//                throw new NotSupportedException();
//            }

//            public IEnumerable<FeatureDescriptor> AvailableFeatures()
//            {
//                return _availableFeautures;
//            }

//            public IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors)
//            {
//                throw new NotSupportedException();
//            }

//            public bool HasDependency(FeatureDescriptor item, FeatureDescriptor subject)
//            {
//                return _availableFeautures.Any(x => x.Id == item.Id);
//            }

//            public ExtensionEntry LoadExtension(ExtensionDescriptor descriptor)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public class TestShapeProvider : IShapeTableProvider
//        {
//            public static IDictionary<Feature, IEnumerable<string>> FeatureShapes;

//            public Action<ShapeTableBuilder> Discover = x => { };

//            void IShapeTableProvider.Discover(ShapeTableBuilder builder)
//            {
//                foreach (var pair in FeatureShapes)
//                {
//                    foreach (var shape in pair.Value)
//                    {
//                        builder.Describe(shape).From(pair.Key).BoundAs(pair.Key.Descriptor.Id, null);
//                    }
//                }

//                Discover(builder);
//            }
//        }

//        [Fact]
//        public void ManagerCanBeResolved()
//        {
//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            Assert.NotNull(manager);
//        }

//        [Fact]
//        public void DefaultShapeTableIsReturnedForNullOrEmpty()
//        {
//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            var shapeTable1 = manager.GetShapeTable(null);
//            var shapeTable2 = manager.GetShapeTable(string.Empty);
//            Assert.NotNull(shapeTable1.Descriptors["Hello"]);
//            Assert.NotNull(shapeTable2.Descriptors["Hello"]);
//        }

//        [Fact]
//        public void CallbackAlterationsContributeToDescriptor()
//        {
//            Action<ShapeCreatingContext> cb1 = x => { };
//            Action<ShapeCreatedContext> cb2 = x => { };
//            Action<ShapeDisplayingContext> cb3 = x => { };
//            Action<ShapeDisplayedContext> cb4 = x => { };

//            _serviceProvider.GetService<TestShapeProvider>().Discover =
//                builder => builder.Describe("Foo").From(TestFeature())
//                               .OnCreating(cb1)
//                               .OnCreated(cb2)
//                               .OnDisplaying(cb3)
//                               .OnDisplayed(cb4);

//            var manager = _serviceProvider.GetService<IShapeTableManager>();

//            var foo = manager.GetShapeTable(null).Descriptors["Foo"];

//            Assert.Same(cb1, foo.Creating.Single());
//            Assert.Same(cb2, foo.Created.Single());
//            Assert.Same(cb3, foo.Displaying.Single());
//            Assert.Same(cb4, foo.Displayed.Single());
//        }
//        [Fact]
//        public void DefaultPlacementIsReturnedByDefault()
//        {
//            var manager = _serviceProvider.GetService<IShapeTableManager>();

//            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
//            hello.DefaultPlacement = "Header:5";
//            var result = hello.Placement(null);
//            Assert.Equal("Header:5", result.Location);
//        }

//        [Fact]
//        public void DescribedPlacementIsReturnedIfNotNull()
//        {
//            var shapeDetail = new Shape { Metadata = new ShapeMetadata { DisplayType = "Detail" } };
//            var shapeSummary = new Shape { Metadata = new ShapeMetadata { DisplayType = "Summary" } };
//            var shapeTile = new Shape { Metadata = new ShapeMetadata { DisplayType = "Tile" } };

//            _serviceProvider.GetService<TestShapeProvider>().Discover =
//                builder => builder.Describe("Hello").From(TestFeature())
//                    .Placement(ctx => ctx.Shape.Metadata.DisplayType == "Detail" ? new PlacementInfo { Location = "Main" } : null)
//                    .Placement(ctx => ctx.Shape.Metadata.DisplayType == "Summary" ? new PlacementInfo { Location = "" } : null);

//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
//            var result1 = hello.Placement(new ShapePlacementContext { Shape = shapeDetail });
//            var result2 = hello.Placement(new ShapePlacementContext { Shape = shapeSummary });
//            var result3 = hello.Placement(new ShapePlacementContext { Shape = shapeTile });
//            hello.DefaultPlacement = "Header:5";
//            var result4 = hello.Placement(new ShapePlacementContext { Shape = shapeDetail });
//            var result5 = hello.Placement(new ShapePlacementContext { Shape = shapeSummary });
//            var result6 = hello.Placement(new ShapePlacementContext { Shape = shapeTile });

//            Assert.Equal("Main", result1.Location);
//            Assert.Empty(result2.Location);
//            Assert.Null(result3);
//            Assert.Equal("Main", result4.Location);
//            Assert.Empty(result5.Location);
//            Assert.Equal("Header:5", result6.Location);
//        }

//        [Fact]
//        public void TwoArgumentVariationDoesSameThing()
//        {
//            var shapeDetail = new Shape { Metadata = new ShapeMetadata { DisplayType = "Detail" } };
//            var shapeSummary = new Shape { Metadata = new ShapeMetadata { DisplayType = "Summary" } };
//            var shapeTile = new Shape { Metadata = new ShapeMetadata { DisplayType = "Tile" } };

//            _serviceProvider.GetService<TestShapeProvider>().Discover =
//                builder => builder.Describe("Hello").From(TestFeature())
//                    .Placement(ctx => ctx.Shape.Metadata.DisplayType == "Detail", new PlacementInfo { Location = "Main" })
//                    .Placement(ctx => ctx.Shape.Metadata.DisplayType == "Summary", new PlacementInfo { Location = "" });

//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            var hello = manager.GetShapeTable(null).Descriptors["Hello"];
//            var result1 = hello.Placement(new ShapePlacementContext { Shape = shapeDetail });
//            var result2 = hello.Placement(new ShapePlacementContext { Shape = shapeSummary });
//            var result3 = hello.Placement(new ShapePlacementContext { Shape = shapeTile });
//            hello.DefaultPlacement = "Header:5";
//            var result4 = hello.Placement(new ShapePlacementContext { Shape = shapeDetail });
//            var result5 = hello.Placement(new ShapePlacementContext { Shape = shapeSummary });
//            var result6 = hello.Placement(new ShapePlacementContext { Shape = shapeTile });

//            Assert.Equal("Main", result1.Location);
//            Assert.Empty(result2.Location);
//            Assert.Null(result3);
//            Assert.Equal("Main", result4.Location);
//            Assert.Empty(result5.Location);
//            Assert.Equal("Header:5", result6.Location);
//        }

//        [Fact(Skip = "Path not supported yet")]
//        public void PathConstraintShouldMatch()
//        {
//            // all path have a trailing / as per the current implementation
//            // todo: (sebros) find a way to 'use' the current implementation in DefaultContentDisplay.BindPlacement instead of emulating it

//            var rules = new[] {
//                Tuple.Create("~/my-blog", "~/my-blog/", true),
//                Tuple.Create("~/my-blog/", "~/my-blog/", true),

//                // star match
//                Tuple.Create("~/my-blog*", "~/my-blog/", true),
//                Tuple.Create("~/my-blog*", "~/my-blog/my-post/", true),
//                Tuple.Create("~/my-blog/*", "~/my-blog/", true),
//                Tuple.Create("~/my-blog/*", "~/my-blog123/", false),
//                Tuple.Create("~/my-blog*", "~/my-blog123/", true)
//            };

//            foreach (var rule in rules)
//            {
//                var path = rule.Item1;
//                var context = rule.Item2;
//                var match = rule.Item3;

//                _serviceProvider.GetService<TestShapeProvider>().Discover =
//                    builder => builder.Describe("Hello").From(TestFeature())
//                                   .Placement(ctx => true, new PlacementInfo { Location = "Match" });

//                var manager = _serviceProvider.GetService<IShapeTableManager>();
//                var hello = manager.GetShapeTable(null).Descriptors["Hello"];
//                //var result = hello.Placement(new ShapePlacementContext { Path = context });

//                //if (match)
//                //{
//                //    Assert.Equal("Match", result.Location);
//                //}
//                //else
//                //{
//                //    Assert.Null(result.Location);
//                //}
//            }
//        }

//        [Fact]
//        public void OnlyShapesFromTheGivenThemeAreProvided()
//        {
//            _serviceProvider.GetService<TestShapeProvider>();
//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            var table = manager.GetShapeTable("Theme1");
//            Assert.True(table.Descriptors.ContainsKey("Theme1Shape"));
//            Assert.False(table.Descriptors.ContainsKey("DerivedShape"));
//            Assert.False(table.Descriptors.ContainsKey("BaseShape"));
//        }

//        [Fact]
//        public void ShapesFromTheBaseThemeAreProvided()
//        {
//            _serviceProvider.GetService<TestShapeProvider>();
//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            var table = manager.GetShapeTable("DerivedTheme");
//            Assert.False(table.Descriptors.ContainsKey("Theme1Shape"));
//            Assert.True(table.Descriptors.ContainsKey("DerivedShape"));
//            Assert.True(table.Descriptors.ContainsKey("BaseShape"));
//        }

//        [Fact]
//        public void DerivedThemesCanOverrideBaseThemeShapeBindings()
//        {
//            _serviceProvider.GetService<TestShapeProvider>();
//            var manager = _serviceProvider.GetService<IShapeTableManager>();
//            var table = manager.GetShapeTable("DerivedTheme");
//            Assert.True(table.Bindings.ContainsKey("OverriddenShape"));
//            Assert.StrictEqual("DerivedTheme", table.Descriptors["OverriddenShape"].BindingSource);
//        }
//    }
//}
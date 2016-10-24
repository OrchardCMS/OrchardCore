//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;
//using Orchard.Tests.Hosting.Environment.Extensions.ExtensionTypes;
//using System.Reflection;
//using System.Globalization;
//using Orchard.Environment.Extensions;
//using Orchard.Environment.Extensions.Loaders;

//namespace Orchard.Tests.Hosting.Environment.Extensions
//{
//    public class ExtensionManagerTests
//    {
//        [Fact]
//        public void AvailableExtensionsShouldFollowCatalogLocations()
//        {
//            var folders = new StubExtensionLocator();
//            folders.Manifests.Add("foo", "Name: Foo");
//            folders.Manifests.Add("bar", "Name: Bar");
//            folders.Manifests.Add("frap", "Name: Frap");
//            folders.Manifests.Add("quad", "Name: Quad");

//            var manager = new ExtensionManager(folders, new[] { new StubLoaders() }, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());

//            var available = manager.GetExtensions();
        
//            Assert.Equal(4, available.Count());
//            Assert.True(available.Any(x => x.Id == "foo"));
//        }

//        [Fact]
//        public void ExtensionDescriptorKeywordsAreCaseInsensitive()
//        {
//            var folders = new StubExtensionLocator();

//            folders.Manifests.Add("Sample", @"
//NaMe: Sample Extension
//SESSIONSTATE: disabled
//version: 2.x
//DESCRIPTION: HELLO
//");
//            var manager = new ExtensionManager(folders, new[] { new StubLoaders() }, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());

//            var descriptor = manager.AvailableExtensions().Single();
//            Assert.Equal("Sample", descriptor.Id);
//            Assert.Equal("Sample Extension", descriptor.Name);
//            Assert.Equal("2.x", descriptor.Version);
//            Assert.Equal("HELLO", descriptor.Description);
//            Assert.Equal("disabled", descriptor.SessionState);
//        }

//        [Fact]
//        public void ExtensionDescriptorsShouldHaveNameAndVersion()
//        {
//            var folders = new StubExtensionLocator();

//            folders.Manifests.Add("Sample", @"
//Name: Sample Extension
//Version: 2.x
//");

//            var manager = new ExtensionManager(folders, new[] { new StubLoaders() }, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());

//            var descriptor = manager.AvailableExtensions().Single();
//            Assert.Equal("Sample", descriptor.Id);
//            Assert.Equal("Sample Extension", descriptor.Name);
//            Assert.Equal("2.x", descriptor.Version);
//        }

//        [Fact]
//        public void ExtensionDescriptorsShouldBeParsedForMinimalModuleTxt()
//        {
//            var folders = new StubExtensionLocator();

//            folders.Manifests.Add("SuperWiki", @"
//Name: SuperWiki
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    SuperWiki:
//        Description: My super wiki module for Orchard.
//");
//            var manager = new ExtensionManager(folders, new[] { new StubLoaders() }, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());

//            var descriptor = manager.AvailableExtensions().Single();
//            Assert.Equal("SuperWiki", descriptor.Id);
//            Assert.Equal("1.0.3", descriptor.Version);
//            Assert.Equal("1", descriptor.OrchardVersion);
//            Assert.Equal(1, descriptor.Features.Count());
//            Assert.Equal("SuperWiki", descriptor.Features.First().Id);
//            Assert.Equal("SuperWiki", descriptor.Features.First().Extension.Id);
//            Assert.Equal("My super wiki module for Orchard.", descriptor.Features.First().Description);
//        }

//        [Fact]
//        public void ExtensionDescriptorsShouldBeParsedForCompleteModuleTxt()
//        {
//            var folders = new StubExtensionLocator();

//            folders.Manifests.Add("MyCompany.AnotherWiki", @"
//Name: AnotherWiki
//SessionState: required
//Author: Coder Notaprogrammer
//Website: http://anotherwiki.codeplex.com
//Version: 1.2.3
//OrchardVersion: 1
//Features:
//    AnotherWiki:
//        Description: My super wiki module for Orchard.
//        Dependencies: Versioning, Search
//        Category: Content types
//    AnotherWiki Editor:
//        Description: A rich editor for wiki contents.
//        Dependencies: TinyMCE, AnotherWiki
//        Category: Input methods
//    AnotherWiki DistributionList:
//        Description: Sends e-mail alerts when wiki contents gets published.
//        Dependencies: AnotherWiki, Email Subscriptions
//        Category: Email
//    AnotherWiki Captcha:
//        Description: Kills spam. Or makes it zombie-like.
//        Dependencies: AnotherWiki, reCaptcha
//        Category: Spam
//");
//            var manager = new ExtensionManager(folders, new[] { new StubLoaders() }, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());

//            var descriptor = manager.AvailableExtensions().Single();
//            Assert.Equal("MyCompany.AnotherWiki", descriptor.Id);
//            Assert.Equal("AnotherWiki", descriptor.Name);
//            Assert.Equal("Coder Notaprogrammer", descriptor.Author);
//            Assert.Equal("http://anotherwiki.codeplex.com", descriptor.WebSite);
//            Assert.Equal("1.2.3", descriptor.Version);
//            Assert.Equal("1", descriptor.OrchardVersion);
//            Assert.Equal(5, descriptor.Features.Count());
//            Assert.Equal("required", descriptor.SessionState);
//            foreach (var featureDescriptor in descriptor.Features)
//            {
//                switch (featureDescriptor.Id)
//                {
//                    case "AnotherWiki":
//                        Assert.Same(descriptor, featureDescriptor.Extension);
//                        Assert.Equal("My super wiki module for Orchard.", featureDescriptor.Description);
//                        Assert.Equal("Content types", featureDescriptor.Category);
//                        Assert.Equal(2, featureDescriptor.Dependencies.Count());
//                        Assert.Contains("Versioning", featureDescriptor.Dependencies);
//                        Assert.Contains("Search", featureDescriptor.Dependencies);
//                        break;
//                    case "AnotherWiki Editor":
//                        Assert.Same(descriptor, featureDescriptor.Extension);
//                        Assert.Equal("A rich editor for wiki contents.", featureDescriptor.Description);
//                        Assert.Equal("Input methods", featureDescriptor.Category);
//                        Assert.Equal(2, featureDescriptor.Dependencies.Count());
//                        Assert.Contains("TinyMCE", featureDescriptor.Dependencies);
//                        Assert.Contains("AnotherWiki", featureDescriptor.Dependencies);
//                        break;
//                    case "AnotherWiki DistributionList":
//                        Assert.Same(descriptor, featureDescriptor.Extension);
//                        Assert.Equal("Sends e-mail alerts when wiki contents gets published.", featureDescriptor.Description);
//                        Assert.Equal("Email", featureDescriptor.Category);
//                        Assert.Equal(2, featureDescriptor.Dependencies.Count());
//                        Assert.Contains("AnotherWiki", featureDescriptor.Dependencies);
//                        Assert.Contains("Email Subscriptions", featureDescriptor.Dependencies);
//                        break;
//                    case "AnotherWiki Captcha":
//                        Assert.Same(descriptor, featureDescriptor.Extension);
//                        Assert.Equal("Kills spam. Or makes it zombie-like.", featureDescriptor.Description);
//                        Assert.Equal("Spam", featureDescriptor.Category);
//                        Assert.Equal(2, featureDescriptor.Dependencies.Count());
//                        Assert.Contains("AnotherWiki", featureDescriptor.Dependencies);
//                        Assert.Contains("reCaptcha", featureDescriptor.Dependencies);
//                        break;
//                    // default feature.
//                    case "MyCompany.AnotherWiki":
//                        Assert.Same(descriptor, featureDescriptor.Extension);
//                        break;
//                    default:
//                        throw new Exception("Features not parsed correctly");
//                }
//            }
//        }

//        [Fact]
//        public void ExtensionManagerShouldLoadFeatures()
//        {
//            var extensionLoader = new StubLoaders();
//            var folders = new StubExtensionLocator();

//            folders.Manifests.Add("TestModule", @"
//Name: TestModule
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    TestModule:
//        Description: My test module for Orchard.
//    TestFeature:
//        Description: Contains the Phi type.
//");

//            IExtensionManager extensionManager = CreateExtensionManager(folders, extensionLoader);
//            var testFeature = extensionManager.AvailableExtensions()
//                .SelectMany(x => x.Features);

//            var features = extensionManager.LoadFeatures(testFeature);
//            var types = features.SelectMany(x => x.ExportedTypes);

//            Assert.NotEqual(0, types.Count());
//        }

//        [Fact]
//        public void ExtensionManagerFeaturesContainNonAbstractClasses()
//        {
//            var extensionLoader = new StubLoaders();
//            var folders = new StubExtensionLocator();

//            folders.Manifests.Add("TestModule", @"
//Name: TestModule
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    TestModule:
//        Description: My test module for Orchard.
//    TestFeature:
//        Description: Contains the Phi type.
//");

//            IExtensionManager extensionManager = CreateExtensionManager(folders, extensionLoader);
//            var testFeature = extensionManager.AvailableExtensions()
//                .SelectMany(x => x.Features);

//            var features = extensionManager.LoadFeatures(testFeature);
//            var types = features.SelectMany(x => x.ExportedTypes);

//            foreach (var type in types)
//            {
//                Assert.True(type.GetTypeInfo().IsClass);
//                Assert.True(!type.GetTypeInfo().IsAbstract);
//            }
//        }

//        [Fact]
//        public void ExtensionManagerShouldReturnEmptyFeatureIfFeatureDoesNotExist()
//        {
//            var featureDescriptor = new FeatureDescriptor { Id = "NoSuchFeature", Extension = new ExtensionDescriptor { Id = "NoSuchFeature" } };
//            var manager = new ExtensionManager(new StubExtensionLocator(), new StubLoaders[] { }, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());

//            Feature feature = manager.LoadFeatures(new[] { featureDescriptor }).First();
//            Assert.Equal(featureDescriptor, feature.Descriptor);
//            Assert.Equal(0, feature.ExportedTypes.Count());
//        }

//        [Fact]
//        public void ExtensionManagerTestFeatureAttribute()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            extensionFolder.Manifests.Add("TestModule", @"
//Name: TestModule
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    TestModule:
//        Description: My test module for Orchard.
//    TestFeature:
//        Description: Contains the Phi type.
//");

//            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
//            var testFeature = extensionManager.AvailableExtensions()
//                .SelectMany(x => x.Features)
//                .Single(x => x.Id == "TestFeature");

//            foreach (var feature in extensionManager.LoadFeatures(new[] { testFeature }))
//            {
//                foreach (var type in feature.ExportedTypes)
//                {
//                    foreach (OrchardFeatureAttribute featureAttribute in type.GetTypeInfo().GetCustomAttributes(typeof(OrchardFeatureAttribute), false))
//                    {
//                        Assert.Equal("TestFeature", featureAttribute.FeatureName);
//                    }
//                }
//            }
//        }


//        [Fact]
//        public void ExtensionManagerLoadFeatureReturnsTypesFromSpecificFeaturesWithFeatureAttribute()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            extensionFolder.Manifests.Add("TestModule", @"
//Name: TestModule
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    TestModule:
//        Description: My test module for Orchard.
//    TestFeature:
//        Description: Contains the Phi type.
//");

//            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
//            var testFeature = extensionManager.AvailableExtensions()
//                .SelectMany(x => x.Features)
//                .Single(x => x.Id == "TestFeature");

//            foreach (var feature in extensionManager.LoadFeatures(new[] { testFeature }))
//            {
//                foreach (var type in feature.ExportedTypes)
//                {
//                    Assert.True(type == typeof(Phi));
//                }
//            }
//        }

//        [Fact]
//        public void ExtensionManagerLoadFeatureDoesNotReturnTypesFromNonMatchingFeatures()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            extensionFolder.Manifests.Add("TestModule", @"
//Name: TestModule
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    TestModule:
//        Description: My test module for Orchard.
//    TestFeature:
//        Description: Contains the Phi type.
//");

//            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
//            var testModule = extensionManager.AvailableExtensions()
//                .SelectMany(x => x.Features)
//                .Single(x => x.Id == "TestModule");

//            foreach (var feature in extensionManager.LoadFeatures(new[] { testModule }))
//            {
//                foreach (var type in feature.ExportedTypes)
//                {
//                    Assert.IsNotType(typeof(Phi), type);
//                    Assert.True((type == typeof(Alpha) || (type == typeof(Beta))));
//                }
//            }
//        }

//        [Fact]
//        public void ModuleNameIsIntroducedAsFeatureImplicitly()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            extensionFolder.Manifests.Add("Minimalistic", @"
//Name: Minimalistic
//Version: 1.0.3
//OrchardVersion: 1
//");

//            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
//            var minimalisticModule = extensionManager.AvailableExtensions().Single(x => x.Id == "Minimalistic");

//            Assert.Equal(1, minimalisticModule.Features.Count());
//            Assert.Equal("Minimalistic", minimalisticModule.Features.Single().Id);
//        }


//        [Fact]
//        public void FeatureDescriptorsAreInDependencyOrder()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            extensionFolder.Manifests.Add("Alpha", @"
//Name: Alpha
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    Alpha:
//        Dependencies: Gamma
//");

//            extensionFolder.Manifests.Add("Beta", @"
//Name: Beta
//Version: 1.0.3
//OrchardVersion: 1
//");
//            extensionFolder.Manifests.Add("Gamma", @"
//Name: Gamma
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    Gamma:
//        Dependencies: Beta
//");

//            IExtensionManager extensionManager = CreateExtensionManager(extensionFolder, extensionLoader);
//            var features = extensionManager.AvailableFeatures();
//            Assert.Equal("<Beta<Gamma<Alpha<", features.Aggregate("<", (a, b) => a + b.Id + "<"));
//        }

//        [Fact]
//        public void FeatureDescriptorsShouldBeLoadedInThemes()
//        {
//            var extensionLoader = new StubLoaders();
//            var moduleExtensionFolder = new StubExtensionLocator(new[] { DefaultExtensionTypes.Module, DefaultExtensionTypes.Theme });

//            moduleExtensionFolder.Manifests.Add("Alpha", @"
//Name: Alpha
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    Alpha:
//        Dependencies: Gamma
//");

//            moduleExtensionFolder.Manifests.Add("Beta", @"
//Name: Beta
//Version: 1.0.3
//OrchardVersion: 1
//");
//            moduleExtensionFolder.Manifests.Add("Gamma", @"
//Name: Gamma
//Version: 1.0.3
//OrchardVersion: 1
//Features:
//    Gamma:
//        Dependencies: Beta
//");

//            moduleExtensionFolder.ThemeManifests.Add("Classic", @"
//Name: Classic
//Version: 1.0.3
//OrchardVersion: 1
//");

//            IExtensionManager extensionManager = CreateExtensionManager(moduleExtensionFolder, new[] { extensionLoader });
//            var features = extensionManager.AvailableFeatures();
//            Assert.Equal(4, features.Count());
//        }



//        [Fact]
//        public void ThemeFeatureDescriptorsShouldBeAbleToDependOnModules()
//        {
//            var extensionLoader = new StubLoaders();
//            var moduleExtensionFolder = new StubExtensionLocator(new[] { DefaultExtensionTypes.Module, DefaultExtensionTypes.Theme });

//            moduleExtensionFolder.Manifests.Add("Alpha", CreateManifest("Alpha", null, "Gamma"));
//            moduleExtensionFolder.Manifests.Add("Beta", CreateManifest("Beta"));
//            moduleExtensionFolder.Manifests.Add("Gamma", CreateManifest("Gamma", null, "Beta"));
//            moduleExtensionFolder.ThemeManifests.Add("Classic", CreateManifest("Classic", null, "Alpha"));

//            AssertFeaturesAreInOrder(moduleExtensionFolder, extensionLoader, "<Beta<Gamma<Alpha<Classic<");
//        }

//        [Fact]
//        public void FeatureDescriptorsAreInDependencyAndPriorityOrder()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            // Check that priorities apply correctly on items on the same level of dependencies and are overwritten by dependencies
//            extensionFolder.Manifests.Add("Alpha", CreateManifest("Alpha", "2", "Gamma")); // More important than Gamma but will get overwritten by the dependency
//            extensionFolder.Manifests.Add("Beta", CreateManifest("Beta", "2"));
//            extensionFolder.Manifests.Add("Foo", CreateManifest("Foo", "1"));
//            extensionFolder.Manifests.Add("Gamma", CreateManifest("Gamma", "3", "Beta, Foo"));
//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Foo<Beta<Gamma<Alpha<");

//            // Change priorities and see that it reflects properly
//            // Gamma comes after Foo (same priority) because their order in the Manifests is preserved
//            extensionFolder.Manifests["Foo"] = CreateManifest("Foo", "3");
//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Beta<Foo<Gamma<Alpha<");

//            // Remove dependency on Foo and see that it moves down the list since no one depends on it anymore
//            extensionFolder.Manifests["Gamma"] = CreateManifest("Gamma", "3", "Beta");
//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Beta<Gamma<Alpha<Foo<");

//            // Change Foo to depend on Gamma and see that it says in its position (same dependencies as alpha but lower priority)
//            extensionFolder.Manifests["Foo"] = CreateManifest("Foo", "3", "Gamma");
//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Beta<Gamma<Alpha<Foo<");

//            // Update Foo to a higher priority than alpha and see that it moves before alpha
//            extensionFolder.Manifests["Foo"] = CreateManifest("Foo", "1", "Gamma");
//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Beta<Gamma<Foo<Alpha<");
//        }

//        [Fact]
//        public void FeatureDescriptorsAreInPriorityOrder()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            // Check that priorities apply correctly on items on the same level of dependencies and are overwritten by dependencies
//            extensionFolder.Manifests.Add("Alpha", CreateManifest("Alpha", "4")); // More important than Gamma but will get overwritten by the dependency
//            extensionFolder.Manifests.Add("Beta", CreateManifest("Beta", "3"));
//            extensionFolder.Manifests.Add("Foo", CreateManifest("Foo", "1"));
//            extensionFolder.Manifests.Add("Gamma", CreateManifest("Gamma", "2"));

//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Foo<Gamma<Beta<Alpha<");
//        }

//        [Fact]
//        public void FeatureDescriptorsAreInManifestOrderWhenTheyHaveEqualPriority()
//        {
//            var extensionLoader = new StubLoaders();
//            var extensionFolder = new StubExtensionLocator();

//            extensionFolder.Manifests.Add("Alpha", CreateManifest("Alpha", "4"));
//            extensionFolder.Manifests.Add("Beta", CreateManifest("Beta", "4"));
//            extensionFolder.Manifests.Add("Gamma", CreateManifest("Gamma", "4"));
//            extensionFolder.Manifests.Add("Foo", CreateManifest("Foo", "3"));
//            extensionFolder.Manifests.Add("Bar", CreateManifest("Bar", "3"));
//            extensionFolder.Manifests.Add("Baz", CreateManifest("Baz", "3"));

//            AssertFeaturesAreInOrder(extensionFolder, extensionLoader, "<Foo<Bar<Baz<Alpha<Beta<Gamma<");
//        }

//        private static string CreateManifest(string name, string priority = null, string dependencies = null)
//        {
//            return string.Format(CultureInfo.InvariantCulture, @"
//Name: {0}
//Version: 1.0.3
//OrchardVersion: 1{1}{2}",
//             name,
//             (dependencies == null ? null : "\nDependencies: " + dependencies),
//             (priority == null ? null : "\nPriority:" + priority));
//        }

//        private static void AssertFeaturesAreInOrder(StubExtensionLocator folder, StubLoaders loader, string expectedOrder)
//        {
//            var extensionManager = CreateExtensionManager(folder, new[] { loader });
//            var features = extensionManager.AvailableFeatures();
//            Assert.Equal(expectedOrder, features.Aggregate("<", (a, b) => a + b.Id + "<"));
//        }

//        private static ExtensionManager CreateExtensionManager(StubExtensionLocator loader, StubLoaders extensionLoader)
//        {
//            return CreateExtensionManager(loader, new[] { extensionLoader });
//        }

//        private static ExtensionManager CreateExtensionManager(StubExtensionLocator locator, IEnumerable<StubLoaders> extensionLoader)
//        {
//            return new ExtensionManager(locator, extensionLoader, new TypeFeatureProvider(), new NullLogger<ExtensionManager>());
//        }

//        public class StubExtensionLocator : IExtensionLocator
//        {
//            private readonly string[] _extensionTypes;

//            public StubExtensionLocator(string[] extensionTypes)
//            {
//                Manifests = new Dictionary<string, string>();
//                ThemeManifests = new Dictionary<string, string>();
//                _extensionTypes = extensionTypes;
//            }

//            public StubExtensionLocator()
//                : this(new[] { DefaultExtensionTypes.Module })
//            {
//            }

//            public IDictionary<string, string> Manifests { get; set; }
//            public IDictionary<string, string> ThemeManifests { get; set; }

//            public IEnumerable<ExtensionDescriptor> AvailableExtensions()
//            {
//                if (_extensionTypes.Contains(DefaultExtensionTypes.Module))
//                {
//                    foreach (var e in Manifests)
//                    {
//                        string name = e.Key;
//                        yield return ExtensionHarvester.GetDescriptorForExtension("~/", name, DefaultExtensionTypes.Module, Manifests[name]);
//                    }
//                }
//                if (_extensionTypes.Contains(DefaultExtensionTypes.Theme))
//                {
//                    foreach (var e in ThemeManifests)
//                    {
//                        string name = e.Key;
//                        yield return ExtensionHarvester.GetDescriptorForExtension("~/", name, DefaultExtensionTypes.Theme, ThemeManifests[name]);
//                    }
//                }
//            }
//        }


//        public class StubLoaders : IExtensionLoader
//        {
//            public string Name
//            {
//                get
//                {
//                    throw new NotImplementedException();
//                }
//            }

//            public int Order
//            {
//                get { return 1; }
//            }

//            public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
//            {
//                throw new NotImplementedException();
//            }

//            public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension)
//            {
//                throw new NotImplementedException();
//            }

//            public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references)
//            {
//                throw new NotImplementedException();
//            }

//            public ExtensionEntry Load(ExtensionDescriptor descriptor)
//            {
//                return new ExtensionEntry { Descriptor = descriptor, ExportedTypes = new[] { typeof(Alpha), typeof(Beta), typeof(Phi) } };
//            }

//            public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor)
//            {
//                return new ExtensionProbeEntry { Descriptor = descriptor, Loader = this };
//            }

//            public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
//            {
//                throw new NotImplementedException();
//            }

//            public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry)
//            {
//                throw new NotImplementedException();
//            }
//        }
//    }
//}
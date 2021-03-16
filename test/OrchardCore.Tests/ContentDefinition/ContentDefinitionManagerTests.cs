//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using OrchardCore.ContentManagement.Metadata;
//using OrchardCore.ContentManagement.Metadata;
//using OrchardCore.ContentManagement.Metadata.Builders;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;
//using YesSql;

//namespace OrchardCore.Tests.ContentDefinition
//{
//    public class ContentDefinitionManagerTests
//    {
//        IServiceProvider _serviceProvider;
//        ISession _session;
//        IContentDefinitionManager _contentDefinitionManager;

//        public ContentDefinitionManagerTests()
//        {
//            IServiceCollection serviceCollection = new ServiceCollection();

//            serviceCollection.AddScoped<ILoggerFactory, StubLoggerFactory>();
//            serviceCollection.AddScoped<ISession, >();

//            serviceCollection.AddScoped<IContentDefinitionManager, ContentDefinitionManager>();

//            _serviceProvider = serviceCollection.BuildServiceProvider();

//            _session = _serviceProvider.GetService<ISession>();
//            _contentDefinitionManager = _serviceProvider.GetService<IContentDefinitionManager>();
//        }

//        [Fact]
//        public void NoTypesAreAvailableByDefault()
//        {
//            var types = _serviceProvider.GetService<IContentDefinitionManager>().ListTypeDefinitions();
//            Assert.Equal(0, types.Count());
//        }

//        [Fact]
//        public void ContentTypesWithSettingsCanBeCreatedAndModified()
//        {
//            var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder()
//                                  .Named("alpha")
//                                  .WithSetting("a", "1")
//                                  .WithSetting("b", "2")
//                                  .Build());

//            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder()
//                                  .Named("beta")
//                                  .WithSetting("c", "3")
//                                  .WithSetting("d", "4")
//                                  .Build());

//            var types1 = manager.ListTypeDefinitions();
//            Assert.Equal(2, types1.Count());
//            var alpha1 = types1.Single(t => t.Name == "alpha");
//            Assert.Equal("1", alpha1.Settings["a"]);
//            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder(alpha1).WithSetting("a", "5").Build());

//            var types2 = manager.ListTypeDefinitions();
//            Assert.Equal(2, types2.Count());
//            var alpha2 = types2.Single(t => t.Name == "alpha");
//            Assert.Equal("5", alpha2.Settings["a"]);
//        }

//        [Fact]
//        public void StubPartDefinitionsAreCreatedWhenContentTypesAreStored()
//        {
//            var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//            manager.StoreTypeDefinition(new ContentTypeDefinitionBuilder()
//                                  .Named("alpha")
//                                  .WithPart("foo", pb => { })
//                                  .Build());

//            var foo = manager.GetPartDefinition("foo");
//            Assert.NotNull(foo);
//            Assert.Equal("foo", foo.Name);

//            var alpha = manager.GetTypeDefinition("alpha");
//            Assert.NotNull(alpha);
//            Assert.Equal(1, alpha.Parts.Count());
//            Assert.Equal("foo", alpha.Parts.Single().PartDefinition.Name);
//        }

//        [Fact]
//        public void GettingDefinitionsByNameCanReturnNullAndWillAcceptNullEmptyOrInvalidNames()
//        {
//            var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//            Assert.Null(manager.GetTypeDefinition("no such name"));
//            Assert.Null(manager.GetTypeDefinition(string.Empty));
//            Assert.Null(manager.GetTypeDefinition(null));
//            Assert.Null(manager.GetPartDefinition("no such name"));
//            Assert.Null(manager.GetPartDefinition(string.Empty));
//            Assert.Null(manager.GetPartDefinition(null));
//        }

//        //[Fact]
//        //public void PartsAreRemovedWhenNotReferencedButPartDefinitionRemains()
//        //{
//        //    var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//        //    manager.StoreTypeDefinition(
//        //        new ContentTypeDefinitionBuilder()
//        //            .Named("alpha")
//        //            .WithPart("foo", pb => { })
//        //            .WithPart("bar", pb => { })
//        //            .Build());

//        //    AssertThatTypeHasParts("alpha", "foo", "bar");
//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(2));
//        //    ResetSession();
//        //    AssertThatTypeHasParts("alpha", "foo", "bar");
//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(2));

//        //    manager.StoreTypeDefinition(
//        //        new ContentTypeDefinitionBuilder(manager.GetTypeDefinition("alpha"))
//        //            .WithPart("frap", pb => { })
//        //            .RemovePart("bar")
//        //            .Build());

//        //    AssertThatTypeHasParts("alpha", "foo", "frap");
//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(3));
//        //    ResetSession();
//        //    AssertThatTypeHasParts("alpha", "foo", "frap");
//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(3));
//        //}

//        //[Fact]
//        //public void PartsCanBeDeleted()
//        //{
//        //    var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//        //    manager.StoreTypeDefinition(
//        //        new ContentTypeDefinitionBuilder()
//        //            .Named("alpha")
//        //            .WithPart("foo", pb => { })
//        //            .WithPart("bar", pb => { })
//        //            .Build());

//        //    AssertThatTypeHasParts("alpha", "foo", "bar");
//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(2));

//        //    manager.DeletePartDefinition("foo");
//        //    ResetSession();

//        //    AssertThatTypeHasParts("alpha", "bar");
//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(1));
//        //}

//        //[Fact]
//        //public void ContentTypesCanBeDeleted()
//        //{
//        //    var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//        //    manager.StoreTypeDefinition(
//        //        new ContentTypeDefinitionBuilder()
//        //            .Named("alpha")
//        //            .WithPart("foo", pb => { })
//        //            .WithPart("bar", pb => { })
//        //            .Build());

//        //    Assert.That(manager.GetTypeDefinition("alpha"), Is.Not.Null);
//        //    manager.DeleteTypeDefinition("alpha");
//        //    ResetSession();

//        //    Assert.That(manager.GetTypeDefinition("alpha"), Is.Null);
//        //}

//        //[Fact]
//        //public void MultipleFieldsCanBeAddedToImplicitParts()
//        //{
//        //    var manager = _serviceProvider.GetService<IContentDefinitionManager>();
//        //    manager.StorePartDefinition(
//        //        new ContentPartDefinitionBuilder()
//        //            .Named("alpha")
//        //            .WithField("field1", f => f.OfType("TextField"))
//        //            .WithField("field2", f => f.OfType("TextField"))
//        //            .Build()
//        //        );

//        //    manager.StoreTypeDefinition(
//        //        new ContentTypeDefinitionBuilder()
//        //            .Named("alpha")
//        //            .WithPart("foo")
//        //            .Build()
//        //        );

//        //    ResetSession();

//        //    var types = manager.ListTypeDefinitions();
//        //    Assert.That(types.Count(), Is.EqualTo(1));
//        //    var parts = manager.ListPartDefinitions();
//        //    Assert.That(parts.Count(), Is.EqualTo(2));
//        //    var fields = manager.ListFieldDefinitions();
//        //    Assert.That(fields.Count(), Is.EqualTo(1));

//        //    var alpha = manager.GetTypeDefinition("alpha");
//        //    Assert.That(alpha.Parts.Count(), Is.EqualTo(1));

//        //    var part = manager.GetPartDefinition("alpha");
//        //    Assert.That(part.Fields.Count(), Is.EqualTo(2));

//        //    manager.AlterPartDefinition("alpha", p => p
//        //            .WithField("field3", f => f.OfType("TextField"))
//        //            .WithField("field4", f => f.OfType("TextField"))
//        //        );

//        //    ResetSession();

//        //    part = manager.GetPartDefinition("alpha");
//        //    Assert.That(part.Fields.Count(), Is.EqualTo(4));

//        //    alpha = manager.GetTypeDefinition("alpha");
//        //    Assert.That(alpha.Parts.Count(), Is.EqualTo(1));
//        //}

//        //[Fact]
//        //public void DontCreateMultiplePartsWhenAddingMultipleFields()
//        //{
//        //    var manager = _serviceProvider.GetService<IContentDefinitionManager>();

//        //    manager.AlterPartDefinition("alpha",
//        //            part => part
//        //                .WithField("StartDate", cfg => cfg
//        //                    .WithDisplayName("Start Date")
//        //                    .OfType("DateTimeField")
//        //                    .WithSetting("DateTimeFieldSettings.Display", "DateAndTime"))
//        //                .WithField("EndDate", cfg => cfg
//        //                    .WithDisplayName("End Date")
//        //                    .OfType("DateTimeField")
//        //                    .WithSetting("DateTimeFieldSettings.Display", "DateAndTime"))
//        //        );

//        //    Assert.That(manager.ListPartDefinitions().Count(), Is.EqualTo(1));

//        //    var p = manager.GetPartDefinition("alpha");
//        //    Assert.That(p.Fields.Count(), Is.EqualTo(2));
//        //}

//        //private void AssertThatTypeHasParts(string typeName, params string[] partNames)
//        //{
//        //    var type = _serviceProvider.GetService<IContentDefinitionManager>().GetTypeDefinition(typeName);
//        //    Assert.That(type, Is.Not.Null);
//        //    Assert.That(type.Parts.Count(), Is.EqualTo(partNames.Count()));
//        //    foreach (var partName in partNames)
//        //    {
//        //        Assert.That(type.Parts.Select(p => p.PartDefinition.Name), Has.Some.EqualTo(partName));
//        //    }
//        //}

//    }
//}

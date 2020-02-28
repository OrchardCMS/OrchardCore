using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using Xunit;
using YesSql;
using YesSql.Indexes;
using YesSql.Provider.Sqlite;
using YesSql.Sql;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class ContentItemsFieldTypeTests : IDisposable
    {
        protected IStore _store;
        protected IStore _prefixedStore;
        protected string _prefix;
        protected string _tempFilename;
        private Task _initializeTask;

        public ContentItemsFieldTypeTests()
        {
            _initializeTask = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var connectionStringTemplate = @"Data Source={0};Cache=Shared";

            _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            _store = await StoreFactory.CreateAsync(new Configuration().UseSqLite(String.Format(connectionStringTemplate, _tempFilename)));

            _prefix = "tp";
            _prefixedStore = await StoreFactory.CreateAsync(new Configuration().UseSqLite(String.Format(connectionStringTemplate, _tempFilename + _prefix)).SetTablePrefix(_prefix + "_"));

            await CreateTablesAsync(_store);
            await CreateTablesAsync(_prefixedStore);
        }

        public void Dispose()
        {
            _store.Dispose();
            _store = null;

            _prefixedStore.Dispose();
            _prefixedStore = null;

            if (File.Exists(_tempFilename))
            {
                File.Delete(_tempFilename);
            }

            var prefixFilename = _tempFilename + _prefix;

            if (File.Exists(prefixFilename))
            {
                File.Delete(prefixFilename);
            }
        }

        private async Task CreateTablesAsync(IStore store)
        {
            using (var session = store.CreateSession())
            {
                var builder = new SchemaBuilder(store.Configuration, await session.DemandAsync());

                builder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                    .Column<string>("ContentItemId", c => c.WithLength(26))
                    .Column<string>("ContentItemVersionId", c => c.WithLength(26))
                    .Column<bool>("Latest")
                    .Column<bool>("Published")
                    .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                    .Column<DateTime>("ModifiedUtc", column => column.Nullable())
                    .Column<DateTime>("PublishedUtc", column => column.Nullable())
                    .Column<DateTime>("CreatedUtc", column => column.Nullable())
                    .Column<string>("Owner", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
                    .Column<string>("Author", column => column.Nullable().WithLength(ContentItemIndex.MaxAuthorSize))
                    .Column<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
                );

                builder.CreateMapIndexTable(nameof(AnimalIndex), column => column
                    .Column<string>(nameof(AnimalIndex.Name))
                );

                builder.CreateMapIndexTable(nameof(AnimalTraitsIndex), column => column
                    .Column<bool>(nameof(AnimalTraitsIndex.IsHappy))
                    .Column<bool>(nameof(AnimalTraitsIndex.IsScary))
                );
            }

            store.RegisterIndexes<ContentItemIndexProvider>();
        }

        [Fact]
        public async Task ShouldFilterByContentItemIndex()
        {
            await _initializeTask;
            _store.RegisterIndexes<AnimalIndexProvider>();

            using (var services = new FakeServiceCollection())
            {
                services.Populate(new ServiceCollection());
                services.Services.AddScoped(x => _store.CreateSession());
                services.Services.AddScoped(x => new ShellSettings());
                services.Build();

                var returnType = new ListGraphType<StringGraphType>();
                returnType.ResolvedType = new StringGraphType() { Name = "Animal" };

                var animalWhereInput = new AnimalPartWhereInput();
                var inputs = new FieldType { Name = "Inputs", Arguments = new QueryArguments { new QueryArgument<WhereInputObjectGraphType> { Name = "where", Description = "filters the animals", ResolvedType = animalWhereInput } } };

                var context = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>(),
                    UserContext = new GraphQLContext
                    {
                        ServiceProvider = services
                    },
                    ReturnType = returnType,
                    FieldDefinition = inputs
                };

                var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
                ci.Weld(new AnimalPart { Name = "doug" });

                var session = ((GraphQLContext)context.UserContext).ServiceProvider.GetService<ISession>();
                session.Save(ci);
                await session.CommitAsync();

                var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

                context.Arguments["where"] = JObject.Parse("{ contentItemId: \"1\" }");
                var dogs = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(dogs);
                Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
            }
        }

        [Fact]
        public async Task ShouldFilterByContentItemIndexWhenSqlTablePrefixIsUsed()
        {
            await _initializeTask;
            _prefixedStore.RegisterIndexes<AnimalIndexProvider>();

            using (var services = new FakeServiceCollection())
            {
                services.Populate(new ServiceCollection());
                services.Services.AddScoped(x => _prefixedStore.CreateSession());

                var shellSettings = new ShellSettings();
                shellSettings["TablePrefix"] = _prefix;

                services.Services.AddScoped(x => shellSettings);
                services.Build();

                var returnType = new ListGraphType<StringGraphType>();
                returnType.ResolvedType = new StringGraphType() { Name = "Animal" };

                var animalWhereInput = new AnimalPartWhereInput();
                var inputs = new FieldType { Name = "Inputs", Arguments = new QueryArguments { new QueryArgument<WhereInputObjectGraphType> { Name = "where", Description = "filters the animals", ResolvedType = animalWhereInput } } };

                var context = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>(),
                    UserContext = new GraphQLContext
                    {
                        ServiceProvider = services
                    },
                    ReturnType = returnType,
                    FieldDefinition = inputs
                };

                var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
                ci.Weld(new AnimalPart { Name = "doug" });

                var session = ((GraphQLContext)context.UserContext).ServiceProvider.GetService<ISession>();
                session.Save(ci);
                await session.CommitAsync();

                var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

                context.Arguments["where"] = JObject.Parse("{ contentItemId: \"1\" }");
                var dogs = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(dogs);
                Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
            }
        }

        [Fact]
        public async Task ShouldBeAbleToUseTheSameIndexForMultipleAliases()
        {
            await _initializeTask;
            _store.RegisterIndexes<AnimalIndexProvider>();

            using (var services = new FakeServiceCollection())
            {
                services.Populate(new ServiceCollection());
                services.Services.AddScoped(x => new ShellSettings());
                services.Services.AddScoped(x => _store.CreateSession());
                services.Services.AddScoped<IIndexProvider, ContentItemIndexProvider>();
                services.Services.AddScoped<IIndexProvider, AnimalIndexProvider>();
                services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
                services.Build();

                var retrunType = new ListGraphType<StringGraphType>();
                retrunType.ResolvedType = new StringGraphType() { Name = "Animal" };

                var context = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>(),
                    UserContext = new GraphQLContext
                    {
                        ServiceProvider = services
                    },
                    ReturnType = retrunType
                };

                var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
                ci.Weld(new Animal { Name = "doug" });

                var session = ((GraphQLContext)context.UserContext).ServiceProvider.GetService<ISession>();
                session.Save(ci);
                await session.CommitAsync();

                var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

                context.Arguments["where"] = JObject.Parse("{ cats: { name: \"doug\" } }");
                var cats = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(cats);
                Assert.Equal("doug", cats.First().As<Animal>().Name);

                context.Arguments["where"] = JObject.Parse("{ dogs: { name: \"doug\" } }");
                var dogs = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(dogs);
                Assert.Equal("doug", dogs.First().As<Animal>().Name);
            }
        }

        [Fact]
        public async Task ShouldFilterOnMultipleIndexesOnSameAlias()
        {
            await _initializeTask;
            _store.RegisterIndexes<AnimalIndexProvider>();
            _store.RegisterIndexes<AnimalTraitsIndexProvider>();

            using (var services = new FakeServiceCollection())
            {
                services.Populate(new ServiceCollection());
                services.Services.AddScoped(x => new ShellSettings());
                services.Services.AddScoped(x => _store.CreateSession());
                services.Services.AddScoped<IIndexProvider, ContentItemIndexProvider>();
                services.Services.AddScoped<IIndexProvider, AnimalIndexProvider>();
                services.Services.AddScoped<IIndexProvider, AnimalTraitsIndexProvider>();
                services.Services.AddScoped<IIndexAliasProvider, MultipleIndexesIndexProvider>();
                services.Build();

                var retrunType = new ListGraphType<StringGraphType>();
                retrunType.ResolvedType = new StringGraphType() { Name = "Animal" };

                var context = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>(),
                    UserContext = new GraphQLContext
                    {
                        ServiceProvider = services
                    },
                    ReturnType = retrunType
                };

                var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
                ci.Weld(new Animal { Name = "doug", IsHappy = true, IsScary = false });

                var ci1 = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "2", ContentItemVersionId = "2" };
                ci1.Weld(new Animal { Name = "doug", IsHappy = false, IsScary = true });

                var ci2 = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "3", ContentItemVersionId = "3" };
                ci2.Weld(new Animal { Name = "tommy", IsHappy = false, IsScary = true });

                var session = ((GraphQLContext)context.UserContext).ServiceProvider.GetService<ISession>();
                session.Save(ci);
                session.Save(ci1);
                session.Save(ci2);
                await session.CommitAsync();

                var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

                context.Arguments["where"] = JObject.Parse("{ animals: { name: \"doug\", isScary: true } }");
                var animals = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(animals);
                Assert.Equal("doug", animals.First().As<Animal>().Name);
                Assert.True(animals.First().As<Animal>().IsScary);
                Assert.False(animals.First().As<Animal>().IsHappy);
            }
        }

        [Fact]
        public async Task ShouldFilterPartsWithoutAPrefixWhenThePartHasNoPrefix()
        {
            await _initializeTask;
            _store.RegisterIndexes<AnimalIndexProvider>();

            using (var services = new FakeServiceCollection())
            {
                services.Populate(new ServiceCollection());
                services.Services.AddScoped(x => _store.CreateSession());
                services.Services.AddScoped(x => new ShellSettings());
                services.Services.AddScoped<IIndexProvider, ContentItemIndexProvider>();
                services.Services.AddScoped<IIndexProvider, AnimalIndexProvider>();
                services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
                services.Build();

                var returnType = new ListGraphType<StringGraphType>();
                returnType.ResolvedType = new StringGraphType() { Name = "Animal" };

                var animalWhereInput = new AnimalPartWhereInput();
                var inputs = new FieldType { Name = "Inputs", Arguments = new QueryArguments { new QueryArgument<WhereInputObjectGraphType> { Name = "where", Description = "filters the animals", ResolvedType = animalWhereInput } } };

                var context = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>(),
                    UserContext = new GraphQLContext
                    {
                        ServiceProvider = services
                    },
                    ReturnType = returnType,
                    FieldDefinition = inputs
                };

                var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
                ci.Weld(new AnimalPart { Name = "doug" });

                var session = ((GraphQLContext)context.UserContext).ServiceProvider.GetService<ISession>();
                session.Save(ci);
                await session.CommitAsync();

                var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

                context.Arguments["where"] = JObject.Parse("{ animal: { name: \"doug\" } }");
                var dogs = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(dogs);
                Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
            }
        }

        [Fact]
        public async Task ShouldFilterByCollapsedWhereInputForCollapsedParts()
        {
            await _initializeTask;
            _store.RegisterIndexes<AnimalIndexProvider>();

            using (var services = new FakeServiceCollection())
            {
                services.Populate(new ServiceCollection());
                services.Services.AddScoped(x => new ShellSettings());
                services.Services.AddScoped(x => _store.CreateSession());
                services.Services.AddScoped<IIndexProvider, ContentItemIndexProvider>();
                services.Services.AddScoped<IIndexProvider, AnimalIndexProvider>();
                services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
                services.Build();

                var returnType = new ListGraphType<StringGraphType>();
                returnType.ResolvedType = new StringGraphType() { Name = "Animal" };

                var animalWhereInput = new AnimalPartCollapsedWhereInput();

                var context = new ResolveFieldContext
                {
                    Arguments = new Dictionary<string, object>(),
                    UserContext = new GraphQLContext
                    {
                        ServiceProvider = services
                    },
                    ReturnType = returnType,
                    FieldDefinition = new FieldType
                    {
                        Name = "Inputs",
                        Arguments = new QueryArguments
                            {
                                new QueryArgument<WhereInputObjectGraphType>
                                {
                                    Name = "where",
                                    Description = "filters the animals",
                                    ResolvedType = animalWhereInput
                                }
                            }
                    }
                };

                var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
                ci.Weld(new AnimalPart { Name = "doug" });

                var session = ((GraphQLContext)context.UserContext).ServiceProvider.GetService<ISession>();
                session.Save(ci);
                await session.CommitAsync();

                var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

                context.Arguments["where"] = JObject.Parse("{ name: \"doug\" }");
                var dogs = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

                Assert.Single(dogs);
                Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
            }
        }
    }

    public class AnimalPartWhereInput : WhereInputObjectGraphType
    {
        public AnimalPartWhereInput()
        {
            Name = "Test";
            Description = "Foo";
            AddField(new FieldType { Name = "Animal", Type = typeof(StringGraphType), Metadata = new Dictionary<string, object> { { "PartName", "AnimalPart" } } });
        }
    }

    public class AnimalPartCollapsedWhereInput : WhereInputObjectGraphType
    {
        public AnimalPartCollapsedWhereInput()
        {
            Name = "Test";
            Description = "Foo";
            AddField(new FieldType { Name = "Name", Type = typeof(StringGraphType), Metadata = new Dictionary<string, object> { { "PartName", "AnimalPart" }, { "PartCollapsed", true } } });
        }
    }

    public class Animal : ContentPart
    {
        public string Name { get; set; }
        public bool IsHappy { get; set; }
        public bool IsScary { get; set; }
    }

    public class AnimalPart : Animal { };

    public class AnimalIndex : MapIndex
    {
        public string Name { get; set; }
    }

    public class AnimalIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AnimalIndex>()
                .Map(contentItem =>
                {
                    return new AnimalIndex
                    {
                        Name = contentItem.As<Animal>() != null ?
                                contentItem.As<Animal>().Name
                                : contentItem.As<AnimalPart>().Name
                    };
                });
        }
    }

    public class AnimalTraitsIndex : MapIndex
    {
        public bool IsHappy { get; set; }
        public bool IsScary { get; set; }
    }

    public class AnimalTraitsIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AnimalTraitsIndex>()
                .Map(contentItem =>
                {
                    var animal = contentItem.As<Animal>();

                    if (animal != null)
                    {
                        return new AnimalTraitsIndex
                        {
                            IsHappy = contentItem.As<Animal>().IsHappy,
                            IsScary = contentItem.As<Animal>().IsScary
                        };
                    }

                    var animalPartSuffix = contentItem.As<AnimalPart>();

                    return new AnimalTraitsIndex
                    {
                        IsHappy = animalPartSuffix.IsHappy,
                        IsScary = animalPartSuffix.IsScary
                    };
                });
        }
    }

    public class MultipleAliasIndexProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "cats",
                Index = nameof(AnimalIndex),
                With = q => q.With<AnimalIndex>()
            },
            new IndexAlias
            {
                Alias = "dogs",
                Index = nameof(AnimalIndex),
                With = q => q.With<AnimalIndex>()
            },
            new IndexAlias
            {
                Alias = nameof(AnimalPart),
                Index = nameof(AnimalIndex),
                With = q => q.With<AnimalIndex>()
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }

    public class MultipleIndexesIndexProvider : IIndexAliasProvider
    {
        private static readonly IndexAlias[] _aliases = new[]
        {
            new IndexAlias
            {
                Alias = "animals.name",
                Index = $"Name",
                With = q => q.With<AnimalIndex>()
            },
            new IndexAlias
            {
                Alias = "animals.isHappy",
                Index = $"IsHappy",
                With = q => q.With<AnimalTraitsIndex>()
            },
            new IndexAlias
            {
                Alias = "animals.isScary",
                Index = $"IsScary",
                With = q => q.With<AnimalTraitsIndex>()
            }
        };

        public IEnumerable<IndexAlias> GetAliases()
        {
            return _aliases;
        }
    }

    public class FakeServiceCollection : IServiceProvider, IDisposable
    {
        private IServiceProvider _inner;
        private IServiceCollection _services;

        public IServiceCollection Services => _services;

        public string State { get; set; }

        public object GetService(Type serviceType)
        {
            return _inner.GetService(serviceType);
        }

        public void Populate(IServiceCollection services)
        {
            _services = services;
            _services.AddSingleton<FakeServiceCollection>(this);
        }

        public void Build()
        {
            _inner = _services.BuildServiceProvider();
        }

        public void Dispose()
        {
            (_inner as IDisposable)?.Dispose();
        }
    }
}

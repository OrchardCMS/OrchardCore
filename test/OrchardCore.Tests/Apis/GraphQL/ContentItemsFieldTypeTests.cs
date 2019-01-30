using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using Xunit;
using YesSql;
using YesSql.Indexes;
using YesSql.Sql;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class ContentItemsFieldTypeTests : IDisposable
    {
        protected Store _store;

        public ContentItemsFieldTypeTests()
        {
            _store = StoreFactory.CreateAsync(new Configuration()).GetAwaiter().GetResult() as Store;

            CreateTables();
        }

        public void Dispose()
        {
            _store.Dispose();
            _store = null;
        }

        private void CreateTables()
        {
            using (var session = _store.CreateSession())
            {
                var builder = new SchemaBuilder(_store.Configuration, session.DemandAsync().GetAwaiter().GetResult());

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

            _store.RegisterIndexes<ContentItemIndexProvider>();
        }


        [Fact]
        public async Task ShouldBeAbleToUseTheSameIndexForMultipleAliases()
        {
            _store.RegisterIndexes<AnimalIndexProvider>();

            var services = new FakeServiceCollection();
            services.Populate(new ServiceCollection());
            services.Services.AddScoped<ISession>(x => new Session(_store, System.Data.IsolationLevel.Unspecified));
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

            var type = new ContentItemsFieldType("Animal", new Schema());

            context.Arguments["where"] = JObject.Parse("{ cats: { name: \"doug\" } }");
            var cats = await ((AsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

            Assert.Single(cats);
            Assert.Equal("doug", cats.First().As<Animal>().Name);


            context.Arguments["where"] = JObject.Parse("{ dogs: { name: \"doug\" } }");
            var dogs = await ((AsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

            Assert.Single(dogs);
            Assert.Equal("doug", dogs.First().As<Animal>().Name);
        }

        [Fact]
        public async Task ShouldFilterOnMultipleIndexesOnSameAlias()
        {
            _store.RegisterIndexes<AnimalIndexProvider>();
            _store.RegisterIndexes<AnimalTraitsIndexProvider>();

            var services = new FakeServiceCollection();
            services.Populate(new ServiceCollection());
            services.Services.AddScoped<ISession>(x => new Session(_store, System.Data.IsolationLevel.Unspecified));
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

            var type = new ContentItemsFieldType("Animal", new Schema());

            context.Arguments["where"] = JObject.Parse("{ animals: { name: \"doug\", isScary: true } }");
            var animals = await ((AsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).Resolve(context);

            Assert.Single(animals);
            Assert.Equal("doug", animals.First().As<Animal>().Name);
            Assert.True(animals.First().As<Animal>().IsScary);
            Assert.False(animals.First().As<Animal>().IsHappy);
        }
    }

    public class Animal : ContentPart
    {
        public string Name { get; set; }
        public bool IsHappy { get; set; }
        public bool IsScary { get; set; }
    }

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
                        Name = contentItem.As<Animal>().Name
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
                    return new AnimalTraitsIndex
                    {
                        IsHappy = contentItem.As<Animal>().IsHappy,
                        IsScary = contentItem.As<Animal>().IsScary
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

    public class FakeServiceCollection : IServiceProvider
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
    }
}

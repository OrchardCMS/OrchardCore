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
using YesSql.Provider.Sqlite;
using YesSql.Sql;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class ContentItemsFieldTypeTests : IDisposable
    {
        protected Store _store;

        public ContentItemsFieldTypeTests() {
            _store = new Store(new Configuration().UseInMemory());

            CreateTables();
        }

        public void Dispose()
        {
            _store.Dispose();
            _store = null;
        }

        private void CreateTables()
        {
            // Create tables
            _store.InitializeAsync().Wait();

            using (var session = _store.CreateSession())
            {
                var builder = new SchemaBuilder(session);

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
            }
        }


        [Fact]
        public async Task ShouldBeAbleToUseTheSameIndexForMultipleAliases() {
            _store.RegisterIndexes<ContentItemIndexProvider>();
            _store.RegisterIndexes<MultipleIndexProvider>();

            var services = new FakeServiceCollection();
            services.Populate(new ServiceCollection());
            services.Services.AddScoped<ISession>(x => new Session(_store, System.Data.IsolationLevel.Unspecified));
            services.Services.AddScoped<IIndexProvider, ContentItemIndexProvider>();
            services.Services.AddScoped<IIndexProvider, MultipleIndexProvider>();
            services.Services.AddScoped<IIndexAliasProvider, MultipleIndexAliasProvider>();
            services.Build();

            var retrunType = new ListGraphType<StringGraphType>();
            retrunType.ResolvedType = new StringGraphType() { Name = "Animal" };

            var context = new ResolveFieldContext
            {
                Arguments = new Dictionary<string, object>(),
                UserContext = new GraphQLContext {
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
            Assert.Equal("doug", cats.First().As<Animal>().Name);
        }
    }

    public class Animal : ContentPart
    {
        public string Name { get; set; }
    }

    public class AnimalIndex : MapIndex
    {
        public string Name { get; set; }
    }

    public class MultipleIndexProvider : IndexProvider<ContentItem>
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

    public class MultipleIndexAliasProvider : IIndexAliasProvider
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

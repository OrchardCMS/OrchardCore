using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Execution;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Extensions;
using OrchardCore.Json;
using YesSql.Indexes;
using YesSql.Provider.Sqlite;
using YesSql.Serialization;
using YesSql.Sql;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Apis.GraphQL;

public class ContentItemsFieldTypeTests : IAsyncLifetime
{
    protected IStore _store;
    protected IStore _prefixedStore;
    protected string _prefix;
    protected string _tempFilename;

    public async Task InitializeAsync()
    {
        var connectionStringTemplate = @"Data Source={0};Cache=Shared";

        _tempFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _store = await StoreFactory.CreateAndInitializeAsync(new Configuration().UseSqLite(string.Format(connectionStringTemplate, _tempFilename)));

        _prefix = "tp";
        _prefixedStore = await StoreFactory.CreateAndInitializeAsync(new Configuration().UseSqLite(string.Format(connectionStringTemplate, _tempFilename + _prefix)).SetTablePrefix(_prefix + "_"));

        var derivedOptions = new Mock<IOptions<JsonDerivedTypesOptions>>();
        derivedOptions.Setup(x => x.Value)
            .Returns(new JsonDerivedTypesOptions());

        var configuration = new DocumentJsonSerializerOptionsConfiguration(derivedOptions.Object);

        var jsonOptions = new DocumentJsonSerializerOptions();
        configuration.Configure(jsonOptions);

        var jsonSerializerOptions = new Mock<IOptions<DocumentJsonSerializerOptions>>();
        jsonSerializerOptions.Setup(x => x.Value)
            .Returns(jsonOptions);

        var contentSerializer = new DefaultContentJsonSerializer(jsonSerializerOptions.Object);

        _store.Configuration.ContentSerializer = contentSerializer;
        _prefixedStore.Configuration.ContentSerializer = contentSerializer;

        await CreateTablesAsync(_store);
        await CreateTablesAsync(_prefixedStore);
    }

    public Task DisposeAsync()
    {
        _store.Dispose();
        _store = null;

        _prefixedStore.Dispose();
        _prefixedStore = null;

        if (File.Exists(_tempFilename))
        {
            try
            {
                File.Delete(_tempFilename);
            }
            catch
            {

            }
        }

        var prefixFilename = _tempFilename + _prefix;

        if (File.Exists(prefixFilename))
        {
            try
            {
                File.Delete(prefixFilename);
            }
            catch
            {

            }
        }

        return Task.CompletedTask;
    }

    private static async Task CreateTablesAsync(IStore store)
    {
        await using (var session = store.CreateSession())
        {
            var builder = new SchemaBuilder(store.Configuration, await session.BeginTransactionAsync());

            await builder.CreateMapIndexTableAsync<ContentItemIndex>(table => table
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

            await builder.CreateMapIndexTableAsync<AnimalIndex>(table => table
                .Column<string>(nameof(AnimalIndex.Name))
            );

            await builder.CreateMapIndexTableAsync<AnimalTraitsIndex>(table => table
                .Column<bool>(nameof(AnimalTraitsIndex.IsHappy))
                .Column<bool>(nameof(AnimalTraitsIndex.IsScary))
            );

            await session.SaveChangesAsync();
        }

        store.RegisterIndexes<ContentItemIndexProvider>();
    }

    [Fact]
    public async Task ShouldFilterByContentItemIndex()
    {
        _store.RegisterIndexes<AnimalIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => _store.CreateSession());
        services.Services.AddScoped(x => new ShellSettings());
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<ContentItemIndex>>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();
        services.Build();

        var context = CreateAnimalFieldContext(services);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new AnimalPart { Name = "doug" });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal", new Schema(services), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"contentItemId\": \"1\" }"), ArgumentSource.Variable);
        var dogs = await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver)
                        .ResolveAsync(context) as IEnumerable<ContentItem>;

        Assert.Single(dogs);
        Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
    }

    [Fact]
    public async Task ShouldFilterByContentItemIndexWhenSqlTablePrefixIsUsed()
    {
        _prefixedStore.RegisterIndexes<AnimalIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => _prefixedStore.CreateSession());
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<ContentItemIndex>>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalTraitsIndex>>();

        var shellSettings = new ShellSettings();
        shellSettings["TablePrefix"] = _prefix;

        services.Services.AddScoped(x => shellSettings);
        services.Build();

        var context = CreateAnimalFieldContext(services);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new AnimalPart { Name = "doug" });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal", new Schema(services), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"contentItemId\": \"1\" }"), ArgumentSource.Variable);
        var dogs = await ResolveContentItems(type, context);

        Assert.Single(dogs);
        Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
    }

    [Theory]
    [InlineData("animal")]
    [InlineData("ANIMAL")]
    [InlineData("Animal")]
    public async Task ShouldFilterByAliasIndexRegardlessOfInputFieldCase(string fieldName)
    {
        _store.RegisterIndexes<AnimalIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => _store.CreateSession());
        services.Services.AddScoped(x => new ShellSettings());
        services.Services.AddIndexProvider<AnimalIndexProvider>();
        services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();
        services.Build();

        var context = CreateAnimalFieldContext(services, fieldName);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new AnimalPart { Name = "doug" });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal",
            new Schema(services),
            Options.Create(new GraphQLContentOptions()),
            Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse($"{{\"{fieldName}\" : {{ \"name\": \"doug\" }} }}"), ArgumentSource.Variable);
        var dogs = await ResolveContentItems(type, context);

        Assert.Single(dogs);
        Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
    }

    [Fact]
    public async Task ShouldBeAbleToUseTheSameIndexForMultipleAliases()
    {
        _store.RegisterIndexes<AnimalIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => new ShellSettings());
        services.Services.AddScoped(x => _store.CreateSession());
        services.Services.AddIndexProvider<ContentItemIndexProvider>();
        services.Services.AddIndexProvider<AnimalIndexProvider>();
        services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();

        services.Build();
        var context = CreateAnimalFieldContext(services);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new Animal { Name = "doug" });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"cats\": { \"name\": \"doug\" } }"), ArgumentSource.Variable);
        var cats = await ResolveContentItems(type, context);

        Assert.Single(cats);
        Assert.Equal("doug", cats.First().As<Animal>().Name);

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"dogs\": { \"name\": \"doug\" } }"), ArgumentSource.Variable);
        var dogs = await ResolveContentItems(type, context);

        Assert.Single(dogs);
        Assert.Equal("doug", dogs.First().As<Animal>().Name);
    }

    [Fact]
    public async Task ShouldFilterOnMultipleIndexesOnSameAlias()
    {
        _store.RegisterIndexes<AnimalIndexProvider>();
        _store.RegisterIndexes<AnimalTraitsIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => new ShellSettings());
        services.Services.AddScoped(x => _store.CreateSession());
        services.Services.AddIndexProvider<ContentItemIndexProvider>();
        services.Services.AddIndexProvider<AnimalIndexProvider>();
        services.Services.AddIndexProvider<AnimalTraitsIndexProvider>();
        services.Services.AddScoped<IIndexAliasProvider, MultipleIndexesIndexProvider>();

        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalTraitsIndex>>();
        services.Build();

        var context = CreateAnimalFieldContext(services);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new Animal { Name = "doug", IsHappy = true, IsScary = false });

        var ci1 = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "2", ContentItemVersionId = "2" };
        ci1.Weld(new Animal { Name = "doug", IsHappy = false, IsScary = true });

        var ci2 = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "3", ContentItemVersionId = "3" };
        ci2.Weld(new Animal { Name = "tommy", IsHappy = false, IsScary = true });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveAsync(ci1);
        await session.SaveAsync(ci2);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"animals\": { \"name\": \"doug\", \"isScary\": true } }"), ArgumentSource.Variable);
        var animals = await ResolveContentItems(type, context);

        Assert.Single(animals);
        Assert.Equal("doug", animals.First().As<Animal>().Name);
        Assert.True(animals.First().As<Animal>().IsScary);
        Assert.False(animals.First().As<Animal>().IsHappy);
    }

    [Fact]
    public async Task ShouldFilterPartsWithoutAPrefixWhenThePartHasNoPrefix()
    {
        _store.RegisterIndexes<AnimalIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => _store.CreateSession());
        services.Services.AddScoped(x => new ShellSettings());
        services.Services.AddIndexProvider<ContentItemIndexProvider>();
        services.Services.AddIndexProvider<AnimalIndexProvider>();
        services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();
        services.Build();

        var context = CreateAnimalFieldContext(services);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new AnimalPart { Name = "doug" });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"animal\": { \"name\": \"doug\" } }"), ArgumentSource.Variable);
        var dogs = await ResolveContentItems(type, context);

        Assert.Single(dogs);
        Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
    }

    [Fact]
    public async Task ShouldFilterByCollapsedWhereInputForCollapsedParts()
    {
        _store.RegisterIndexes<AnimalIndexProvider>();

        using var services = new FakeServiceCollection();
        services.Populate(new ServiceCollection());
        services.Services.AddScoped(x => new ShellSettings());
        services.Services.AddScoped(x => _store.CreateSession());
        services.Services.AddIndexProvider<ContentItemIndexProvider>();
        services.Services.AddIndexProvider<AnimalIndexProvider>();
        services.Services.AddScoped<IIndexAliasProvider, MultipleAliasIndexProvider>();
        services.Services.AddSingleton<IIndexPropertyProvider, IndexPropertyProvider<AnimalIndex>>();
        services.Build();

        var context = CreateAnimalFieldContext(services, collapsed: true);

        var ci = new ContentItem { ContentType = "Animal", Published = true, ContentItemId = "1", ContentItemVersionId = "1" };
        ci.Weld(new AnimalPart { Name = "doug" });

        var session = context.RequestServices.GetService<ISession>();
        await session.SaveAsync(ci);
        await session.SaveChangesAsync();

        var type = new ContentItemsFieldType("Animal", new Schema(), Options.Create(new GraphQLContentOptions()), Options.Create(new GraphQLSettings { DefaultNumberOfResults = 10 }));

        context.Arguments["where"] = new ArgumentValue(JObject.Parse("{ \"name\": \"doug\" }"), ArgumentSource.Variable);
        var dogs = await ResolveContentItems(type, context);

        Assert.Single(dogs);
        Assert.Equal("doug", dogs.First().As<AnimalPart>().Name);
    }

    private static async Task<IEnumerable<ContentItem>> ResolveContentItems(ContentItemsFieldType type, ResolveFieldContext context)
    {
        return (await ((LockedAsyncFieldResolver<IEnumerable<ContentItem>>)type.Resolver).ResolveAsync(context)) as IEnumerable<ContentItem>;
    }
    private static ResolveFieldContext CreateAnimalFieldContext(IServiceProvider services, string fieldName = null, bool collapsed = false)
    {
        IGraphType where;

        if (!collapsed)
        {
            where = new AnimalPartWhereInput(fieldName ?? "Animal");
        }
        else
        {
            where = new AnimalPartCollapsedWhereInput();
        }

        return new ResolveFieldContext
        {
            Arguments = new Dictionary<string, ArgumentValue>(),
            UserContext = new GraphQLUserContext(),
            FieldDefinition = new FieldType
            {
                Name = "Inputs",
                ResolvedType = new ListGraphType(new StringGraphType() { Name = "Animal" }),
                Arguments = [
                        new QueryArgument<WhereInputObjectGraphType>
                        {
                            Name = "where",
                            Description = "filters the animals",
                            ResolvedType = where
                        }
                    ]
            },
            RequestServices = services
        };
    }
}

public class AnimalPartWhereInput : WhereInputObjectGraphType
{
    public AnimalPartWhereInput(string fieldName)
    {
        Name = "Test";
        Description = "Foo";
        var fieldType = new FieldType { Name = fieldName, Type = typeof(StringGraphType) };
        fieldType.Metadata["PartName"] = "AnimalPart";
        AddField(fieldType);
    }
}

public class AnimalPartCollapsedWhereInput : WhereInputObjectGraphType
{
    public AnimalPartCollapsedWhereInput()
    {
        Name = "Test";
        Description = "Foo";
        var fieldType = new FieldType { Name = "Name", Type = typeof(StringGraphType) };
        fieldType.Metadata["PartName"] = "AnimalPart";
        fieldType.Metadata["PartCollapsed"] = true;
        AddField(fieldType);
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
                    Name = contentItem.As<Animal>() != null
                        ? contentItem.As<Animal>().Name
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
    private static readonly IndexAlias[] _aliases =
    [
        new IndexAlias
        {
            Alias = "cats",
            Index = nameof(AnimalIndex),
            IndexType = typeof(AnimalIndex)
        },
        new IndexAlias
        {
            Alias = "dogs",
            Index = nameof(AnimalIndex),
            IndexType = typeof(AnimalIndex)
        },
        new IndexAlias
        {
            Alias = nameof(AnimalPart),
            Index = nameof(AnimalIndex),
            IndexType = typeof(AnimalIndex)
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(_aliases);
    }
}

public class MultipleIndexesIndexProvider : IIndexAliasProvider
{
    private static readonly IndexAlias[] _aliases =
    [
        new IndexAlias
        {
            Alias = "animals.name",
            Index = $"Name",
            IndexType = typeof(AnimalIndex)
        },
        new IndexAlias
        {
            Alias = "animals.isHappy",
            Index = $"IsHappy",
            IndexType = typeof(AnimalTraitsIndex)
        },
        new IndexAlias
        {
            Alias = "animals.isScary",
            Index = $"IsScary",
            IndexType = typeof(AnimalTraitsIndex)
        }
    ];

    public ValueTask<IEnumerable<IndexAlias>> GetAliasesAsync()
    {
        return ValueTask.FromResult<IEnumerable<IndexAlias>>(_aliases);
    }
}

public class FakeServiceCollection : IServiceProvider, IDisposable
{
    private ServiceProvider _inner;
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
        _services.AddSingleton(this);
    }

    public void Build()
    {
        _inner = _services.BuildServiceProvider();
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        (_inner as IDisposable)?.Dispose();
    }
}

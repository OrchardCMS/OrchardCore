using System.Data.Common;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using ISession = YesSql.ISession;

namespace OrchardCore.Data.Migration.Tests;

public class DataMigrationManagerTests
{
    [Fact]
    public async Task UpdateAsync_ShouldExecuteDataMigration_CreateMethod_OnFreshMigration()
    {
        // Arrange
        var migration1 = new Migration1();
        var migration2 = new Migration2();
        var migrationManager = GetDataMigrationManager([migration1, migration2]);

        // Act
        await migrationManager.UpdateAsync("TestFeature");

        // Assert
        Assert.True(migration1.CreateCalled);
        Assert.True(migration2.CreateCalled);
    }

    [Fact]
    public async Task UpdateAsync_ShouldExecuteDataMigration_UpdateFromMethods()
    {
        // Arrange
        var migration1 = new Migration1();
        var migration2 = new Migration2();
        var migrationManager = GetDataMigrationManager([migration1, migration2]);

        // Act
        await migrationManager.UpdateAsync("TestFeature");

        // Assert
        Assert.Equal(2, migration1.UpdateFromCalls);
        Assert.Equal(0, migration2.UpdateFromCalls);
    }

    [Fact]
    public async Task Uninstall_ShouldExecuteDataMigration_UninstallMethod()
    {
        // Arrange
        var migration1 = new Migration1();
        var migration2 = new Migration2();
        var migrationManager = GetDataMigrationManager([migration1, migration2]);

        // Act
        await migrationManager.Uninstall("TestFeature");

        // Assert
        Assert.True(migration1.UninstallCalled);
        Assert.True(migration2.UninstallCalled);
    }

    private static DataMigrationManager GetDataMigrationManager(IEnumerable<DataMigration> dataMigrations)
    {
        var featureInfo = new Mock<IFeatureInfo>();
        featureInfo.Setup(f => f.Id).Returns("TestFeature");

        var typeFeatureProviderMock = new Mock<ITypeFeatureProvider>();
        typeFeatureProviderMock.Setup(m => m.GetFeatureForDependency(It.IsAny<Type>()))
            .Returns(featureInfo.Object);

        var extensionManagerMock = new Mock<IExtensionManager>();
        extensionManagerMock.Setup(m => m.GetFeatureDependencies(It.IsAny<string>()))
            .Returns(Enumerable.Empty<IFeatureInfo>());

        var sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.BeginTransactionAsync())
            .ReturnsAsync(Mock.Of<DbTransaction>());

        sessionMock.Setup(s => s.Query())
            .Returns(new FakeQuery());

        sessionMock.Setup(s => s.SaveAsync(It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        var storeMock = new Mock<IStore>();
        storeMock.Setup(s => s.Configuration).Returns(new Configuration());

        return new DataMigrationManager(
            typeFeatureProviderMock.Object,
            dataMigrations,
            sessionMock.Object,
            storeMock.Object,
            extensionManagerMock.Object,
            NullLogger<DataMigrationManager>.Instance);
    }

    private sealed class Migration1 : DataMigration
    {
        public bool CreateCalled { get; private set; }

        public bool UninstallCalled { get; private set; }

        public int UpdateFromCalls { get; private set; }

        public int Create()
        {
            CreateCalled = true;

            return 1;
        }

        public int UpdateFrom1()
        {
            ++UpdateFromCalls;

            return 2;
        }

        public Task<int> UpdateFrom2Async()
        {
            ++UpdateFromCalls;

            return Task.FromResult(3);
        }

#pragma warning disable CA1822 // Mark members as static
        public int UpdateFromInvalid() => 0;
#pragma warning restore CA1822 // Mark members as static

        public void Uninstall() => UninstallCalled = true;
    }

    private sealed class Migration2 : DataMigration
    {
        public bool CreateCalled { get; private set; }

        public bool UninstallCalled { get; private set; }

        public int UpdateFromCalls { get; private set; }

        public Task<int> CreateAsync()
        {
            CreateCalled = true;

            return Task.FromResult(1);
        }

        public Task UninstallAsync()
        {
            UninstallCalled = true;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeQuery : IQuery
    {
        public IQuery<object> Any()
            => throw new NotImplementedException();

        public IQuery<T> For<T>(bool filterType = true) where T : class => new FakeQuery<T>();

        IQueryIndex<T> IQuery.ForIndex<T>()
            => throw new NotImplementedException();
    }

    private sealed class FakeQuery<T> : IQuery<T> where T : class
    {
        public IQuery<T> All(params Func<IQuery<T>, IQuery<T>>[] predicates)
            => throw new NotImplementedException();

        public ValueTask<IQuery<T>> AllAsync(params Func<IQuery<T>, ValueTask<IQuery<T>>>[] predicates)
            => throw new NotImplementedException();

        public IQuery<T> Any(params Func<IQuery<T>, IQuery<T>>[] predicates)
            => throw new NotImplementedException();

        public ValueTask<IQuery<T>> AnyAsync(params Func<IQuery<T>, ValueTask<IQuery<T>>>[] predicates)
            => throw new NotImplementedException();

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<int> CountAsync()
            => throw new NotImplementedException();

        public Task<T> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<T> FirstOrDefaultAsync() => Task.FromResult((T)null);

        public string GetTypeAlias(Type t)
            => throw new NotImplementedException();

        public Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<T>> ListAsync()
            => throw new NotImplementedException();

        public IQuery<T> NoDuplicates()
            => throw new NotImplementedException();

        public IQuery<T> Skip(int count)
            => throw new NotImplementedException();

        public IQuery<T> Take(int count)
            => throw new NotImplementedException();

        public IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public IAsyncEnumerable<T> ToAsyncEnumerable()
            => throw new NotImplementedException();

        public IQuery<T> With(Type indexType)
            => throw new NotImplementedException();

        IQuery<T, TIndex> IQuery<T>.With<TIndex>()
            => throw new NotImplementedException();

        IQuery<T, TIndex> IQuery<T>.With<TIndex>(System.Linq.Expressions.Expression<Func<TIndex, bool>> predicate)
            => throw new NotImplementedException();
    }
}

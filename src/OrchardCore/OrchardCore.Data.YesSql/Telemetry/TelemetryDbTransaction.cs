using System.Data;
using System.Data.Common;

namespace OrchardCore.Data.YesSql.Telemetry;

/// <summary>
/// Wraps the transaction of a database provider, so that it refers to the instrumented connection, and that the
/// commands it's assigned to use the transaction of the provider.
/// </summary>
internal sealed class TelemetryDbTransaction : DbTransaction
{
    private readonly TelemetryDbConnection _connection;

    public TelemetryDbTransaction(DbTransaction transaction, TelemetryDbConnection connection)
    {
        InnerTransaction = transaction;
        _connection = connection;
    }

    public DbTransaction InnerTransaction { get; }

    public override IsolationLevel IsolationLevel => InnerTransaction.IsolationLevel;

    public override bool SupportsSavepoints => InnerTransaction.SupportsSavepoints;

    protected override DbConnection DbConnection => _connection;

    public override void Commit() => InnerTransaction.Commit();

    public override Task CommitAsync(CancellationToken cancellationToken = default)
    {
        return InnerTransaction.CommitAsync(cancellationToken);
    }

    public override void Rollback() => InnerTransaction.Rollback();

    public override Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        return InnerTransaction.RollbackAsync(cancellationToken);
    }

    public override void Save(string savepointName) => InnerTransaction.Save(savepointName);

    public override Task SaveAsync(string savepointName, CancellationToken cancellationToken = default)
    {
        return InnerTransaction.SaveAsync(savepointName, cancellationToken);
    }

    public override void Rollback(string savepointName) => InnerTransaction.Rollback(savepointName);

    public override Task RollbackAsync(string savepointName, CancellationToken cancellationToken = default)
    {
        return InnerTransaction.RollbackAsync(savepointName, cancellationToken);
    }

    public override void Release(string savepointName) => InnerTransaction.Release(savepointName);

    public override Task ReleaseAsync(string savepointName, CancellationToken cancellationToken = default)
    {
        return InnerTransaction.ReleaseAsync(savepointName, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            InnerTransaction.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await InnerTransaction.DisposeAsync();
        await base.DisposeAsync();
    }
}

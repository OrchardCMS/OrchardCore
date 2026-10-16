using System.Data;
using System.Data.Common;

namespace OrchardCore.Data.YesSql.Telemetry;

/// <summary>
/// Wraps the command of a database provider to trace and measure its executions.
/// </summary>
internal sealed class TelemetryDbCommand : DbCommand
{
    private readonly DbCommand _command;
    private TelemetryDbConnection _connection;
    private TelemetryDbTransaction _transaction;

    public TelemetryDbCommand(DbCommand command, TelemetryDbConnection connection)
    {
        _command = command;
        _connection = connection;
    }

    public override string CommandText
    {
        get => _command.CommandText;
        set => _command.CommandText = value;
    }

    public override int CommandTimeout
    {
        get => _command.CommandTimeout;
        set => _command.CommandTimeout = value;
    }

    public override CommandType CommandType
    {
        get => _command.CommandType;
        set => _command.CommandType = value;
    }

    public override bool DesignTimeVisible
    {
        get => _command.DesignTimeVisible;
        set => _command.DesignTimeVisible = value;
    }

    public override UpdateRowSource UpdatedRowSource
    {
        get => _command.UpdatedRowSource;
        set => _command.UpdatedRowSource = value;
    }

    protected override DbConnection DbConnection
    {
        get => _connection;
        set
        {
            // A connection that isn't instrumented is used as it is.
            _connection = value as TelemetryDbConnection;
            _command.Connection = value?.Unwrap();
        }
    }

    protected override DbParameterCollection DbParameterCollection => _command.Parameters;

    protected override DbTransaction DbTransaction
    {
        get => (DbTransaction)_transaction ?? _command.Transaction;
        set
        {
            _transaction = value as TelemetryDbTransaction;
            _command.Transaction = _transaction?.InnerTransaction ?? value;
        }
    }

    public override void Cancel() => _command.Cancel();

    public override void Prepare() => _command.Prepare();

    public override Task PrepareAsync(CancellationToken cancellationToken = default)
    {
        return _command.PrepareAsync(cancellationToken);
    }

    protected override DbParameter CreateDbParameter() => _command.CreateParameter();

    public override int ExecuteNonQuery()
    {
        return _connection is null
            ? _command.ExecuteNonQuery()
            : DatabaseTelemetry.Execute(_connection.Context, this, _command.ExecuteNonQuery);
    }

    public override object ExecuteScalar()
    {
        return _connection is null
            ? _command.ExecuteScalar()
            : DatabaseTelemetry.Execute(_connection.Context, this, _command.ExecuteScalar);
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        return _connection is null
            ? _command.ExecuteReader(behavior)
            : DatabaseTelemetry.Execute(_connection.Context, this, () => _command.ExecuteReader(behavior));
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return _connection is null
            ? _command.ExecuteNonQueryAsync(cancellationToken)
            : DatabaseTelemetry.ExecuteAsync(_connection.Context, this, () => _command.ExecuteNonQueryAsync(cancellationToken));
    }

    public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return _connection is null
            ? _command.ExecuteScalarAsync(cancellationToken)
            : DatabaseTelemetry.ExecuteAsync(_connection.Context, this, () => _command.ExecuteScalarAsync(cancellationToken));
    }

    protected override Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
    {
        // The duration is the one of the execution, until the first results are available, as for the providers'
        // own instrumentations.
        return _connection is null
            ? _command.ExecuteReaderAsync(behavior, cancellationToken)
            : DatabaseTelemetry.ExecuteAsync(_connection.Context, this, () => _command.ExecuteReaderAsync(behavior, cancellationToken));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _command.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _command.DisposeAsync();
        await base.DisposeAsync();
    }
}
